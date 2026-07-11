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

    public async Task<ApprovalItem?> GetApprovalItemByIdAsync(int approvalItemId)
    {
        var sql = @"
            SELECT ApprovalItemId, LeaveRequestId, RequesterEmployeeId, ApproverEmployeeId, Type, Status, Comment, CreatedAt, UpdatedAt
            FROM ApprovalItems
            WHERE ApprovalItemId = @ApprovalItemId";

        return await QuerySingleOrDefaultAsync<ApprovalItem>(sql, new { ApprovalItemId = approvalItemId });
    }

    public async Task<(List<ApprovalItemDto> Items, int Total)> GetPendingByApproverEmployeeIdAsync(int approverEmployeeId, int page, int limit)
    {
        var countSql = "SELECT COUNT(*) FROM ApprovalItems WHERE ApproverEmployeeId = @ApproverEmployeeId";
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
            WHERE a.ApproverEmployeeId = @ApproverEmployeeId
            ORDER BY CASE WHEN a.Status = 'Pending' THEN 0 ELSE 1 END, a.CreatedAt DESC
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
                a.CreatedAt,
                lr.Reason
            FROM ApprovalItems a
            INNER JOIN Employees r ON a.RequesterEmployeeId = r.EmployeeId
            INNER JOIN Employees ap ON a.ApproverEmployeeId = ap.EmployeeId
            INNER JOIN LeaveRequests lr ON a.LeaveRequestId = lr.Id
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

    public async Task<LeaveApprovalHistory> CreateHistoryAsync(LeaveApprovalHistory history)
    {
        var sql = @"
            INSERT INTO LeaveApprovalHistory (LeaveRequestId, StepNumber, ApproverRole, ApproverId, Status, Comment, ActionAt, CreatedAt)
            VALUES (@LeaveRequestId, @StepNumber, @ApproverRole, @ApproverId, @Status, @Comment, @ActionAt, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            LeaveRequestId = history.LeaveRequestId,
            history.StepNumber,
            history.ApproverRole,
            history.ApproverId,
            history.Status,
            history.Comment,
            history.ActionAt,
            CreatedAt = DateTime.UtcNow
        });

        history.Id = newId;
        return history;
    }

    public async Task<List<LeaveApprovalHistory>> GetHistoryByLeaveRequestIdAsync(int leaveRequestId)
    {
        var sql = @"
            SELECT Id, LeaveRequestId, StepNumber, ApproverRole, ApproverId, Status, Comment, ActionAt, CreatedAt
            FROM LeaveApprovalHistory
            WHERE LeaveRequestId = @LeaveRequestId
            ORDER BY StepNumber";

        var results = await QueryAsync<LeaveApprovalHistory>(sql, new { LeaveRequestId = leaveRequestId });
        return results.ToList();
    }

    public async Task<LeaveApprovalHistory?> GetCurrentStepAsync(int leaveRequestId)
    {
        var sql = @"
            SELECT TOP 1 Id, LeaveRequestId, StepNumber, ApproverRole, ApproverId, Status, Comment, ActionAt, CreatedAt
            FROM LeaveApprovalHistory
            WHERE LeaveRequestId = @LeaveRequestId AND Status = 'Pending'
            ORDER BY StepNumber";

        return await QuerySingleOrDefaultAsync<LeaveApprovalHistory>(sql, new { LeaveRequestId = leaveRequestId });
    }

    public async Task<LeaveApprovalHistory?> GetLatestStepAsync(int leaveRequestId)
    {
        var sql = @"
            SELECT TOP 1 Id, LeaveRequestId, StepNumber, ApproverRole, ApproverId, Status, Comment, ActionAt, CreatedAt
            FROM LeaveApprovalHistory
            WHERE LeaveRequestId = @LeaveRequestId
            ORDER BY StepNumber DESC";

        return await QuerySingleOrDefaultAsync<LeaveApprovalHistory>(sql, new { LeaveRequestId = leaveRequestId });
    }

    public async Task<LeaveApprovalHistory?> GetNextWaitingStepAsync(int leaveRequestId)
    {
        var sql = @"
            SELECT TOP 1 Id, LeaveRequestId, StepNumber, ApproverRole, ApproverId, Status, Comment, ActionAt, CreatedAt
            FROM LeaveApprovalHistory
            WHERE LeaveRequestId = @LeaveRequestId AND Status = 'Waiting'
            ORDER BY StepNumber ASC";

        return await QuerySingleOrDefaultAsync<LeaveApprovalHistory>(sql, new { LeaveRequestId = leaveRequestId });
    }

    public async Task UpdateHistoryAsync(LeaveApprovalHistory history)
    {
        var sql = @"
            UPDATE LeaveApprovalHistory
            SET Status = @Status, Comment = @Comment, ActionAt = @ActionAt
            WHERE Id = @Id";

        await ExecuteAsync(sql, new
        {
            history.Id,
            history.Status,
            history.Comment,
            history.ActionAt
        });
    }

    public async Task<int> GetPendingCountForApproverAsync(int approverId)
    {
        var sql = @"
            SELECT COUNT(DISTINCT lah.LeaveRequestId)
            FROM LeaveApprovalHistory lah
            INNER JOIN (
                SELECT LeaveRequestId, MAX(StepNumber) as MaxStep
                FROM LeaveApprovalHistory
                WHERE Status = 'Pending'
                GROUP BY LeaveRequestId
            ) pending ON lah.LeaveRequestId = pending.LeaveRequestId AND lah.StepNumber = pending.MaxStep
            WHERE lah.ApproverId = @ApproverId AND lah.Status = 'Pending'";

        return await ExecuteScalarAsync<int>(sql, new { ApproverId = approverId });
    }

    public async Task<int> GetInProgressCountForApproverAsync(int approverId)
    {
        var sql = @"
            SELECT COUNT(DISTINCT sub.LeaveRequestId)
            FROM (
                SELECT lah.LeaveRequestId, lah.StepNumber,
                    MAX(CASE WHEN lah.Status = 'Approved' AND lah.ApproverId = @ApproverId THEN 1 ELSE 0 END) as UserApproved,
                    MAX(CASE WHEN lah.Status = 'Pending' THEN 1 ELSE 0 END) as HasPending
                FROM LeaveApprovalHistory lah
                GROUP BY lah.LeaveRequestId, lah.StepNumber
            ) sub
            WHERE sub.UserApproved = 1 AND sub.HasPending = 1";

        return await ExecuteScalarAsync<int>(sql, new { ApproverId = approverId });
    }

    public async Task<int> GetThisMonthApprovedCountForApproverAsync(int approverId)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var sql = @"
            SELECT COUNT(*)
            FROM LeaveApprovalHistory
            WHERE ApproverId = @ApproverId
              AND Status = 'Approved'
              AND ActionAt >= @StartOfMonth
              AND ActionAt < @EndOfMonth";

        return await ExecuteScalarAsync<int>(sql, new
        {
            ApproverId = approverId,
            StartOfMonth = startOfMonth,
            EndOfMonth = endOfMonth
        });
    }

    public async Task<int> GetThisMonthRejectedCountForApproverAsync(int approverId)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var sql = @"
            SELECT COUNT(*)
            FROM LeaveApprovalHistory
            WHERE ApproverId = @ApproverId
              AND Status = 'Rejected'
              AND ActionAt >= @StartOfMonth
              AND ActionAt < @EndOfMonth";

        return await ExecuteScalarAsync<int>(sql, new
        {
            ApproverId = approverId,
            StartOfMonth = startOfMonth,
            EndOfMonth = endOfMonth
        });
    }
}
