using MongoDB.Driver;
using OrderProcessingApp.Api.Entities;

namespace OrderProcessingApp.Api.Data;

public class DataSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IServiceProvider serviceProvider, ILogger<DataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mongoContext = scope.ServiceProvider.GetRequiredService<MongoContext>();

        await SeedProductsAsync(mongoContext);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedProductsAsync(MongoContext context)
    {
        try
        {
            var count = await context.Products.CountDocumentsAsync(FilterDefinition<Product>.Empty);
            
            if (count > 0)
            {
                _logger.LogInformation("Products already seeded. Skipping seed operation.");
                return;
            }

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Laptop",
                    Description = "High-performance laptop with 16GB RAM",
                    Price = 999.99m,
                    Stock = 10,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse",
                    Price = 29.99m,
                    Stock = 100,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Mechanical Keyboard",
                    Description = "RGB mechanical keyboard with Cherry MX switches",
                    Price = 79.99m,
                    Stock = 50,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "27-inch Monitor",
                    Description = "4K UHD monitor with HDR support",
                    Price = 299.99m,
                    Stock = 15,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "USB-C Cable",
                    Description = "2-meter USB-C charging cable",
                    Price = 9.99m,
                    Stock = 200,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Webcam",
                    Description = "1080p HD webcam with built-in microphone",
                    Price = 49.99m,
                    Stock = 30,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Products.InsertManyAsync(products);
            _logger.LogInformation("Successfully seeded {Count} products", products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding products");
        }
    }
}
