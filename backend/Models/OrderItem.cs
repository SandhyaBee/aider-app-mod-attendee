namespace StyleVerse.Backend.Models;
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string ShippingRegion { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}