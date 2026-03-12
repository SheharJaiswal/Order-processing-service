namespace OrderProcessingService.Exceptions;

public class OrderNotFoundException : Exception
{
    public string OrderId { get; }

    public OrderNotFoundException(string orderId)
        : base($"Order with ID {orderId} not found")
    {
        OrderId = orderId;
    }
}
