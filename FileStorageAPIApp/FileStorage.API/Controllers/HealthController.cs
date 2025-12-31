using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FileStorage.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        // GET /health/live
        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new { status = "Healthy" });
        }

        // GET /health/ready
        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            var report = await _healthCheckService.CheckHealthAsync();

            if (report.Status == HealthStatus.Healthy)
            {
                return Ok(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new { e.Key, e.Value.Status, e.Value.Description })
                });
            }

            return StatusCode(503, new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new { e.Key, e.Value.Status, e.Value.Description })
            });
        }
    }
}
