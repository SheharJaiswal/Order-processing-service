namespace OrderProcessingService.Interfaces;

public interface IInventoryService
{
    Task ReserveStockAsync(string productId, int quantity);
    Task ReleaseStockAsync(string productId, int quantity);
    Task<int> GetProductStockAsync(string productId);
}
