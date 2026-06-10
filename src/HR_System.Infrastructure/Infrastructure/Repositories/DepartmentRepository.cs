using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class DepartmentRepository : BaseRepository, IDepartmentRepository
{
    public DepartmentRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Department?> GetByIdAsync(int id)
    {
        var sql = @"
            SELECT DepartmentId, DivisionId, DepartmentName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Departments
            WHERE DepartmentId = @DepartmentId";

        return await QuerySingleOrDefaultAsync<Department>(sql, new { DepartmentId = id });
    }

    public async Task<List<Department>> GetAllAsync()
    {
        var sql = @"
            SELECT DepartmentId, DivisionId, DepartmentName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Departments";

        var results = await QueryAsync<Department>(sql);
        return results.ToList();
    }

    public async Task<List<Department>> GetByDivisionIdAsync(int divisionId)
    {
        var sql = @"
            SELECT DepartmentId, DivisionId, DepartmentName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Departments
            WHERE DivisionId = @DivisionId";

        var results = await QueryAsync<Department>(sql, new { DivisionId = divisionId });
        return results.ToList();
    }

    public async Task<Department> CreateAsync(Department department)
    {
        var sql = @"
            INSERT INTO Departments (DivisionId, DepartmentName, Code, Description, Status, CreatedAt)
            VALUES (@DivisionId, @DepartmentName, @Code, @Description, @Status, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            department.DivisionId,
            department.DepartmentName,
            department.Code,
            department.Description,
            Status = department.Status ?? "Active",
            CreatedAt = DateTime.UtcNow
        });

        department.DepartmentId = newId;
        return department;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        var sql = @"
            UPDATE Departments
            SET DivisionId = @DivisionId, DepartmentName = @DepartmentName, Code = @Code,
                Description = @Description, Status = @Status, UpdatedAt = @UpdatedAt
            WHERE DepartmentId = @DepartmentId";

        await ExecuteAsync(sql, new
        {
            DepartmentId = department.DepartmentId,
            department.DivisionId,
            department.DepartmentName,
            department.Code,
            department.Description,
            department.Status,
            UpdatedAt = DateTime.UtcNow
        });

        return department;
    }

    public async Task DeleteAsync(int id)
    {
        var sql = "DELETE FROM Departments WHERE DepartmentId = @DepartmentId";
        await ExecuteAsync(sql, new { DepartmentId = id });
    }
}