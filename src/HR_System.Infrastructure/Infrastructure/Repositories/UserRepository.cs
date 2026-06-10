using HR_System.Application.DTOs.Auth;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<User> CreateAsync(User user)
    {
        var sql = @"
            INSERT INTO Users (Email, PasswordHash, Name, Status, CreatedAt)
            VALUES (@Email, @PasswordHash, @Name, @Status, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            user.Email,
            user.PasswordHash,
            user.Name,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        });

        user.Id = newId;

        if (user.Roles != null && user.Roles.Count > 0)
        {
            foreach (var role in user.Roles)
            {
                var roleId = (int)role + 1;
                var insertRoleSql = @"
                    INSERT INTO UserRoles (UserId, RoleId)
                    VALUES (@UserId, @RoleId)";
                await ExecuteAsync(insertRoleSql, new { UserId = newId, RoleId = roleId });
            }
        }

        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        var sql = @"
            UPDATE Users
            SET Email = @Email, PasswordHash = @PasswordHash, Name = @Name,
                Status = @Status, UpdatedAt = @UpdatedAt
            WHERE UserId = @UserId";

        await ExecuteAsync(sql, new
        {
            UserId = user.Id,
            user.Email,
            user.PasswordHash,
            user.Name,
            Status = "Active",
            UpdatedAt = DateTime.UtcNow
        });

        if (user.Roles != null)
        {
            var deleteRolesSql = "DELETE FROM UserRoles WHERE UserId = @UserId";
            await ExecuteAsync(deleteRolesSql, new { UserId = user.Id });

            foreach (var role in user.Roles)
            {
                var roleId = (int)role + 1;
                var insertRoleSql = @"
                    INSERT INTO UserRoles (UserId, RoleId)
                    VALUES (@UserId, @RoleId)";
                await ExecuteAsync(insertRoleSql, new { UserId = user.Id, RoleId = roleId });
            }
        }

        return user;
    }

    public async Task DeleteAsync(int id)
    {
        var sql = "DELETE FROM Users WHERE UserId = @UserId";
        await ExecuteAsync(sql, new { UserId = id });
    }

    public async Task<UserDto?> GetByIdAsDtoAsync(int id)
    {
        var sql = @"
            SELECT u.UserId as Id, u.Email, u.Name,
                   e.DivisionId, e.DepartmentId, e.PositionId,
                   u.CreatedAt, u.UpdatedAt,
                   r.RoleName as Role,
                   p.PositionName as Position,
                   d.DepartmentName as Department,
                   div.DivisionName as Division
            FROM Users u
            LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
            LEFT JOIN Roles r ON ur.RoleId = r.RoleId
            LEFT JOIN Employees e ON u.UserId = e.UserId
            LEFT JOIN Positions p ON e.PositionId = p.PositionId
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN Divisions div ON e.DivisionId = div.DivisionId
            WHERE u.UserId = @UserId";

        var results = await QueryAsync<UserDto>(sql, new { UserId = id });
        return GroupUserWithRoles(results, id);
    }

    public async Task<List<UserDto>> GetAllAsDtoAsync()
    {
        var sql = @"
            SELECT u.UserId as Id, u.Email, u.Name,
                   e.DivisionId, e.DepartmentId, e.PositionId,
                   u.CreatedAt, u.UpdatedAt,
                   r.RoleName as Role,
                   p.PositionName as Position,
                   d.DepartmentName as Department,
                   div.DivisionName as Division
            FROM Users u
            LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
            LEFT JOIN Roles r ON ur.RoleId = r.RoleId
            LEFT JOIN Employees e ON u.UserId = e.UserId
            LEFT JOIN Positions p ON e.PositionId = p.PositionId
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN Divisions div ON e.DivisionId = div.DivisionId";

        var results = (await QueryAsync<UserDto>(sql)).ToList();
        return results.GroupBy(u => u.Id).Select(g => GroupUserWithRoles(g.ToList(), g.Key)).ToList();
    }

    public async Task<UserDto?> GetByEmailAsDtoAsync(string email)
    {
        var sql = @"
            SELECT u.UserId as Id, u.Email, u.PasswordHash, u.Name,
                   e.DivisionId, e.DepartmentId, e.PositionId,
                   u.CreatedAt, u.UpdatedAt,
                   r.RoleName as Role,
                   p.PositionName as Position,
                   d.DepartmentName as Department,
                   div.DivisionName as Division
            FROM Users u
            LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
            LEFT JOIN Roles r ON ur.RoleId = r.RoleId
            LEFT JOIN Employees e ON u.UserId = e.UserId
            LEFT JOIN Positions p ON e.PositionId = p.PositionId
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN Divisions div ON e.DivisionId = div.DivisionId
            WHERE u.Email = @Email";

        var results = await QueryAsync<UserDto>(sql, new { Email = email });
        var user = results.FirstOrDefault();
        if (user == null) return null;
        return GroupUserWithRoles(results, user.Id);
    }

    private static UserDto GroupUserWithRoles(IEnumerable<UserDto> results, int userId)
    {
        var first = results.First();
        return new UserDto
        {
            Id = userId,
            Email = first.Email,
            PasswordHash = first.PasswordHash,
            Name = first.Name,
            Roles = results.Where(r => !string.IsNullOrEmpty(r.Role)).Select(r => r.Role!).ToList(),
            DivisionId = first.DivisionId,
            DepartmentId = first.DepartmentId,
            PositionId = first.PositionId,
            Position = first.Position,
            Division = first.Division,
            Department = first.Department,
            CreatedAt = first.CreatedAt,
            UpdatedAt = first.UpdatedAt
        };
    }
}