using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleVerse.Backend.Data;
using StyleVerse.Backend.Models;

namespace StyleVerse.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            // Simulating "Legacy" latency (optional helper)
            // await Task.Delay(100); 
            return await _context.Products.ToListAsync();
        }
    }
}