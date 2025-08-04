using MyCustomApp.Application.Contracts;
using MyCustomApp.Infrastructure.Modules.Game;
using MyCustomApp.Infrastructure.Modules.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Identities;
using Shared.Interfaces;

namespace MyCustomApp.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection SetupInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration, bool enableDatalogging)
        {
            services.AddScoped<IUserIdentity, UserIdentity>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            services.AddDbContextFactory<GameDbContext>(configuration, enableDatalogging);
            services.AddScoped<IGameDbContext>(provider => provider.GetRequiredService<GameDbContext>());

            services.AddDbContextFactory<UserDbContext>(configuration, enableDatalogging);
            services.AddScoped<IUserDbContext>(provider => provider.GetRequiredService<UserDbContext>());

            return services;
        }
        

    }
}
