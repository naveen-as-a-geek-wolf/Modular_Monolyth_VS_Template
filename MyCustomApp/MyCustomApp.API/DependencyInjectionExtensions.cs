using System.Reflection;
using FluentValidation;
using MyCustomAppMyCustomApp.API.Contracts;
using MyCustomAppMyCustomApp.API.Services;

namespace MyCustomApp.API
{
    public static class DependencyInjectionExtensions
    {
        public static void AddApplicationDependencies(this IServiceCollection services, ConfigurationManager configuration) 
        {
            services.AddHttpClient();
            services.AddValidatorsFromAssembly(Assembly.Load("MyCustomApp.Application"));
            services.AddScoped<IPermissionService, PermissionService>();
        }
    }
}
