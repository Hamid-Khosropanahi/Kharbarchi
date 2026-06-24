using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new
        {
            ok = true,
            service = "Kharbarchi.Server",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            serverTimeUtc = DateTimeOffset.UtcNow
        });
    }
}
