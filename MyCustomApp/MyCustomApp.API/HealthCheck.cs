using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace MyCustomApp.API
{
    public class HealthCheck(IServiceProvider serviceProvider, ILogger<HealthCheck> logger) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var healthChecks = new List<(string Name, bool IsHealthy, string? Details)>();
                
                // Check API basic readiness
                await CheckApiReadiness(healthChecks, cancellationToken);
                
                // Check core services availability
                CheckCoreServices(healthChecks);
                
                // Generate result
                var healthyCount = healthChecks.Count(c => c.IsHealthy);
                var totalCount = healthChecks.Count;
                
                if (healthyCount == totalCount)
                {
                    var successDetails = string.Join(", ", healthChecks.Select(c => c.Name));
                    var message = $"API is healthy - All {totalCount} checks passed: {successDetails}";
                    logger.LogDebug("API health check successful: {Message}", message);
                    return HealthCheckResult.Healthy(message);
                }
                
                var failedChecks = healthChecks.Where(c => !c.IsHealthy).ToList();
                var errorMessage = $"API health check failed - {failedChecks.Count}/{totalCount} checks failed: " +
                                 $"{string.Join(", ", failedChecks.Select(c => $"{c.Name} ({c.Details})"))}";
                
                logger.LogWarning("API health check failed: {Message}", errorMessage);
                return HealthCheckResult.Unhealthy(errorMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "API health check failed with exception");
                return HealthCheckResult.Unhealthy("API health check failed", ex);
            }
        }
        
        private async Task CheckApiReadiness(List<(string Name, bool IsHealthy, string? Details)> healthChecks, 
            CancellationToken cancellationToken)
        {
            // Basic async operation test
            await Task.Delay(1, cancellationToken);
            healthChecks.Add(("AsyncOperations", true, "Responsive"));
            
            // Check if we can get basic app info
            try
            {
                var appName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
                var version = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Unknown";
                healthChecks.Add(("AppInfo", true, $"{appName} v{version}"));
            }
            catch (Exception ex)
            {
                healthChecks.Add(("AppInfo", false, ex.Message));
            }
        }
        
        private void CheckCoreServices(List<(string Name, bool IsHealthy, string? Details)> healthChecks)
        {
            // Check if IConfiguration is available
            try
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                healthChecks.Add(("Configuration", config != null, config != null ? "Available" : "Missing"));
            }
            catch (Exception ex)
            {
                healthChecks.Add(("Configuration", false, ex.Message));
            }
            
            // Check if ILogger is available
            try
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                healthChecks.Add(("Logging", loggerFactory != null, loggerFactory != null ? "Available" : "Missing"));
            }
            catch (Exception ex)
            {
                healthChecks.Add(("Logging", false, ex.Message));
            }
            
            // Check if HTTP context accessor is available
            try
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                healthChecks.Add(("HttpContext", httpContextAccessor != null, httpContextAccessor != null ? "Available" : "Missing"));
            }
            catch (Exception ex)
            {
                healthChecks.Add(("HttpContext", false, ex.Message));
            }
            
            // Check memory pressure (basic check)
            try
            {
                var workingSet = Environment.WorkingSet;
                var isHealthy = workingSet > 0; // Basic sanity check
                healthChecks.Add(("Memory", isHealthy, $"WorkingSet: {workingSet / 1024 / 1024} MB"));
            }
            catch (Exception ex)
            {
                healthChecks.Add(("Memory", false, ex.Message));
            }
        }
    }
}
