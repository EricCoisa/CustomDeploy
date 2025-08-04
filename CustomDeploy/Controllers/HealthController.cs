using Microsoft.AspNetCore.Mvc;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            return Ok(new 
            { 
                message = "API Online",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
