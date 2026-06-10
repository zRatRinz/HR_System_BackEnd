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

    public async Task<(List<PayrollDto> Items, int Total)> GetAllAsync(string? period, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId)
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
            }
        }

        if (!string.IsNullOrEmpty(period))
        {
            whereClause += " AND p.Period = @Period";
            parameters.Add("Period", period);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND p.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM PayrollRecords p INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId {whereClause}";
        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var dataSql = $@"
            SELECT p.PayrollRecordId as Id, p.EmployeeId, p.Period, p.BasicSalary as BaseSalary,
                   p.Deduction as Deductions, p.Allowance as Bonuses,
                   p.NetSalary, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY p.Period DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<PayrollDto>(dataSql, parameters);
        return (results.ToList(), total);
    }

    public async Task<PayrollRecord> CreateAsync(PayrollRecord record)
    {
        var sql = @"
            INSERT INTO PayrollRecords (EmployeeId, Period, BasicSalary, Allowance, Deduction, NetSalary, Status, CreatedAt)
            VALUES (@EmployeeId, @Period, @BasicSalary, @Allowance, @Deduction, @NetSalary, @Status, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            EmployeeId = record.EmployeeId,
            record.Period,
            record.BasicSalary,
            record.Allowance,
            record.Deduction,
            record.NetSalary,
            Status = record.Status.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        record.Id = Guid.Parse(newId.ToString());
        return record;
    }

    public async Task<PayrollRecord> UpdateAsync(PayrollRecord record)
    {
        var sql = @"
            UPDATE PayrollRecords
            SET BasicSalary = @BasicSalary, Allowance = @Allowance, Deduction = @Deduction,
                NetSalary = @NetSalary, Status = @Status, ProcessedAt = @ProcessedAt, UpdatedAt = @UpdatedAt
            WHERE PayrollRecordId = @PayrollRecordId";

        await ExecuteAsync(sql, new
        {
            PayrollRecordId = record.Id.ToString(),
            record.BasicSalary,
            record.Allowance,
            record.Deduction,
            record.NetSalary,
            Status = record.Status.ToString(),
            record.ProcessedAt,
            UpdatedAt = DateTime.UtcNow
        });

        return record;
    }

    public async Task<List<PayrollDto>> GetByPeriodAsDtoAsync(string period)
    {
        var sql = @"
            SELECT p.PayrollRecordId as Id, p.EmployeeId, p.Period, p.BasicSalary as BaseSalary,
                   p.Deduction as Deductions, p.Allowance as Bonuses,
                   p.NetSalary, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            WHERE p.Period = @Period
            ORDER BY e.FirstName";

        var results = await QueryAsync<PayrollDto>(sql, new { Period = period });
        return results.ToList();
    }

    public async Task<decimal> GetTotalPayrollForPeriodAsync(string period)
    {
        var sql = "SELECT COALESCE(SUM(NetSalary), 0) FROM PayrollRecords WHERE Period = @Period";
        return await ExecuteScalarAsync<decimal>(sql, new { Period = period });
    }

    public async Task<List<PayrollDto>> GetAllAsDtoAsync(string? period, int? employeeId, int page, int limit)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(period))
        {
            whereClause += " AND p.Period = @Period";
            parameters.Add("Period", period);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND p.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var sql = $@"
            SELECT p.PayrollRecordId as Id, p.EmployeeId, p.Period, p.BasicSalary as BaseSalary,
                   p.Deduction as Deductions, p.Allowance as Bonuses,
                   p.NetSalary, p.Status,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM PayrollRecords p
            INNER JOIN Employees e ON p.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY p.Period DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<PayrollDto>(sql, parameters);
        return results.ToList();
    }
}