namespace StyleVerse.Backend.Models;
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    public int InventoryCount { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}