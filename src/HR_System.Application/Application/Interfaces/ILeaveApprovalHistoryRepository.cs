using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface ILeaveApprovalHistoryRepository
{
    Task<LeaveApprovalHistory> CreateAsync(LeaveApprovalHistory history);
    Task<List<LeaveApprovalHistory>> GetByLeaveRequestIdAsync(Guid leaveRequestId);
    Task<LeaveApprovalHistory?> GetCurrentStepAsync(Guid leaveRequestId);
    Task<LeaveApprovalHistory?> GetLatestStepAsync(Guid leaveRequestId);
    Task UpdateAsync(LeaveApprovalHistory history);
}