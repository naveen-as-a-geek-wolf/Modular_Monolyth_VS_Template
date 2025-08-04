using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Primitives;
using Shared.ConfigurationKeys;
using System.Reflection;

namespace MyCustomApp.API.Logging
{
    /// <summary>
    /// Telemetry initializer that enriches Application Insights telemetry with custom HTTP headers
    /// and application metadata like the app name and client details.
    /// </summary>
    public class EnrichLogsWithAppDetails : TelemetryInitializerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnrichLogsWithAppDetails(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Called automatically by Application Insights to enrich telemetry data before it's sent.
        /// </summary>
        protected override void OnInitializeTelemetry(
            HttpContext platformContext,
            RequestTelemetry requestTelemetry,
            ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties[ConfigurationKeys.Telemetry.ApplicationNameProperty] =
                Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty;

            StringValues? userAgent = _httpContextAccessor?.HttpContext?.Request.Headers.UserAgent;
            StringValues? userSessionId = _httpContextAccessor?.HttpContext?.Request.Headers[ConfigurationKeys.Telemetry.UserSessionIdHeader];
            StringValues? platform = _httpContextAccessor?.HttpContext?.Request.Headers[ConfigurationKeys.Telemetry.PlatformHeader];

            if (requestTelemetry is not null)
            {
                if (!StringValues.IsNullOrEmpty(userAgent ?? StringValues.Empty))
                {
                    requestTelemetry.Properties[ConfigurationKeys.Telemetry.UserAgentHeader] = userSessionId.ToString();
                }

                if (!StringValues.IsNullOrEmpty(userSessionId ?? StringValues.Empty))
                {
                    requestTelemetry.Properties[ConfigurationKeys.Telemetry.UserSessionIdHeader] = userSessionId.ToString();
                }

                if (!StringValues.IsNullOrEmpty(platform ?? StringValues.Empty))
                {
                    requestTelemetry.Properties[ConfigurationKeys.Telemetry.PlatformHeader] = platform.ToString();
                }
            }


        }
    }
}
