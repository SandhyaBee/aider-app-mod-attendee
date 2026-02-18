using Microsoft.EntityFrameworkCore;
using StyleVerse.Backend.Models;

namespace StyleVerse.Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Precision for Money
        modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(10,2)");
        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
        modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasColumnType("decimal(10,2)");

        // Configure Many-to-Many for Products <-> Tags
        modelBuilder.Entity<Product>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Products)
            .UsingEntity<Dictionary<string, object>>(
                "ProductTags",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId")
            );

        // Composite Key for OrderItems
        modelBuilder.Entity<OrderItem>()
            .HasKey(oi => new { oi.OrderId, oi.ProductId });
    }
}