using MongoDB.Driver;
using OrderProcessingService.Interfaces;
using OrderProcessingService.Entities;
using OrderProcessingService.Dtos;
using OrderProcessingService.Data;
using OrderProcessingService.Exceptions;

namespace OrderProcessingService.Services;

public class OrderService : IOrderService
{
    private readonly MongoContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cacheService;
    private readonly ILogger<OrderService> _logger;
    private const string OrderCacheKeyPrefix = "order:";
    private readonly TimeSpan _orderCacheExpiration = TimeSpan.FromHours(1);

    public OrderService(
        MongoContext context,
        IInventoryService inventoryService,
        IEventPublisher eventPublisher,
        ICacheService cacheService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _inventoryService = inventoryService;
        _eventPublisher = eventPublisher;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);

        // Validate that items exist and have quantity > 0
        if (!request.Items.Any())
        {
            throw new ArgumentException("Order must contain at least one item");
        }

        // Fetch product details and validate
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new ProductNotFoundException(item.ProductId);
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException($"Quantity for product {item.ProductId} must be greater than 0");
            }

            // Reserve stock atomically (will throw if insufficient)
            await _inventoryService.ReserveStockAsync(item.ProductId, item.Quantity);

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                SubTotal = product.Price * item.Quantity
            };

            orderItems.Add(orderItem);
            totalAmount += orderItem.SubTotal;
        }

        // Create order
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = request.CustomerId,
            Items = orderItems,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            // Persist order
            await _context.Orders.InsertOneAsync(order);
            _logger.LogInformation("Order {OrderNumber} created successfully with ID {OrderId}", order.OrderNumber, order.Id);

            // Invalidate product cache since stock changed
            await InvalidateProductCacheAsync();

            // Publish event
            await _eventPublisher.PublishOrderCreatedEventAsync(order);

            // Cache the order
            await _cacheService.SetAsync($"{OrderCacheKeyPrefix}{order.Id}", order, _orderCacheExpiration);

            return order;
        }
        catch (Exception ex)
        {
            // Rollback stock reservation if order creation fails
            _logger.LogError(ex, "Failed to create order - rolling back stock reservations");

            foreach (var item in orderItems)
            {
                try
                {
                    await _inventoryService.ReleaseStockAsync(item.ProductId, item.Quantity);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Failed to rollback stock for product {ProductId}", item.ProductId);
                }
            }

            throw;
        }
    }

    public async Task<Order> GetOrderByIdAsync(string orderId)
    {
        var cacheKey = $"{OrderCacheKeyPrefix}{orderId}";

        // Try cache first
        var cachedOrder = await _cacheService.GetAsync<Order>(cacheKey);
        if (cachedOrder != null)
        {
            _logger.LogDebug("Returning order {OrderId} from cache", orderId);
            return cachedOrder;
        }

        // Fetch from database
        var order = await _context.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();

        if (order == null)
        {
            throw new OrderNotFoundException(orderId);
        }

        // Cache it
        await _cacheService.SetAsync(cacheKey, order, _orderCacheExpiration);

        return order;
    }

    public async Task<Order> TransitionOrderStatusAsync(string orderId, StatusTransitionRequest request)
    {
        _logger.LogInformation("Transitioning order {OrderId} to status {NewStatus}", orderId, request.NewStatus);

        var order = await _context.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();

        if (order == null)
        {
            throw new OrderNotFoundException(orderId);
        }

        var oldStatus = order.Status;

        // Validate transition
        StatusTransitionValidator.ValidateTransition(oldStatus, request.NewStatus);

        // Update order status
        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;

        // Handle cancellation - release stock
        if (request.NewStatus == OrderStatus.Cancelled)
        {
            order.CancelledAt = DateTime.UtcNow;
            order.CancelReason = request.CancelReason ?? "No reason provided";

            _logger.LogInformation("Cancelling order {OrderId} - releasing stock", orderId);

            foreach (var item in order.Items)
            {
                await _inventoryService.ReleaseStockAsync(item.ProductId, item.Quantity);
            }

            // Invalidate product cache since stock changed
            await InvalidateProductCacheAsync();
        }

        // Persist changes
        var filter = Builders<Order>.Filter.Eq(o => o.Id, orderId);
        await _context.Orders.ReplaceOneAsync(filter, order);

        // Invalidate order cache
        await _cacheService.RemoveAsync($"{OrderCacheKeyPrefix}{orderId}");

        // Publish appropriate event
        if (request.NewStatus == OrderStatus.Cancelled)
        {
            await _eventPublisher.PublishOrderCancelledEventAsync(orderId, order.CustomerId, order.CancelReason ?? "");
        }
        else
        {
            await _eventPublisher.PublishOrderStatusChangedEventAsync(orderId, oldStatus, request.NewStatus);
        }

        _logger.LogInformation("Order {OrderId} transitioned from {OldStatus} to {NewStatus}", orderId, oldStatus, request.NewStatus);

        return order;
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private async Task InvalidateProductCacheAsync()
    {
        await _cacheService.RemoveAsync("products:all");
        await _cacheService.RemoveByPatternAsync("product:*");
    }
}
