using Microsoft.AspNetCore.Mvc;

namespace GalacticFleetCommand.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns the health status of the service.
    /// </summary>
    /// <returns>Indicates that the service is running.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}