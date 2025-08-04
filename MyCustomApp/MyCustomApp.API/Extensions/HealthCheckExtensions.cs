using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace MyCustomApp.API.Extensions
{
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Enhanced health check response writer that provides detailed JSON responses
        /// with status, timing, and error information for each health check.
        /// </summary>
        public static async Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds,
                timestamp = DateTimeOffset.UtcNow,
                results = report.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration = entry.Value.Duration.TotalMilliseconds,
                        data = entry.Value.Data.Count > 0 ? entry.Value.Data : null,
                        exception = entry.Value.Exception?.Message
                    }
                )
            };
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        /// <summary>
        /// Simple health check response writer for basic alive/ready checks
        /// </summary>
        public static async Task WriteSimpleHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTimeOffset.UtcNow,
                version = System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString()
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        /// <summary>
        /// Configure health check options with enhanced response formatting
        /// </summary>
        public static HealthCheckOptions WithDetailedResponse(this HealthCheckOptions options)
        {
            options.ResponseWriter = WriteDetailedHealthCheckResponse;
            return options;
        }

        /// <summary>
        /// Configure health check options with simple response formatting
        /// </summary>
        public static HealthCheckOptions WithSimpleResponse(this HealthCheckOptions options)
        {
            options.ResponseWriter = WriteSimpleHealthCheckResponse;
            return options;
        }
    }
} 