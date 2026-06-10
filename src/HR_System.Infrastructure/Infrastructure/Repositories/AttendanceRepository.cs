using HR_System.Application.DTOs.Attendance;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class AttendanceRepository : BaseRepository, IAttendanceRepository
{
    public AttendanceRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<AttendanceDto?> GetByEmployeeAndDateAsync(int employeeId, DateTime date)
    {
        var sql = @"
            SELECT a.AttendanceRecordId, a.EmployeeId, a.Date, a.CheckIn, a.CheckOut,
                   a.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM AttendanceRecords a
            INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
            WHERE a.EmployeeId = @EmployeeId AND CAST(a.Date AS DATE) = CAST(@Date AS DATE)";

        return await QuerySingleOrDefaultAsync<AttendanceDto>(sql, new { EmployeeId = employeeId, Date = date });
    }

    public async Task<AttendanceRecord> CreateAsync(AttendanceRecord record)
    {
        var sql = @"
            INSERT INTO AttendanceRecords (EmployeeId, Date, CheckIn, CheckOut, Status, Notes, CreatedAt)
            VALUES (@EmployeeId, @Date, @CheckIn, @CheckOut, @Status, @Notes, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            EmployeeId = record.EmployeeId,
            record.Date,
            record.CheckIn,
            record.CheckOut,
            Status = record.Status.ToString(),
            record.Notes,
            CreatedAt = DateTime.UtcNow
        });

        record.AttendanceRecordId = newId;
        return record;
    }

    public async Task<AttendanceRecord> UpdateAsync(AttendanceRecord record)
    {
        var sql = @"
            UPDATE AttendanceRecords
            SET CheckIn = @CheckIn, CheckOut = @CheckOut, Status = @Status,
                Notes = @Notes, UpdatedAt = @UpdatedAt
            WHERE AttendanceRecordId = @AttendanceRecordId";

        await ExecuteAsync(sql, new
        {
            AttendanceRecordId = record.AttendanceRecordId,
            record.CheckIn,
            record.CheckOut,
            Status = record.Status.ToString(),
            record.Notes,
            UpdatedAt = DateTime.UtcNow
        });

        return record;
    }

    public async Task<decimal> GetAttendanceRateAsync()
    {
        var totalSql = "SELECT COUNT(*) FROM AttendanceRecords";
        var totalRecords = await ExecuteScalarAsync<int>(totalSql);
        if (totalRecords == 0) return 100;

        var onTimeSql = "SELECT COUNT(*) FROM AttendanceRecords WHERE Status = 'OnTime'";
        var onTimeRecords = await ExecuteScalarAsync<int>(onTimeSql);

        return (decimal)onTimeRecords / totalRecords * 100;
    }

    public async Task<List<AttendanceDto>> GetAllAsDtoAsync(DateTime? date, int? employeeId, int page, int limit)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (date.HasValue)
        {
            whereClause += " AND CAST(a.Date AS DATE) = CAST(@Date AS DATE)";
            parameters.Add("Date", date.Value);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND a.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var sql = $@"
            SELECT a.AttendanceRecordId, a.EmployeeId, a.Date, a.CheckIn, a.CheckOut,
                   a.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM AttendanceRecords a
            INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY a.Date DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<AttendanceDto>(sql, parameters);
        return results.ToList();
    }

    public async Task<List<AttendanceDto>> GetAllAsDtoAsync(DateTime? date, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope, int? scopeEmployeeId)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (date.HasValue)
        {
            whereClause += " AND CAST(a.Date AS DATE) = CAST(@Date AS DATE)";
            parameters.Add("Date", date.Value);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND a.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        if (!bypassScope)
        {
            if (scopeEmployeeId.HasValue)
            {
                whereClause += " AND a.EmployeeId = @ScopeEmployeeId";
                parameters.Add("ScopeEmployeeId", scopeEmployeeId.Value);
            }
            else if (scopeDepartmentId.HasValue)
            {
                whereClause += " AND e.DepartmentId = @DepartmentId";
                parameters.Add("DepartmentId", scopeDepartmentId.Value);
            }
            else if (scopeDivisionId.HasValue)
            {
                whereClause += " AND e.DivisionId = @DivisionId";
                parameters.Add("DivisionId", scopeDivisionId.Value);
            }
        }

        var sql = $@"
            SELECT a.AttendanceRecordId, a.EmployeeId, a.Date, a.CheckIn, a.CheckOut,
                   a.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM AttendanceRecords a
            INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY a.Date DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<AttendanceDto>(sql, parameters);
        return results.ToList();
    }

    public async Task<(int CheckedIn, int Late)> GetTodayStatsAsync(int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope)
    {
        var today = DateTime.UtcNow.Date;
        var whereClause = "WHERE CAST(a.Date AS DATE) = @Today";
        var parameters = new DynamicParameters();
        parameters.Add("Today", today);

        if (!bypassScope)
        {
            if (scopeDepartmentId.HasValue)
            {
                whereClause += " AND e.DepartmentId = @DepartmentId";
                parameters.Add("DepartmentId", scopeDepartmentId.Value);
            }
            else if (scopeDivisionId.HasValue)
            {
                whereClause += " AND e.DivisionId = @DivisionId";
                parameters.Add("DivisionId", scopeDivisionId.Value);
            }
        }

        var checkedInSql = $@"
            SELECT COUNT(*) FROM AttendanceRecords a
            INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
            {whereClause}";

        var lateSql = $@"
            SELECT COUNT(*) FROM AttendanceRecords a
            INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
            {whereClause} AND a.Status = 'Late'";

        var checkedIn = await ExecuteScalarAsync<int>(checkedInSql, parameters);
        var late = await ExecuteScalarAsync<int>(lateSql, parameters);

        return (checkedIn, late);
    }

    public async Task<int> GetActiveEmployeeCountAsync(int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope)
    {
        var whereClause = "WHERE e.Status = 'Active'";
        var parameters = new DynamicParameters();

        if (!bypassScope)
        {
            if (scopeDepartmentId.HasValue)
            {
                whereClause += " AND e.DepartmentId = @DepartmentId";
                parameters.Add("DepartmentId", scopeDepartmentId.Value);
            }
            else if (scopeDivisionId.HasValue)
            {
                whereClause += " AND e.DivisionId = @DivisionId";
                parameters.Add("DivisionId", scopeDivisionId.Value);
            }
        }

        var sql = $@"
            SELECT COUNT(*) FROM Employees e
            {whereClause}";

        return await ExecuteScalarAsync<int>(sql, parameters);
    }
}