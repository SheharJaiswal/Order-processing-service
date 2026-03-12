using MongoDB.Driver;
using OrderProcessingService.Entities;

namespace OrderProcessingService.Data;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"] ?? "mongodb://mongodb:27017";
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "order_processing_db";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);

        // Create indexes
        CreateIndexes();
    }

    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");

    private void CreateIndexes()
    {
        // Create unique index on OrderNumber
        var orderIndexKeys = Builders<Order>.IndexKeys.Ascending(o => o.OrderNumber);
        var orderIndexOptions = new CreateIndexOptions { Unique = true };
        var orderIndexModel = new CreateIndexModel<Order>(orderIndexKeys, orderIndexOptions);
        Orders.Indexes.CreateOne(orderIndexModel);

        // Create index on CustomerId for better query performance
        var customerIndexKeys = Builders<Order>.IndexKeys.Ascending(o => o.CustomerId);
        var customerIndexModel = new CreateIndexModel<Order>(customerIndexKeys);
        Orders.Indexes.CreateOne(customerIndexModel);

        // Create index on Status
        var statusIndexKeys = Builders<Order>.IndexKeys.Ascending(o => o.Status);
        var statusIndexModel = new CreateIndexModel<Order>(statusIndexKeys);
        Orders.Indexes.CreateOne(statusIndexModel);

        // Create index on Product Name
        var productNameIndexKeys = Builders<Product>.IndexKeys.Ascending(p => p.Name);
        var productNameIndexModel = new CreateIndexModel<Product>(productNameIndexKeys);
        Products.Indexes.CreateOne(productNameIndexModel);
    }
}
