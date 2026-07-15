using HR_System.Application.Interfaces;
using HR_System.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace HR_System.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly Dictionary<UserRole, string[]> _permissionsByRole;
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        _permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        _permissionsByRole = LoadPermissions().GetAwaiter().GetResult();
    }

    private async Task<Dictionary<UserRole, string[]>> LoadPermissions()
    {
        var dbPermissions = await _permissionRepository.GetPermissionsByRoleIdAsync();
        var result = new Dictionary<UserRole, string[]>();

        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            var roleId = (int)role + 1;
            if (dbPermissions.TryGetValue(roleId, out var perms))
            {
                result[role] = perms;
            }
            else
            {
                result[role] = Array.Empty<string>();
            }
        }

        return result;
    }

    public string[] GetPermissionsForRole(UserRole role)
    {
        return _permissionsByRole.TryGetValue(role, out var permissions)
            ? permissions
            : Array.Empty<string>();
    }

    public string[] GetPermissionsForRoles(IEnumerable<UserRole> roles)
    {
        var permissions = roles.SelectMany(r => GetPermissionsForRole(r));
        return permissions.Distinct().ToArray();
    }

    public bool HasPermission(string[] userPermissions, string requiredPermission)
    {
        return userPermissions.Contains(requiredPermission);
    }
}
