using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class DivisionRepository : BaseRepository, IDivisionRepository
{
    public DivisionRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Division?> GetByIdAsync(int id)
    {
        var sql = @"
            SELECT DivisionId, DivisionName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Divisions
            WHERE DivisionId = @DivisionId";

        return await QuerySingleOrDefaultAsync<Division>(sql, new { DivisionId = id });
    }

    public async Task<List<Division>> GetAllAsync()
    {
        var sql = @"
            SELECT DivisionId, DivisionName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Divisions";

        var results = await QueryAsync<Division>(sql);
        return results.ToList();
    }

    public async Task<Division> CreateAsync(Division division)
    {
        var sql = @"
            INSERT INTO Divisions (DivisionName, Code, Description, Status, CreatedAt)
            VALUES (@DivisionName, @Code, @Description, @Status, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            division.DivisionName,
            division.Code,
            division.Description,
            Status = division.Status ?? "Active",
            CreatedAt = DateTime.UtcNow
        });

        division.DivisionId = newId;
        return division;
    }

    public async Task<Division> UpdateAsync(Division division)
    {
        var sql = @"
            UPDATE Divisions
            SET DivisionName = @DivisionName, Code = @Code, Description = @Description,
                Status = @Status, UpdatedAt = @UpdatedAt
            WHERE DivisionId = @DivisionId";

        await ExecuteAsync(sql, new
        {
            DivisionId = division.DivisionId,
            division.DivisionName,
            division.Code,
            division.Description,
            division.Status,
            UpdatedAt = DateTime.UtcNow
        });

        return division;
    }

    public async Task DeleteAsync(int id)
    {
        var sql = "DELETE FROM Divisions WHERE DivisionId = @DivisionId";
        await ExecuteAsync(sql, new { DivisionId = id });
    }
}