using Dapper;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class PermissionRepository : BaseRepository, IPermissionRepository
{
    public PermissionRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Dictionary<int, string[]>> GetPermissionsByRoleIdAsync()
    {
        var sql = @"
            SELECT r.RoleId, p.PermissionName
            FROM Roles r
            INNER JOIN RolePermissions rp ON r.RoleId = rp.RoleId
            INNER JOIN Permissions p ON rp.PermissionId = p.PermissionId";

        var results = await QueryAsync<dynamic>(sql);
        return results
            .GroupBy(r => (int)r.RoleId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => (string)r.PermissionName).ToArray()
            );
    }

}
