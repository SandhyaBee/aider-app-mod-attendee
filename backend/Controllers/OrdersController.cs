using Microsoft.AspNetCore.Mvc;
using StyleVerse.Backend.Models;
using StyleVerse.Backend.Services;
using StyleVerse.Backend.Common.Security.System.Win;

namespace StyleVerse.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly CosmosDbService _cosmos;

    public OrdersController(CosmosDbService cosmos)
    {
        _cosmos = cosmos;
    }

    [HttpGet]
    public async Task<ActionResult> GetOrders()
    {
        var orders = await _cosmos.GetOrdersAsync();

        if (!SecurityCheck.OrderSecurityCheck(orders))
        {
            return Forbid("Security check failed for orders.");
        }

        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var order = await _cosmos.PlaceOrderAsync(request);
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}
