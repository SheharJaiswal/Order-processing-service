using Microsoft.AspNetCore.Mvc;
using OrderProcessingService.Exceptions;
using OrderProcessingService.Interfaces;

namespace OrderProcessingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with current stock levels
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProducts()
    {
        _logger.LogInformation("GET /api/products - Fetching all products");

        var products = await _productService.GetAllProductsAsync();

        return Ok(products);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(string id)
    {
        _logger.LogInformation("GET /api/products/{ProductId} - Fetching product", id);

        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(product);
        }
        catch (ProductNotFoundException ex)
        {
            _logger.LogWarning("Product {ProductId} not found", id);
            return NotFound(new { error = ex.Message, code = "PRODUCT_NOT_FOUND" });
        }
    }
}
