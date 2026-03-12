using OrderProcessingApp.Api.Entities;

namespace OrderProcessingApp.Api.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product> GetProductByIdAsync(string productId);
}
