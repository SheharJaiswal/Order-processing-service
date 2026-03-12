using MongoDB.Driver;
using OrderProcessingService.Data;
using OrderProcessingService.Entities;
using OrderProcessingService.Exceptions;
using OrderProcessingService.Interfaces;

namespace OrderProcessingService.Services;

public class InventoryService : IInventoryService
{
    private readonly MongoContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(MongoContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ReserveStockAsync(string productId, int quantity)
    {
        _logger.LogInformation("Reserving {Quantity} units of product {ProductId}", quantity, productId);

        // Atomic operation: Check stock AND decrement in a single operation
        // This prevents race conditions where multiple orders compete for the same stock
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Eq(p => p.Id, productId),
            Builders<Product>.Filter.Gte(p => p.Stock, quantity)
        );

        var update = Builders<Product>.Update
            .Inc(p => p.Stock, -quantity)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<Product>
        {
            ReturnDocument = ReturnDocument.After
        };

        var result = await _context.Products.FindOneAndUpdateAsync(filter, update, options);

        if (result == null)
        {
            // Either product doesn't exist or insufficient stock
            var product = await _context.Products.Find(p => p.Id == productId).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new ProductNotFoundException(productId);
            }

            throw new InsufficientStockException(productId, product.Stock, quantity);
        }

        _logger.LogInformation("Successfully reserved {Quantity} units of product {ProductId}. Remaining stock: {Stock}",
            quantity, productId, result.Stock);
    }

    public async Task ReleaseStockAsync(string productId, int quantity)
    {
        _logger.LogInformation("Releasing {Quantity} units of product {ProductId}", quantity, productId);

        var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
        var update = Builders<Product>.Update
            .Inc(p => p.Stock, quantity)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Products.FindOneAndUpdateAsync(filter, update);

        if (result == null)
        {
            throw new ProductNotFoundException(productId);
        }

        _logger.LogInformation("Successfully released {Quantity} units of product {ProductId}", quantity, productId);
    }

    public async Task<int> GetProductStockAsync(string productId)
    {
        var product = await _context.Products.Find(p => p.Id == productId).FirstOrDefaultAsync();

        if (product == null)
        {
            throw new ProductNotFoundException(productId);
        }

        return product.Stock;
    }
}
