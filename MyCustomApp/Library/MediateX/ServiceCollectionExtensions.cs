using System.Reflection;
using MediateX.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediateX;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediateX(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        services.TryAddScoped<IMediateX, MediateX>();
        RegisterHandlers(services, assemblies);
        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        bool IsHandlerInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            return genericTypeDefinition == typeof(ICommandHandler<,>) ||
                   genericTypeDefinition == typeof(IQueryHandler<,>);
        }

        var handlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface)
            .Where(type => type.GetInterfaces().Any(IsHandlerInterface))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(IsHandlerInterface)
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                services.TryAddScoped(interfaceType, handlerType);
            }
        }
    }
}