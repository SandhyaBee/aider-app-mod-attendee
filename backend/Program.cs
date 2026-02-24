using Microsoft.Azure.Cosmos;
using StyleVerse.Backend.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cosmosEndpoint = builder.Configuration["CosmosDb:Endpoint"]
    ?? throw new InvalidOperationException("CosmosDb:Endpoint not configured");
var cosmosKey = builder.Configuration["CosmosDb:Key"]
    ?? Environment.GetEnvironmentVariable("COSMOSDB_KEY")
    ?? throw new InvalidOperationException("CosmosDb:Key not configured. Set it in appsettings.json or COSMOSDB_KEY env var.");
var cosmosDatabaseName = builder.Configuration["CosmosDb:DatabaseName"] ?? "StyleVerseDb";

var preferredRegion = builder.Configuration["CosmosDb:PreferredRegion"]
    ?? Environment.GetEnvironmentVariable("COSMOS_PREFERRED_REGION")
    ?? "Germany West Central";

builder.Services.AddSingleton(_ =>
{
    var options = new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct,
        ApplicationPreferredRegions = new List<string> { preferredRegion },
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
