using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class PositionRepository : BaseRepository, IPositionRepository
{
    public PositionRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Position> CreateAsync(Position position)
    {
        var sql = @"
            INSERT INTO Positions (PositionName, Code, Description, Status, CreatedAt)
            VALUES (@PositionName, @Code, @Description, @Status, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            position.PositionName,
            position.Code,
            position.Description,
            Status = position.Status ?? "Active",
            CreatedAt = DateTime.UtcNow
        });

        position.PositionId = newId;
        return position;
    }

    public async Task<Position> UpdateAsync(Position position)
    {
        var sql = @"
            UPDATE Positions
            SET PositionName = @PositionName, Code = @Code, Description = @Description,
                Status = @Status, UpdatedAt = @UpdatedAt
            WHERE PositionId = @PositionId";

        await ExecuteAsync(sql, new
        {
            PositionId = position.PositionId,
            position.PositionName,
            position.Code,
            position.Description,
            Status = position.Status ?? "Active",
            UpdatedAt = DateTime.UtcNow
        });

        return position;
    }

    public async Task<Position?> GetByIdAsync(int id)
    {
        var sql = @"
            SELECT PositionId, PositionName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Positions
            WHERE PositionId = @PositionId";

        return await QuerySingleOrDefaultAsync<Position>(sql, new { PositionId = id });
    }

    public async Task<Position?> GetByCodeAsync(string code)
    {
        var sql = @"
            SELECT PositionId, PositionName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Positions
            WHERE Code = @Code";

        return await QuerySingleOrDefaultAsync<Position>(sql, new { Code = code });
    }

    public async Task<List<Position>> GetAllAsync()
    {
        var sql = @"
            SELECT PositionId, PositionName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Positions
            ORDER BY PositionName";

        var results = await QueryAsync<Position>(sql);
        return results.ToList();
    }

    public async Task<List<Position>> GetActiveAsync()
    {
        var sql = @"
            SELECT PositionId, PositionName, Code, Description, Status, CreatedAt, UpdatedAt
            FROM Positions
            WHERE Status = 'Active'
            ORDER BY PositionName";

        var results = await QueryAsync<Position>(sql);
        return results.ToList();
    }
}