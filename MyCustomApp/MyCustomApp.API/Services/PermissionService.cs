using MyCustomApp.Domain.Enums;
using MyCustomAppMyCustomApp.API.Contracts;

namespace MyCustomAppMyCustomApp.API.Services
{
    public class PermissionService(ILogger<PermissionService> logger) : IPermissionService
    {
        public bool HasPermission(HttpContext context, params Permissions[] requiredPermissions)
        {
            if (requiredPermissions == null || requiredPermissions.Length == 0)
                return true;

            var userPermissions = GetUserPermissions(context);
            
            return requiredPermissions.Any(required => 
                userPermissions.Contains(required.ToString()));
        }

        public void EnsurePermission(HttpContext context, params Permissions[] requiredPermissions)
        {
            if (!HasPermission(context, requiredPermissions))
            {
                var permissionsString = string.Join(", ", requiredPermissions);
                logger.LogWarning("User lacks required permissions: {Permissions}", permissionsString);
                
                throw new UnauthorizedAccessException("Forbidden: User lacks required permissions");
            }
        }

        private static HashSet<string> GetUserPermissions(HttpContext context)
        {
            var permissions = new HashSet<string>();
            var permissionClaims = context.User.Claims.Where(c => c.Type == "permissions");
            
            foreach (var claim in permissionClaims)
            {
                if (!string.IsNullOrWhiteSpace(claim.Value))
                {
                    permissions.Add(claim.Value);
                }
            }
            
            return permissions;
        }
    }
} 