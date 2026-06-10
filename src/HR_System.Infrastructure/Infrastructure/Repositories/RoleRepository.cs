using Dapper;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class RoleRepository : BaseRepository, IRoleRepository
{
    public RoleRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<List<Role>> GetAllAsync()
    {
        var sql = "SELECT RoleId, RoleName FROM Roles ORDER BY RoleId";
        var results = await QueryAsync<Role>(sql);
        return results.ToList();
    }
}