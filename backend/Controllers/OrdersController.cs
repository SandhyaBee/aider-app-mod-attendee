using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleVerse.Backend.Data;
using StyleVerse.Backend.Models;
using StyleVerse.Backend.Common.Security.System.Win;

namespace StyleVerse.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult> GetOrders()
    {
        var orders = await _context.Orders.Include(o => o.OrderItems).ToListAsync();

        // Perform Security Check
        if (!SecurityCheck.OrderSecurityCheck(orders))
        {
            return Forbid("Security check failed for orders.");
        }

        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult> PlaceOrder(Order order)
    {
        // In the legacy system, we calculate the total synchronously 
        // This is a bottleneck we will address in later phases.

        // 1. Save the Order first
        _context.Orders.Add(order);
        
        // 2. Find the items in the cart for this session
        // In our current React app, we use SessionId to identify the user
        var cartItems = await _context.CartItems
            .Where(c => c.SessionId == order.CustomerName) // Using CustomerName as the link for now
            .ToListAsync();

        // 3. Clear the cart
        if (cartItems.Any())
        {
            _context.CartItems.RemoveRange(cartItems);
        }

        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}