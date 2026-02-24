using Newtonsoft.Json;

namespace StyleVerse.Backend.Models;

public class CartItem
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonProperty("productId")]
    public int ProductId { get; set; }

    [JsonProperty("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonProperty("productPrice")]
    public double ProductPrice { get; set; }

    [JsonProperty("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("dateAdded")]
    public string DateAdded { get; set; } = string.Empty;

    [JsonProperty("lineTotal")]
    public double LineTotal { get; set; }

    [JsonProperty("productPriceCents")]
    public int ProductPriceCents { get; set; }

    [JsonProperty("lineTotalCents")]
    public int LineTotalCents { get; set; }

    [JsonProperty("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    [JsonProperty("type")]
    public string Type { get; set; } = "cartItem";

    [JsonProperty("updatedDate")]
    public string UpdatedDate { get; set; } = string.Empty;

    [JsonProperty("_rid")]
    public string? Rid { get; set; }
}

public class CartItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string DateAdded { get; set; } = string.Empty;
    public CartProductInfo Product { get; set; } = new();

    public static CartItemResponse FromCosmos(CartItem c) => new()
    {
        Id = c.Id,
        SessionId = c.SessionId,
        ProductId = c.ProductId,
        Quantity = c.Quantity,
        DateAdded = c.DateAdded,
        Product = new CartProductInfo
        {
            Name = c.ProductName,
            Price = c.ProductPrice
        }
    };
}

public class CartProductInfo
{
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
}

public class AddToCartRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
