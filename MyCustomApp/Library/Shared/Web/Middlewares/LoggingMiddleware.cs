using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Shared.Web.Middlewares
{
    public sealed class LoggingMiddleware(
        ILogger<LoggingMiddleware> logger,
        RequestDelegate next)
    {
        private readonly ILogger<LoggingMiddleware> _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

        public async Task InvokeAsync(HttpContext httpContext)
        {
            using IDisposable? scope = _Logger.BeginScope(new Dictionary<string, object>
            {
                {"Application", Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty}
            });
            await _next(httpContext).ConfigureAwait(false);
        }
    }
}
