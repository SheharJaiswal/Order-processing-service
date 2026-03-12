using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrderProcessingService.Entities;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [BsonElement("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<OrderItem> Items { get; set; } = new();

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; }

    [BsonElement("totalAmount")]
    public decimal TotalAmount { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [BsonElement("cancelReason")]
    public string? CancelReason { get; set; }
}
