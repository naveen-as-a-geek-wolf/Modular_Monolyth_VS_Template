
using MyCustomApp.Domain.Enums;

namespace MyCustomApp.API.Extensions
{
    public static class EndpointsBuilderExtension
    {
        // Builder extension for convenience - adds permission checking to endpoint
        public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, params Permissions[] permissions)
            where TBuilder : IEndpointConventionBuilder
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (permissions == null || permissions.Length == 0)
                return builder;

            return builder.AddEndpointFilter(async (context, next) =>
            {
                var httpContext = context.HttpContext;
                httpContext.EnsurePermission(permissions);
                return await next(context);
            });
        }

        // Builder extension for endpoints that don't require permissions
        public static TBuilder RequireNoPermission<TBuilder>(this TBuilder builder)
            where TBuilder : IEndpointConventionBuilder
        {
            return builder;
        }   

    }
}
