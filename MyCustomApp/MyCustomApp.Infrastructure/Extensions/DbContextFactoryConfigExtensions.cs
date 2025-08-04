using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.ConfigurationKeys;
using Shared.DataAccess;
using Shared.Interfaces;
namespace MyCustomApp.Infrastructure.Extensions
{
    public static class DbContextFactoryConfigExtension
    {

        public static IServiceCollection AddDbContextFactory<TContext>(this IServiceCollection services, IConfiguration configuration, bool enableDatalogging, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
        {
            services.AddDbContextFactory<TContext>(OptionBuilder<TContext>(configuration, enableDatalogging), serviceLifetime);
            return services;
        }

        private static Action<IServiceProvider, DbContextOptionsBuilder> OptionBuilder<TContext>(IConfiguration configuration, bool enableDatalogging)
            where TContext : DbContext
        {
            return (sp, options) =>
            {
                IUserIdentity userIdentity = sp.GetRequiredService<IUserIdentity>();
                UpdateAuditableEntitiesInterceptor auditableInterceptor = new(userIdentity);

                string? connectionString = configuration.GetConnectionString("SqlServer");

                // Force SSL configuration for development
                var builder = new SqlConnectionStringBuilder(connectionString);
                builder.Encrypt = false;
                builder.TrustServerCertificate = true;
                connectionString = builder.ConnectionString;

                // Determine schema and migration table based on context type
                string schema = GetSchemaForContext<TContext>();
                string migrationTable = $"__EF_MigrationsHistory";

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(ConfigurationKeys.Assemblies.InfrastructureAssemblyName);
                    sqlOptions.MigrationsHistoryTable(migrationTable, schema);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(3), null);
                    sqlOptions.CommandTimeout(15);
                });
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging(enableDatalogging);
                options.AddInterceptors(auditableInterceptor);
            };
        }

        private static string GetSchemaForContext<TContext>() where TContext : DbContext
        {
            var contextTypeName = typeof(TContext).Name;

            // Remove "DbContext" suffix to get schema name
            return contextTypeName.Replace("DbContext", string.Empty).ToLower();
        }
    }
}
