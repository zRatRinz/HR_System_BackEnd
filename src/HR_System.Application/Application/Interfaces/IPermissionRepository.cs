using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IPermissionRepository
{
    Task<Dictionary<int, string[]>> GetPermissionsByRoleIdAsync();
}
