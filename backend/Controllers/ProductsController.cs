using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StyleVerse.Backend.Models;
using StyleVerse.Backend.Services;

namespace StyleVerse.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly CosmosDbService _cosmos;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductsController> _logger;
    private const string ProductsCacheKey = "all_products";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public ProductsController(CosmosDbService cosmos, IMemoryCache cache, ILogger<ProductsController> logger)
    {
        _cosmos = cosmos;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 10)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
    {
        var responses = await _cache.GetOrCreateAsync(ProductsCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var products = await _cosmos.GetProductsAsync();
            return products.Select(ProductResponse.FromCosmos).ToList();
        });
        return Ok(responses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(string id)
    {
        var product = await _cosmos.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(ProductResponse.FromCosmos(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> UpsertProduct([FromBody] Product product)
    {
        var result = await _cosmos.UpsertProductAsync(product);
        _cache.Remove(ProductsCacheKey);
        return Ok(ProductResponse.FromCosmos(result));
    }
}
