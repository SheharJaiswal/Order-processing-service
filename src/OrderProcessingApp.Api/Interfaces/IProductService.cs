using OrderProcessingService.Entities;

namespace OrderProcessingService.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product> GetProductByIdAsync(string productId);
}
