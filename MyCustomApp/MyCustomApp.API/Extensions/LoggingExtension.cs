using MyCustomApp.API.Logging;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Shared.ConfigurationKeys;

namespace MyCustomApp.API.Extensions
{
    public static class LoggingExtension
    {
        /// <summary>
        /// Sets up logging and telemetry for the application.
        /// <para>
        /// <b>Development or Testing:</b>
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <description>Enables Seq logging using the "Seq" configuration section.</description>
        /// </item>
        /// <item>
        /// <description>Disables Application Insights telemetry to reduce local noise.</description>
        /// </item>
        /// </list>
        /// <para>
        /// <b>Production or other environments:</b>
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <description>Enables Application Insights with a configured connection string.</description>
        /// </item>
        /// <item>
        /// <description>Adds Kubernetes enricher for environment-specific telemetry enrichment.</description>
        /// </item>
        /// <item>
        /// <description>Registers a custom telemetry initializer to inject HTTP headers and application metadata.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="services">The DI container to register logging and telemetry services.</param>
        /// <param name="environment">Provides the current runtime environment (Development, Testing, Production).</param>
        /// <param name="configuration">Application configuration to read logging settings.</param>
        public static void SetupLogging(this IServiceCollection services, IWebHostEnvironment environment, ConfigurationManager configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                if (environment.IsDevelopment() || environment.IsEnvironment(ConfigurationKeys.Environment.Testing))
                {
                    loggingBuilder.AddSeq(configuration.GetSection(ConfigurationKeys.AppSettings.Seq));

                    services.Configure<TelemetryConfiguration>(telemetryConfig =>
                    {
                        telemetryConfig.DisableTelemetry = true;
                    });
                }
                else
                {
                    loggingBuilder.AddApplicationInsights(config =>
                    {
                        config.ConnectionString = configuration[ConfigurationKeys.AppSettings.ApplicationInsights];
                    }, _ => { });

                    services.AddApplicationInsightsTelemetry();
                    services.AddApplicationInsightsKubernetesEnricher(LogLevel.None);

                    services.ConfigureTelemetryModule((Action<DependencyTrackingTelemetryModule, ApplicationInsightsServiceOptions>)delegate { });

                    services.AddSingleton<ITelemetryInitializer, EnrichLogsWithAppDetails>();
                }
            });

            services.AddHttpClient();
        }
    }
}
