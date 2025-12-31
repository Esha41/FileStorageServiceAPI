using Microsoft.AspNetCore.Mvc;

namespace FileStorage.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        // GET /health/live
        [HttpGet("live")]
        public IActionResult Live() => Ok("Alive");

        // GET /health/ready
        [HttpGet("ready")]
        public IActionResult Ready() => Ok("Ready");
    }
}
