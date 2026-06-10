using HR_System.Domain.Enums;

namespace HR_System.Application.Interfaces;

public interface IPermissionService
{
    string[] GetPermissionsForRole(UserRole role);
    string[] GetPermissionsForRoles(IEnumerable<UserRole> roles);
    bool HasPermission(string[] userPermissions, string requiredPermission);
}