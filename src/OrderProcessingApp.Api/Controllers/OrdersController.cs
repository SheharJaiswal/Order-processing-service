using Microsoft.AspNetCore.Mvc;
using OrderProcessingService.Interfaces;
using OrderProcessingService.Dtos;
using OrderProcessingService.Exceptions;

namespace OrderProcessingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("POST /api/orders - Creating order for customer {CustomerId}", request.CustomerId);

        try
        {
            var order = await _orderService.CreateOrderAsync(request);

            _logger.LogInformation("Order {OrderNumber} created successfully", order.OrderNumber);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                order
            );
        }
        catch (ProductNotFoundException ex)
        {
            _logger.LogWarning("Product not found: {Message}", ex.Message);
            return BadRequest(new
            {
                error = ex.Message,
                code = "PRODUCT_NOT_FOUND",
                timestamp = DateTime.UtcNow
            });
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning("Insufficient stock: {Message}", ex.Message);
            return Conflict(new
            {
                error = ex.Message,
                code = "INSUFFICIENT_STOCK",
                productId = ex.ProductId,
                available = ex.Available,
                requested = ex.Requested,
                timestamp = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error: {Message}", ex.Message);
            return BadRequest(new
            {
                error = ex.Message,
                code = "VALIDATION_ERROR",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new
            {
                error = "Internal server error occurred while creating order",
                code = "INTERNAL_ERROR",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(string id)
    {
        _logger.LogInformation("GET /api/orders/{OrderId} - Fetching order", id);

        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return NotFound(new
            {
                error = ex.Message,
                code = "ORDER_NOT_FOUND",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Transition order status (includes cancellation)
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionOrderStatus(string id, [FromBody] StatusTransitionRequest request)
    {
        _logger.LogInformation("PATCH /api/orders/{OrderId}/status - Transitioning to {NewStatus}", id, request.NewStatus);

        try
        {
            var order = await _orderService.TransitionOrderStatusAsync(id, request);
            return Ok(order);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return NotFound(new
            {
                error = ex.Message,
                code = "ORDER_NOT_FOUND",
                timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOrderStatusTransitionException ex)
        {
            _logger.LogWarning("Invalid status transition: {Message}", ex.Message);
            return BadRequest(new
            {
                error = ex.Message,
                code = "INVALID_STATUS_TRANSITION",
                currentStatus = ex.CurrentStatus.ToString(),
                requestedStatus = ex.RequestedStatus.ToString(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning order status");
            return StatusCode(500, new
            {
                error = "Internal server error occurred while updating order status",
                code = "INTERNAL_ERROR",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
