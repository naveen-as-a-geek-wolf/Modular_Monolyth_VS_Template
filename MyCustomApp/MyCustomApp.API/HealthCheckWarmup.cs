using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using MyCustomApp.Application.Contracts;

namespace MyCustomApp.API
{
    public class HealthCheckWarmup(IServiceProvider serviceProvider, ILogger<HealthCheckWarmup> logger) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthResults = new List<(string Name, bool IsHealthy, string? Error)>();
            
            try
            {
                var dbContextTypes = GetRegisteredDbContextTypes();
                logger.LogInformation("Starting health check for {Count} DbContext(s)", dbContextTypes.Count);
                
                foreach (var (interfaceType, name) in dbContextTypes)
                {
                    try
                    {
                        await CheckDbContextHealth(interfaceType, name, healthResults, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to check health for DbContext: {Name}", name);
                        healthResults.Add((name, false, ex.Message));
                    }
                }
                
                var healthyCount = healthResults.Count(r => r.IsHealthy);
                var totalCount = healthResults.Count;
                
                if (totalCount == 0)
                {
                    return HealthCheckResult.Degraded("No DbContexts found to check");
                }
                
                if (healthyCount == totalCount)
                {
                    var successMessage = $"All {totalCount} DbContext(s) are healthy: {string.Join(", ", healthResults.Select(r => r.Name))}";
                    logger.LogInformation("Database warmup successful: {Message}", successMessage);
                    return HealthCheckResult.Healthy(successMessage);
                }
                
                var unhealthyContexts = healthResults.Where(r => !r.IsHealthy).ToList();
                var errorMessage = $"{unhealthyContexts.Count}/{totalCount} DbContext(s) failed: " +
                                 $"{string.Join(", ", unhealthyContexts.Select(r => $"{r.Name} ({r.Error})"))}";
                
                logger.LogWarning("Database warmup partially failed: {Message}", errorMessage);
                return HealthCheckResult.Degraded(errorMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database warmup failed completely");
                return HealthCheckResult.Unhealthy("Database warmup failed", ex);
            }
        }
        
        private async Task CheckDbContextHealth(Type interfaceType, string name, 
            List<(string Name, bool IsHealthy, string? Error)> results, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService(interfaceType);
            
            if (dbContext is DbContext efContext)
            {
                // Test database connectivity
                var canConnect = await efContext.Database.CanConnectAsync(cancellationToken);
                
                if (canConnect)
                {
                    logger.LogDebug("DbContext {Name} connection successful", name);
                    results.Add((name, true, null));
                }
                else
                {
                    logger.LogWarning("DbContext {Name} cannot connect to database", name);
                    results.Add((name, false, "Cannot connect to database"));
                }
            }
            else
            {
                logger.LogWarning("DbContext {Name} is not an Entity Framework DbContext", name);
                results.Add((name, false, "Not an Entity Framework DbContext"));
            }
        }
        
        private List<(Type InterfaceType, string Name)> GetRegisteredDbContextTypes()
        {
            // Find all interfaces that inherit from IDbContext (except IDbContext itself)
            var dbContextInterfaces = typeof(IDbContext).Assembly
                .GetTypes()
                .Where(t => t.IsInterface && 
                           typeof(IDbContext).IsAssignableFrom(t) && 
                           t != typeof(IDbContext))
                .ToList();
            
            var registeredTypes = new List<(Type, string)>();
            
            foreach (var interfaceType in dbContextInterfaces)
            {
                try
                {
                    // Check if the service is registered in DI container
                    using var scope = serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetService(interfaceType);
                    
                    if (service != null)
                    {
                        registeredTypes.Add((interfaceType, interfaceType.Name));
                        logger.LogDebug("Found registered DbContext interface: {Name}", interfaceType.Name);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Failed to resolve DbContext interface: {Name}", interfaceType.Name);
                }
            }
            
            return registeredTypes;
        }
    }
}
