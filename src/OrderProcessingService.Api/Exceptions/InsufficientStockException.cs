namespace OrderProcessingService.Exceptions;

public class InsufficientStockException : Exception
{
    public string ProductId { get; }
    public int Available { get; }
    public int Requested { get; }

    public InsufficientStockException(string productId, int available, int requested)
        : base($"Insufficient stock for product {productId}. Available: {available}, Requested: {requested}")
    {
        ProductId = productId;
        Available = available;
        Requested = requested;
    }
}
