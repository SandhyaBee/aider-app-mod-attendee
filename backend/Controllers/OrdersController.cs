using Microsoft.AspNetCore.Mvc;
using StyleVerse.Backend.Models;
using StyleVerse.Backend.Services;

namespace StyleVerse.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly CosmosDbService _cosmos;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(CosmosDbService cosmos, ILogger<OrdersController> logger)
    {
        _cosmos = cosmos;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetOrders()
    {
        var orders = await _cosmos.GetOrdersAsync();
        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName) || string.IsNullOrEmpty(request.Email))
            return BadRequest("CustomerName and Email are required.");

        var order = await _cosmos.PlaceOrderAsync(request);
        _logger.LogInformation("Order {OrderId} placed for {Customer}", order.Id, order.CustomerName);
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}
