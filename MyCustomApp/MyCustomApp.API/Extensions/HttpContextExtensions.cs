

using MyCustomApp.Domain.Enums;
using MyCustomAppMyCustomApp.API.Contracts;

namespace MyCustomApp.API.Extensions
{
    public static class HttpContextExtensions
    {
        // HttpContext extension - Check if user has required permissions (returns bool)
        public static bool HasPermission(this HttpContext context, params Permissions[] requiredPermissions)
        {
            var permissionService = context.RequestServices.GetRequiredService<IPermissionService>();
            return permissionService.HasPermission(context, requiredPermissions);
        }

        // HttpContext extension - Ensure user has required permissions (throws exception if not)
        public static void EnsurePermission(this HttpContext context, params Permissions[] requiredPermissions)
        {
            var permissionService = context.RequestServices.GetRequiredService<IPermissionService>();
            permissionService.EnsurePermission(context, requiredPermissions);
        }
    }
} 