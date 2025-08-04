using Shared.ConfigurationKeys;
namespace MyCustomApp.API.Extensions
{
    /// <summary>
    /// Extension methods for setting up Cross-Origin Resource Sharing (CORS) policies.
    /// </summary>
    public static class CorsExtension
    {
        /// <summary>
        /// Configures CORS policies for the application.
        /// 
        /// This method enables a permissive CORS policy only in the Development environment.
        /// It allows any origin, method, and header, and exposes the "Content-Disposition" header to the client.
        /// 
        /// In non-development environments, CORS is not configured by this method.
        /// </summary>
        /// <param name="services">The IServiceCollection to register CORS services.</param>
        /// <param name="environment">Provides information about the hosting environment.</param>
        public static void SetupCors(this IServiceCollection services, IWebHostEnvironment environment)
        {
            // Only enable CORS if the environment is Development.
            if (!environment.IsDevelopment())
            {
                return;
            }

            services.AddCors(options =>
            {
                options.AddPolicy(ConfigurationKeys.CrossOriginResourceSharing.PolicyName, policy =>
                {
                    policy.AllowAnyOrigin()    // Allow requests from any origin
                          .AllowAnyMethod()    // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
                          .AllowAnyHeader()    // Allow any HTTP header
                          .WithExposedHeaders(ConfigurationKeys.CrossOriginResourceSharing.ExposedHeaderContentDisposition); // Allow client to read this response header
                });
            });
        }
    }
}
