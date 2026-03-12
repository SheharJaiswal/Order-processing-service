using OrderProcessingApp.Api.Entities;

namespace OrderProcessingApp.Api.Events;

public class OrderCreatedEvent : OrderEvent
{
    public OrderCreatedEvent()
    {
        EventType = "OrderCreated";
    }

    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
}

public class OrderItemEvent
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
