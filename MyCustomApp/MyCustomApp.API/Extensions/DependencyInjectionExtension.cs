using System.Reflection;
using MediateX;

namespace MyCustomApp.API.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static void SetupApplicationDependencies(this IServiceCollection services, ConfigurationManager configuration)
        {
            // Register MediateX and scan both API and Application assemblies for handlers
            services.AddMediateX(
                typeof(Program).Assembly, // MyCustomApp.API assembly
                Assembly.Load("MyCustomApp.Application") // MyCustomApp.Application assembly
            );

            services.AddHealthChecks().AddCheck<HealthCheck>("Ping").AddCheck<HealthCheckWarmup>("Warmup");
        }
    }
}
