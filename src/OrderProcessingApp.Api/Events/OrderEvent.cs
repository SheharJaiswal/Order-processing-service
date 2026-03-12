namespace OrderProcessingApp.Api.Events;

public abstract class OrderEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string OrderId { get; set; } = string.Empty;
}
