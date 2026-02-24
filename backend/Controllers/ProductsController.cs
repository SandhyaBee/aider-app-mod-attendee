using Microsoft.AspNetCore.Mvc;
using StyleVerse.Backend.Models;
using StyleVerse.Backend.Services;

namespace StyleVerse.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly CosmosDbService _cosmos;

    public ProductsController(CosmosDbService cosmos)
    {
        _cosmos = cosmos;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
    {
        var products = await _cosmos.GetProductsAsync();
        return Ok(products.Select(ProductResponse.FromCosmos));
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
        return Ok(ProductResponse.FromCosmos(result));
    }
}
