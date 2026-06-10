using HR_System.Application.DTOs.Approval;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class ApprovalRepository : BaseRepository, IApprovalRepository
{
    public ApprovalRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<(List<ApprovalItemDto> Items, int Total)> GetAllAsync(ApprovalStatus? status, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        var bypassScope = role == "Admin";

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
                whereClause += " AND a.EmployeeId IN (SELECT EmployeeId FROM Employees WHERE UserId = @ScopeUserId)";
                parameters.Add("ScopeUserId", scopeUserId.Value);
            }

            if (scopeUserId.HasValue && (role == "Manager" || role == "HeadDepartment" || role == "HeadDivision"))
            {
                whereClause += " AND a.ApprovedBy = @ApproverId";
                parameters.Add("ApproverId", scopeUserId.Value);
            }
        }

        if (status.HasValue)
        {
            whereClause += " AND a.Status = @Status";
            parameters.Add("Status", status.Value.ToString());
        }

        var countSql = $"SELECT COUNT(*) FROM ApprovalItems a INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId {whereClause}";
        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var dataSql = $@"
            SELECT a.ApprovalItemId as Id, a.Type, a.EmployeeId, a.Title, a.Detail,
                   a.Status, a.CreatedAt as Date,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM ApprovalItems a
            INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY a.CreatedAt DESC";

        var results = await QueryAsync<ApprovalItemDto>(dataSql, parameters);
        return (results.ToList(), total);
    }

    public async Task<ApprovalItem> CreateAsync(ApprovalItem item)
    {
        var sql = @"
            INSERT INTO ApprovalItems (EmployeeId, Type, Title, Detail, Status, CreatedAt)
            VALUES (@EmployeeId, @Type, @Title, @Detail, @Status, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            EmployeeId = item.EmployeeId,
            Type = item.Type.ToString(),
            item.Title,
            item.Detail,
            Status = item.Status.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        item.Id = Guid.Parse(newId.ToString());
        return item;
    }

    public async Task<ApprovalItem> UpdateAsync(ApprovalItem item)
    {
        var sql = @"
            UPDATE ApprovalItems
            SET Type = @Type, Title = @Title, Detail = @Detail,
                Status = @Status, ApprovedBy = @ApprovedBy, ApprovedAt = @ApprovedAt, UpdatedAt = @UpdatedAt
            WHERE ApprovalItemId = @ApprovalItemId";

        await ExecuteAsync(sql, new
        {
            ApprovalItemId = item.Id.ToString(),
            Type = item.Type.ToString(),
            item.Title,
            item.Detail,
            Status = item.Status.ToString(),
            ApprovedBy = item.ApprovedBy,
            item.ApprovedAt,
            UpdatedAt = DateTime.UtcNow
        });

        return item;
    }

    public async Task<int> GetPendingCountAsync()
    {
        var sql = "SELECT COUNT(*) FROM ApprovalItems WHERE Status = 'Pending'";
        return await ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<ApprovalItemDto>> GetAllAsDtoAsync(ApprovalStatus? status)
    {
        var whereClause = status.HasValue ? "WHERE a.Status = @Status" : "WHERE 1=1";

        var sql = $@"
            SELECT a.ApprovalItemId as Id, a.Type, a.EmployeeId, a.Title, a.Detail,
                   a.Status, a.CreatedAt as Date,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM ApprovalItems a
            INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY a.CreatedAt DESC";

        var results = await QueryAsync<ApprovalItemDto>(sql, status.HasValue ? new { Status = status.Value.ToString() } : null);
        return results.ToList();
    }
}