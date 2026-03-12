namespace OrderProcessingApp.Api.Events;

public class OrderCancelledEvent : OrderEvent
{
    public OrderCancelledEvent()
    {
        EventType = "OrderCancelled";
    }

    public string Reason { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
}
