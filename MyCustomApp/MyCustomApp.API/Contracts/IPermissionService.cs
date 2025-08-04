using MyCustomApp.Domain.Enums;

namespace MyCustomAppMyCustomApp.API.Contracts
{
    public interface IPermissionService
    {
        bool HasPermission(HttpContext context, params Permissions[] requiredPermissions);
        void EnsurePermission(HttpContext context, params Permissions[] requiredPermissions);
    }
} 