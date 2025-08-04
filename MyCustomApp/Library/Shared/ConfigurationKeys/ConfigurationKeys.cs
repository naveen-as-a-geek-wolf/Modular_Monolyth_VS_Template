namespace Shared.ConfigurationKeys
{
    public static class ConfigurationKeys
    {

        public static class Assemblies
        {
            public const string AppName = "MyCustomApp";
            public const string ApplicationAssemblyName = "MyCustomApp.Application";
            public const string InfrastructureAssemblyName = "MyCustomApp.Infrastructure";
        }

        public static class AppSettings
        {
            public const string ApplicationInsights = "ApplicationInsights:ConnectionString";
            public const string Seq = "Seq";
        }

        public static class Environment
        {
            public const string Testing = "Testing";
        }

        public static class CrossOriginResourceSharing
        {
            public const string PolicyName = "_myAllowSpecificOrigins";
            public const string ExposedHeaderContentDisposition = "Content-Disposition";
        }

        public static class Telemetry
        {
            public const string UserSessionIdHeader = "User-Session-Id";
            public const string PlatformHeader = "Platform";
            public const string UserAgentHeader = "UserAgent";
            public const string ApplicationNameProperty = "ApplicationName";
        }
        public static class Swagger
        {
            public const string Version = "v1";
            public const string SecuritySchemeName = "Bearer";
            public const string AuthorizationHeader = "Authorization";
            public const string TokenDescription = "Please enter token";
            public const string BearerFormat = "JWT";
            public const string Scheme = "bearer";
        }
    }
}
