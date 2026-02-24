using Newtonsoft.Json;

namespace StyleVerse.Backend.Models;

public class Order
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("totalAmount")]
    public double TotalAmount { get; set; }

    [JsonProperty("totalAmountCents")]
    public int TotalAmountCents { get; set; }

    [JsonProperty("orderDate")]
    public string OrderDate { get; set; } = string.Empty;

    [JsonProperty("shippingRegion")]
    public string ShippingRegion { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("items")]
    public List<OrderLineItem> Items { get; set; } = new();

    [JsonProperty("itemCount")]
    public int ItemCount { get; set; }

    [JsonProperty("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    [JsonProperty("type")]
    public string Type { get; set; } = "order";

    [JsonProperty("updatedDate")]
    public string UpdatedDate { get; set; } = string.Empty;

    [JsonProperty("_rid")]
    public string? Rid { get; set; }
}

public class OrderLineItem
{
    [JsonProperty("productId")]
    public int ProductId { get; set; }

    [JsonProperty("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("unitPrice")]
    public double UnitPrice { get; set; }

    [JsonProperty("unitPriceCents")]
    public int UnitPriceCents { get; set; }

    [JsonProperty("lineTotal")]
    public double LineTotal { get; set; }

    [JsonProperty("lineTotalCents")]
    public int LineTotalCents { get; set; }
}

public class PlaceOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double TotalAmount { get; set; }
    public string ShippingRegion { get; set; } = "East US";
    public List<OrderItemRequest> OrderItems { get; set; } = new();
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
}
