using Microsoft.AspNetCore.Mvc;

namespace RealEstate.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
        });
    }
}
