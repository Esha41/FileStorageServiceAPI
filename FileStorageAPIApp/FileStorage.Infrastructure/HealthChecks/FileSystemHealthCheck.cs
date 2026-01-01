using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FileStorage.Infrastructure.HealthChecks
{
    public class FileSystemHealthCheck : IHealthCheck
    {
        private readonly string? _storagePath;

        public FileSystemHealthCheck(IConfiguration configuration)
        {
            _storagePath = configuration["FileStorage:BaseFilePath"];
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Directory.Exists(_storagePath))
                    return Task.FromResult(HealthCheckResult.Unhealthy("Storage folder missing"));

                var testFile = Path.Combine(_storagePath, "healthcheck.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                return Task.FromResult(HealthCheckResult.Healthy("Filesystem read/write OK"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Filesystem check failed", ex));
            }
        }
    }
}
