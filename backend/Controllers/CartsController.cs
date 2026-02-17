using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleVerse.Backend.Data;
using StyleVerse.Backend.Models;

namespace StyleVerse.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{sessionId}")]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCart(string sessionId)
        {
            return await _context.CartItems
                .Where(c => c.SessionId == sessionId)
                .Include(c => c.Product)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CartItem>> AddToCart(CartItem item)
        {
            var existing = await _context.CartItems
                .FirstOrDefaultAsync(c => c.SessionId == item.SessionId && c.ProductId == item.ProductId);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                _context.CartItems.Add(item);
            }

            await _context.SaveChangesAsync();
            return Ok(item);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
             var item = await _context.CartItems.FindAsync(id);
             if (item == null) return NotFound();

             _context.CartItems.Remove(item);
             await _context.SaveChangesAsync();
             return NoContent();
        }
    }
}