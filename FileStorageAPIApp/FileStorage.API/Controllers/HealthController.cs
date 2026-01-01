using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FileStorage.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;
        public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        // GET /health/live
        [HttpGet("live")]
        public IActionResult Live()
        {
            _logger.LogInformation("Liveness check requested at {Time}", DateTime.UtcNow);
            return Ok(new { status = "Healthy" });
        }

        // GET /health/ready
        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            _logger.LogInformation("Readiness check requested at {Time}", DateTime.UtcNow);
            var report = await _healthCheckService.CheckHealthAsync();

            if (report.Status == HealthStatus.Healthy)
            {
                _logger.LogInformation("Readiness check passed with status {Status}", report.Status);

                return Ok(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new { e.Key, e.Value.Status, e.Value.Description })
                });
            }
            _logger.LogWarning("Readiness check failed with status {Status}", report.Status);

            return StatusCode(503, new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new { e.Key, e.Value.Status, e.Value.Description })
            });
        }
    }
}
