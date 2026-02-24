using Microsoft.AspNetCore.Mvc;

namespace StyleVerse.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var region = Environment.GetEnvironmentVariable("REGION_NAME") ?? "germanywestcentral";
        Response.Headers["X-Azure-Region"] = region;
        return Ok(new { status = "healthy", region, timestamp = DateTime.UtcNow });
    }
}
