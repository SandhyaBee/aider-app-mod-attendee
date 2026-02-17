namespace StyleVerse.Backend.Models
{
    public class CartItem
    {
        public int Id { get; set; } // SQL Primary Key
        public string SessionId { get; set; } = string.Empty; // Used to identify user session
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public virtual Product? Product { get; set; }
    }
}