using Newtonsoft.Json;

namespace StyleVerse.Backend.Models;

public class Product
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("price")]
    public double Price { get; set; }

    [JsonProperty("priceCents")]
    public int PriceCents { get; set; }

    [JsonProperty("categoryId")]
    public int CategoryId { get; set; }

    [JsonProperty("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonProperty("inventoryCount")]
    public int InventoryCount { get; set; }

    [JsonProperty("createdDate")]
    public string CreatedDate { get; set; } = string.Empty;

    [JsonProperty("updatedDate")]
    public string UpdatedDate { get; set; } = string.Empty;

    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonProperty("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    [JsonProperty("type")]
    public string Type { get; set; } = "product";

    [JsonProperty("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonProperty("metadata")]
    public ProductMetadata? Metadata { get; set; }

    [JsonProperty("rating")]
    public ProductRating? Rating { get; set; }

    [JsonProperty("_rid")]
    public string? Rid { get; set; }
}

public class ProductMetadata
{
    [JsonProperty("source")]
    public string Source { get; set; } = string.Empty;

    [JsonProperty("migratedDate")]
    public string MigratedDate { get; set; } = string.Empty;
}

public class ProductRating
{
    [JsonProperty("average")]
    public double Average { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }
}

public class ProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int InventoryCount { get; set; }
    public string CreatedDate { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Cosmosdb_id { get; set; } = string.Empty;

    public static ProductResponse FromCosmos(Product p) => new()
    {
        Id = p.Id,
        ProductId = p.ProductId,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        CategoryId = p.CategoryId,
        Category = p.CategoryName,
        InventoryCount = p.InventoryCount,
        CreatedDate = p.CreatedDate,
        Tags = p.Tags,
        Cosmosdb_id = p.Rid ?? string.Empty
    };
}
