using MongoDB.Driver;
using OrderProcessingApp.Api.Data;
using OrderProcessingApp.Api.Exceptions;
using OrderProcessingApp.Api.Entities;
using OrderProcessingApp.Api.Interfaces;

namespace OrderProcessingApp.Api.Services;

public class ProductService : IProductService
{
    private readonly MongoContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductService> _logger;
    private const string ProductsListCacheKey = "products:all";
    private const string ProductCacheKeyPrefix = "product:";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public ProductService(MongoContext context, ICacheService cacheService, ILogger<ProductService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        // Try to get from cache first
        var cachedProducts = await _cacheService.GetAsync<List<Product>>(ProductsListCacheKey);
        
        if (cachedProducts != null)
        {
            _logger.LogDebug("Returning products from cache");
            return cachedProducts;
        }

        // Cache miss - fetch from database
        _logger.LogDebug("Cache miss - fetching products from database");
        var products = await _context.Products.Find(_ => true).ToListAsync();

        // Cache the result
        await _cacheService.SetAsync(ProductsListCacheKey, products, _cacheExpiration);

        return products;
    }

    public async Task<Product> GetProductByIdAsync(string productId)
    {
        var cacheKey = $"{ProductCacheKeyPrefix}{productId}";
        
        // Try to get from cache first
        var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey);
        
        if (cachedProduct != null)
        {
            _logger.LogDebug("Returning product {ProductId} from cache", productId);
            return cachedProduct;
        }

        // Cache miss - fetch from database
        _logger.LogDebug("Cache miss - fetching product {ProductId} from database", productId);
        var product = await _context.Products.Find(p => p.Id == productId).FirstOrDefaultAsync();

        if (product == null)
        {
            throw new ProductNotFoundException(productId);
        }

        // Cache the result
        await _cacheService.SetAsync(cacheKey, product, _cacheExpiration);

        return product;
    }
}
