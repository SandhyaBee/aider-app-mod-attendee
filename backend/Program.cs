using Microsoft.Azure.Cosmos;
using StyleVerse.Backend.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cosmosEndpoint = builder.Configuration["CosmosDb:Endpoint"]
    ?? throw new InvalidOperationException("CosmosDb:Endpoint not configured");
var cosmosKey = builder.Configuration["CosmosDb:Key"]
    ?? throw new InvalidOperationException("CosmosDb:Key not configured");
var cosmosDatabaseName = builder.Configuration["CosmosDb:DatabaseName"] ?? "StyleVerseDb";

builder.Services.AddSingleton(_ =>
{
    var options = new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Gateway,
        ApplicationPreferredRegions = new List<string> { "Germany West Central", "East US", "North Europe" },
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };
    return new CosmosClient(cosmosEndpoint, cosmosKey, options);
});

builder.Services.AddSingleton<CosmosDbService>(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    return new CosmosDbService(client, cosmosDatabaseName);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
