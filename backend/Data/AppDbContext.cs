using Microsoft.EntityFrameworkCore;
using StyleVerse.Backend.Models;

namespace StyleVerse.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial data
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Nebula Sneakers", Price = 120.50m, Category = "Footwear", ImageUrl = "/images/nebula.png" },
                new Product { Id = 2, Name = "Cyberpunk Hoodie", Price = 85.00m, Category = "Apparel", ImageUrl = "/images/hoodie.png" },
                new Product { Id = 3, Name = "Retro Visor", Price = 45.00m, Category = "Accessories", ImageUrl = "/images/visor.png" }
            );
        }
    }
}