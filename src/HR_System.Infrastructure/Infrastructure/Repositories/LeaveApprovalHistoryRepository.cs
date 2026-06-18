using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class LeaveApprovalHistoryRepository : BaseRepository, ILeaveApprovalHistoryRepository
{
    public LeaveApprovalHistoryRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<LeaveApprovalHistory> CreateAsync(LeaveApprovalHistory history)
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

    public async Task<List<LeaveApprovalHistory>> GetByLeaveRequestIdAsync(int leaveRequestId)
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

    public async Task UpdateAsync(LeaveApprovalHistory history)
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
}