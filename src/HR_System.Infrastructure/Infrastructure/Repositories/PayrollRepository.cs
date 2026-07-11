using HR_System.Application.DTOs.Payroll;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class PayrollRepository : BaseRepository, IPayrollRepository
{
    public PayrollRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<(List<PayrollDto> Items, int Total)> GetAllAsync(int? month, int? year, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        var bypassScope = role == "Admin" || role == "HR";

        if (!bypassScope)
        {
            if (role == "HeadDivision" && scopeDivisionId.HasValue)
            {
                whereClause += " AND e.DivisionId = @ScopeDivisionId";
                parameters.Add("ScopeDivisionId", scopeDivisionId.Value);
            }
            else if ((role == "HeadDepartment" || role == "Manager") && scopeDepartmentId.HasValue)
            {
                whereClause += " AND e.DepartmentId = @ScopeDepartmentId";
                parameters.Add("ScopeDepartmentId", scopeDepartmentId.Value);
            }
            else if (role == "Employee" && scopeUserId.HasValue)
            {
                whereClause += " AND p.EmployeeId IN (SELECT EmployeeId FROM Employees WHERE UserId = @ScopeUserId)";
                parameters.Add("ScopeUserId", scopeUserId.Value);
                whereClause += " AND p.Status = 'Approved'";
            }
        }

        if (month.HasValue)
        {
            whereClause += " AND p.Month = @Month";
            parameters.Add("Month", month.Value);
        }

        if (year.HasValue)
        {
            whereClause += " AND p.Year = @Year";
            parameters.Add("Year", year.Value);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND p.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM PayrollRecords p INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId {whereClause}";
        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var dataSql = $@"
            SELECT p.PayrollRecordId as Id, p.EmployeeId, p.Month, p.Year, p.BasicSalary,
                   p.Deduction, p.Allowance,
                   p.NetSalary, p.UnpaidLeaveDays, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY p.Year DESC, p.Month DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<PayrollDto>(dataSql, parameters);
        return (results.ToList(), total);
    }

    public async Task<PayrollRecord> CreateAsync(PayrollRecord record)
    {
        var sql = @"
            INSERT INTO PayrollRecords (EmployeeId, Month, Year, Period, BasicSalary, Allowance, Deduction, NetSalary, UnpaidLeaveDays, Status, ProcessedAt, CreatedAt)
            VALUES (@EmployeeId, @Month, @Year, @Period, @BasicSalary, @Allowance, @Deduction, @NetSalary, @UnpaidLeaveDays, @Status, @ProcessedAt, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            record.EmployeeId,
            record.Month,
            record.Year,
            record.Period,
            record.BasicSalary,
            record.Allowance,
            record.Deduction,
            record.NetSalary,
            record.UnpaidLeaveDays,
            Status = record.Status.ToString(),
            record.ProcessedAt,
            CreatedAt = DateTime.UtcNow
        });

        record.Id = newId;
        return record;
    }

    public async Task<PayrollRecord> UpdateAsync(PayrollRecord record)
    {
        var sql = @"
            UPDATE PayrollRecords
            SET BasicSalary = @BasicSalary, Allowance = @Allowance, Deduction = @Deduction,
                NetSalary = @NetSalary, UnpaidLeaveDays = @UnpaidLeaveDays, Status = @Status,
                ProcessedAt = @ProcessedAt, UpdatedAt = @UpdatedAt
            WHERE PayrollRecordId = @PayrollRecordId";

        await ExecuteAsync(sql, new
        {
            PayrollRecordId = record.Id.ToString(),
            record.BasicSalary,
            record.Allowance,
            record.Deduction,
            record.NetSalary,
            record.UnpaidLeaveDays,
            Status = record.Status.ToString(),
            record.ProcessedAt,
            UpdatedAt = DateTime.UtcNow
        });

        return record;
    }

    public async Task<List<PayrollDto>> GetByMonthYearAsDtoAsync(int month, int year)
    {
        var sql = @"
            SELECT p.PayrollRecordId as Id, p.EmployeeId, p.Month, p.Year, p.BasicSalary,
                   p.Deduction, p.Allowance,
                   p.NetSalary, p.UnpaidLeaveDays, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            WHERE p.Month = @Month AND p.Year = @Year
            ORDER BY e.FirstName";

        var results = await QueryAsync<PayrollDto>(sql, new { Month = month, Year = year });
        return results.ToList();
    }

    public async Task<decimal> GetTotalPayrollForMonthYearAsync(int month, int year)
    {
        var sql = "SELECT COALESCE(SUM(NetSalary), 0) FROM PayrollRecords WHERE Month = @Month AND Year = @Year";
        return await ExecuteScalarAsync<decimal>(sql, new { Month = month, Year = year });
    }

    public async Task<(List<PayrollDto> Items, int Total)> GetAllAsDtoAsync(int? month, int? year, int? employeeId, int page, int limit)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (month.HasValue)
        {
            whereClause += " AND p.Month = @Month";
            parameters.Add("Month", month.Value);
        }

        if (year.HasValue)
        {
            whereClause += " AND p.Year = @Year";
            parameters.Add("Year", year.Value);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND p.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM PayrollRecords p INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId {whereClause}";
        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var sql = $@"
            SELECT p.PayrollRecordId, p.EmployeeId, p.Month, p.Year, p.BasicSalary,
                   p.Deduction, p.Allowance,
                   p.NetSalary, p.UnpaidLeaveDays, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY p.Year DESC, p.Month DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<PayrollDto>(sql, parameters);
        return (results.ToList(), total);
    }

    public async Task<PayrollDto?> GetByIdAsDtoAsync(int id)
    {
        var sql = @"
            SELECT p.PayrollRecordId as Id, p.EmployeeId, p.Month, p.Year, p.BasicSalary,
                   p.Deduction, p.Allowance,
                   p.NetSalary, p.UnpaidLeaveDays, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            WHERE p.PayrollRecordId = @PayrollRecordId";

        return await QuerySingleOrDefaultAsync<PayrollDto>(sql, new { PayrollRecordId = id });
    }

    public async Task<PayrollDto?> GetByMonthYearEmployeeAsync(int month, int year, int employeeId)
    {
        var sql = @"
            SELECT p.PayrollRecordId as Id, p.EmployeeId, p.Month, p.Year, p.BasicSalary,
                   p.Deduction, p.Allowance,
                   p.NetSalary, p.UnpaidLeaveDays, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            WHERE p.Month = @Month AND p.Year = @Year AND p.EmployeeId = @EmployeeId";

        return await QuerySingleOrDefaultAsync<PayrollDto>(sql, new { Month = month, Year = year, EmployeeId = employeeId });
    }

    public async Task DeleteByMonthYearAsync(int month, int year)
    {
        var sql = "DELETE FROM PayrollRecords WHERE Month = @Month AND Year = @Year";
        await ExecuteAsync(sql, new { Month = month, Year = year });
    }

    public async Task ApproveAsync(int month, int year)
    {
        var sql = "UPDATE PayrollRecords SET Status = 'Approved' WHERE Month = @Month AND Year = @Year";
        await ExecuteAsync(sql, new { Month = month, Year = year });
    }

    public async Task<int> ApproveByIdsAsync(List<int> payrollRecordIds)
    {
        if (!payrollRecordIds.Any())
            return 0;

        var sql = @"UPDATE PayrollRecords
                    SET Status = 'Approved'
                    WHERE PayrollRecordId IN @Ids";
        return await ExecuteAsync(sql, new { Ids = payrollRecordIds });
    }

    public async Task<PayrollDetailDto?> GetDetailAsync(int month, int year, int employeeId)
    {
        var sql = @"
            SELECT p.PayrollRecordId, p.EmployeeId, p.Month, p.Year, p.Period,
                   p.BasicSalary, p.Allowance, p.Deduction, p.NetSalary,
                   p.UnpaidLeaveDays, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            WHERE p.Month = @Month AND p.Year = @Year AND p.EmployeeId = @EmployeeId";

        return await QuerySingleOrDefaultAsync<PayrollDetailDto>(sql,
            new { Month = month, Year = year, EmployeeId = employeeId });
    }
}
