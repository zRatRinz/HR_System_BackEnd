using HR_System.Application.DTOs.Approval;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class ApprovalRepository : BaseRepository, IApprovalRepository
{
    public ApprovalRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<ApprovalItem> CreateAsync(ApprovalItem item)
    {
        var sql = @"
            INSERT INTO ApprovalItems (LeaveRequestId, RequesterEmployeeId, ApproverEmployeeId, Type, Status, Comment, CreatedAt)
            VALUES (@LeaveRequestId, @RequesterEmployeeId, @ApproverEmployeeId, @Type, @Status, @Comment, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            LeaveRequestId = item.LeaveRequestId,
            RequesterEmployeeId = item.RequesterEmployeeId,
            ApproverEmployeeId = item.ApproverEmployeeId,
            Type = item.Type,
            Status = item.Status,
            Comment = item.Comment,
            CreatedAt = DateTime.UtcNow
        });

        item.ApprovalItemId = newId;
        return item;
    }

    public async Task<ApprovalItem> UpdateAsync(ApprovalItem item)
    {
        var sql = @"
            UPDATE ApprovalItems
            SET Status = @Status, Comment = @Comment, UpdatedAt = @UpdatedAt
            WHERE ApprovalItemId = @ApprovalItemId";

        await ExecuteAsync(sql, new
        {
            ApprovalItemId = item.ApprovalItemId,
            Status = item.Status,
            Comment = item.Comment,
            UpdatedAt = DateTime.UtcNow
        });

        return item;
    }

    public async Task<(List<ApprovalItemDto> Items, int Total)> GetPendingByApproverEmployeeIdAsync(int approverEmployeeId, int page, int limit)
    {
        var countSql = "SELECT COUNT(*) FROM ApprovalItems WHERE ApproverEmployeeId = @ApproverEmployeeId AND Status = 'Pending'";
        var total = await ExecuteScalarAsync<int>(countSql, new { ApproverEmployeeId = approverEmployeeId });

        var sql = @"
            SELECT 
                a.ApprovalItemId,
                a.LeaveRequestId,
                a.RequesterEmployeeId,
                (r.FirstName + ' ' + r.LastName) AS RequesterName,
                a.ApproverEmployeeId,
                (ap.FirstName + ' ' + ap.LastName) AS ApproverName,
                a.Type,
                a.Status,
                a.Comment,
                a.CreatedAt
            FROM ApprovalItems a
            INNER JOIN Employees r ON a.RequesterEmployeeId = r.EmployeeId
            INNER JOIN Employees ap ON a.ApproverEmployeeId = ap.EmployeeId
            WHERE a.ApproverEmployeeId = @ApproverEmployeeId AND a.Status = 'Pending'
            ORDER BY a.CreatedAt DESC
            OFFSET (@Page-1)*@Limit ROWS FETCH NEXT @Limit ROWS ONLY";

        var results = await QueryAsync<ApprovalItemDto>(sql, new { ApproverEmployeeId = approverEmployeeId, Page = page, Limit = limit });
        return (results.ToList(), total);
    }

    public async Task<List<ApprovalItemDto>> GetByLeaveRequestIdAsync(int leaveRequestId)
    {
        var sql = @"
            SELECT 
                a.ApprovalItemId,
                a.LeaveRequestId,
                a.RequesterEmployeeId,
                (r.FirstName + ' ' + r.LastName) AS RequesterName,
                a.ApproverEmployeeId,
                (ap.FirstName + ' ' + ap.LastName) AS ApproverName,
                a.Type,
                a.Status,
                a.Comment,
                a.CreatedAt
            FROM ApprovalItems a
            INNER JOIN Employees r ON a.RequesterEmployeeId = r.EmployeeId
            INNER JOIN Employees ap ON a.ApproverEmployeeId = ap.EmployeeId
            WHERE a.LeaveRequestId = @LeaveRequestId
            ORDER BY a.CreatedAt ASC";

        var results = await QueryAsync<ApprovalItemDto>(sql, new { LeaveRequestId = leaveRequestId });
        return results.ToList();
    }

    public async Task<int> GetPendingCountByApproverEmployeeIdAsync(int approverEmployeeId)
    {
        var sql = "SELECT COUNT(*) FROM ApprovalItems WHERE ApproverEmployeeId = @ApproverEmployeeId AND Status = 'Pending'";
        return await ExecuteScalarAsync<int>(sql, new { ApproverEmployeeId = approverEmployeeId });
    }
}