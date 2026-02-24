using Microsoft.AspNetCore.Mvc;
using StyleVerse.Backend.Models;
using StyleVerse.Backend.Services;

namespace StyleVerse.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartsController : ControllerBase
{
    private readonly CosmosDbService _cosmos;
    private readonly ILogger<CartsController> _logger;

    public CartsController(CosmosDbService cosmos, ILogger<CartsController> logger)
    {
        _cosmos = cosmos;
        _logger = logger;
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<IEnumerable<CartItemResponse>>> GetCart(string sessionId)
    {
        var items = await _cosmos.GetCartAsync(sessionId);
        return Ok(items.Select(CartItemResponse.FromCosmos));
    }

    [HttpPost]
    public async Task<ActionResult<CartItemResponse>> AddToCart([FromBody] AddToCartRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
            return BadRequest("SessionId is required.");

        var item = await _cosmos.AddToCartAsync(request);
        return Ok(CartItemResponse.FromCosmos(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCart(string id, [FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return BadRequest("sessionId query parameter is required");

        var deleted = await _cosmos.RemoveFromCartAsync(id, sessionId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
