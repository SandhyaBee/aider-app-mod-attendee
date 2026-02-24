using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using StyleVerse.Backend.Models;

namespace StyleVerse.Backend.Services;

public class CosmosDbService
{
    private readonly Container _productsContainer;
    private readonly Container _cartItemsContainer;
    private readonly Container _ordersContainer;

    public CosmosDbService(CosmosClient client, string databaseName)
    {
        var database = client.GetDatabase(databaseName);
        _productsContainer = database.GetContainer("Products");
        _cartItemsContainer = database.GetContainer("CartItems");
        _ordersContainer = database.GetContainer("Orders");
    }

    // ─── Products ───────────────────────────────────────────────

    public async Task<List<Product>> GetProductsAsync()
    {
        var query = new QueryDefinition(
            "SELECT c.id, c.productId, c.name, c.description, c.price, c.priceCents, " +
            "c.categoryId, c.categoryName, c.inventoryCount, c.createdDate, c.updatedDate, " +
            "c.tags, c.schemaVersion, c.type, c.imageUrl, c.metadata, c.rating, c._rid " +
            "FROM c WHERE c.type = 'product'");
        var iterator = _productsContainer.GetItemQueryIterator<Product>(
            query,
            requestOptions: new QueryRequestOptions { MaxConcurrency = -1, MaxItemCount = 100 });

        var results = new List<Product>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        try
        {
            // Point read is 3-10x cheaper than query when id + partition key are known
            // Try all category partitions since we only have the id
            var query = new QueryDefinition(
                "SELECT c.id, c.productId, c.name, c.description, c.price, c.priceCents, " +
                "c.categoryId, c.categoryName, c.inventoryCount, c.createdDate, c.updatedDate, " +
                "c.tags, c.schemaVersion, c.type, c.imageUrl, c.metadata, c.rating, c._rid " +
                "FROM c WHERE c.id = @id AND c.type = 'product'")
                .WithParameter("@id", id);

            var iterator = _productsContainer.GetItemQueryIterator<Product>(
                query,
                requestOptions: new QueryRequestOptions { MaxConcurrency = -1, MaxItemCount = 1 });

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }
            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Product?> GetProductByIdAndCategoryAsync(string id, int categoryId)
    {
        try
        {
            var response = await _productsContainer.ReadItemAsync<Product>(
                id, new PartitionKey(categoryId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Product> UpsertProductAsync(Product product)
    {
        product.Type = "product";
        product.SchemaVersion = 1;
        product.UpdatedDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
        product.PriceCents = (int)Math.Round(product.Price * 100);

        if (string.IsNullOrEmpty(product.Id))
        {
            product.Id = Guid.NewGuid().ToString();
            product.ProductId = product.Id;
        }
        if (string.IsNullOrEmpty(product.ProductId))
            product.ProductId = product.Id;

        if (string.IsNullOrEmpty(product.CreatedDate))
            product.CreatedDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000Z");

        var response = await _productsContainer.UpsertItemAsync(
            product, new PartitionKey(product.CategoryId));
        return response.Resource;
    }

    // ─── Cart Items ─────────────────────────────────────────────

    public async Task<List<CartItem>> GetCartAsync(string sessionId)
    {
        var query = new QueryDefinition(
            "SELECT c.id, c.sessionId, c.productId, c.productName, c.productPrice, " +
            "c.categoryName, c.quantity, c.dateAdded, c.lineTotal, c.productPriceCents, " +
            "c.lineTotalCents, c.schemaVersion, c.type, c.updatedDate, c._rid " +
            "FROM c WHERE c.sessionId = @sid AND c.type = 'cartItem'")
            .WithParameter("@sid", sessionId);

        var iterator = _cartItemsContainer.GetItemQueryIterator<CartItem>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(sessionId),
                MaxItemCount = 50
            });

        var results = new List<CartItem>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<CartItem> AddToCartAsync(AddToCartRequest request)
    {
        var existing = await FindCartItemAsync(request.SessionId, request.ProductId);

        if (existing != null)
        {
            existing.Quantity += request.Quantity;
            existing.LineTotal = existing.ProductPrice * existing.Quantity;
            existing.LineTotalCents = existing.ProductPriceCents * existing.Quantity;
            existing.UpdatedDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000Z");

            var updateResponse = await _cartItemsContainer.UpsertItemAsync(
                existing, new PartitionKey(existing.SessionId));
            return updateResponse.Resource;
        }

        var product = await GetProductByIdAsync(request.ProductId.ToString());

        var cartItem = new CartItem
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = request.SessionId,
            ProductId = request.ProductId,
            ProductName = product?.Name ?? "Unknown",
            ProductPrice = product?.Price ?? 0,
            CategoryName = product?.CategoryName ?? "",
            Quantity = request.Quantity,
            DateAdded = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000Z"),
            UpdatedDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000Z"),
            SchemaVersion = 1,
            Type = "cartItem"
        };
        cartItem.LineTotal = cartItem.ProductPrice * cartItem.Quantity;
        cartItem.ProductPriceCents = (int)Math.Round(cartItem.ProductPrice * 100);
        cartItem.LineTotalCents = cartItem.ProductPriceCents * cartItem.Quantity;

        var response = await _cartItemsContainer.CreateItemAsync(
            cartItem, new PartitionKey(cartItem.SessionId));
        return response.Resource;
    }

    public async Task<bool> RemoveFromCartAsync(string id, string sessionId)
    {
        try
        {
            await _cartItemsContainer.DeleteItemAsync<CartItem>(
                id, new PartitionKey(sessionId));
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private async Task<CartItem?> FindCartItemAsync(string sessionId, int productId)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.sessionId = @sid AND c.productId = @pid AND c.type = 'cartItem'")
            .WithParameter("@sid", sessionId)
            .WithParameter("@pid", productId);

        var iterator = _cartItemsContainer.GetItemQueryIterator<CartItem>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(sessionId),
                MaxItemCount = 1
            });

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }
        return null;
    }

    // ─── Orders ─────────────────────────────────────────────────

    public async Task<List<Order>> GetOrdersAsync()
    {
        var query = new QueryDefinition(
            "SELECT c.id, c.customerName, c.email, c.totalAmount, c.totalAmountCents, " +
            "c.orderDate, c.shippingRegion, c.status, c.items, c.itemCount, " +
            "c.schemaVersion, c.type, c.updatedDate, c._rid " +
            "FROM c WHERE c.type = 'order'");
        var iterator = _ordersContainer.GetItemQueryIterator<Order>(
            query,
            requestOptions: new QueryRequestOptions { MaxConcurrency = -1, MaxItemCount = 100 });

        var results = new List<Order>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<Order> PlaceOrderAsync(PlaceOrderRequest request)
    {
        var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
        var region = string.IsNullOrEmpty(request.ShippingRegion) ? "East US" : request.ShippingRegion;

        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerName = request.CustomerName,
            Email = request.Email,
            TotalAmount = request.TotalAmount,
            TotalAmountCents = (int)Math.Round(request.TotalAmount * 100),
            OrderDate = now,
            ShippingRegion = region,
            Status = "Pending",
            SchemaVersion = 1,
            Type = "order",
            UpdatedDate = now,
            Items = request.OrderItems.Select(oi => new OrderLineItem
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                UnitPriceCents = (int)Math.Round(oi.UnitPrice * 100),
                LineTotal = oi.UnitPrice * oi.Quantity,
                LineTotalCents = (int)Math.Round(oi.UnitPrice * 100) * oi.Quantity
            }).ToList(),
        };
        order.ItemCount = order.Items.Count;

        var response = await _ordersContainer.CreateItemAsync(
            order, new PartitionKey(order.ShippingRegion));

        // Clear cart items in parallel
        _ = ClearCartAsync(request.CustomerName);

        return response.Resource;
    }

    private async Task ClearCartAsync(string sessionId)
    {
        var items = await GetCartAsync(sessionId);
        if (items.Count == 0) return;

        var deleteTasks = items.Select(item =>
            _cartItemsContainer.DeleteItemAsync<CartItem>(
                item.Id, new PartitionKey(sessionId)));
        await Task.WhenAll(deleteTasks);
    }
}
