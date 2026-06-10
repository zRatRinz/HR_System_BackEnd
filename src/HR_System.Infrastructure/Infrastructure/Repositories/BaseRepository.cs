using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace HR_System.Infrastructure.Repositories;

public abstract class BaseRepository
{
    private readonly string _connectionString;

    protected BaseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    protected IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<T>(sql, param);
    }

    protected async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(sql, param);
    }

    protected async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, param);
    }

    protected async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<T>(sql, param);
    }
}
