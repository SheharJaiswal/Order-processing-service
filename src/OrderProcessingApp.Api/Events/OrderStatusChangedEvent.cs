using OrderProcessingApp.Api.Entities;

namespace OrderProcessingApp.Api.Events;

public class OrderStatusChangedEvent : OrderEvent
{
    public OrderStatusChangedEvent()
    {
        EventType = "OrderStatusChanged";
    }

    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
}
