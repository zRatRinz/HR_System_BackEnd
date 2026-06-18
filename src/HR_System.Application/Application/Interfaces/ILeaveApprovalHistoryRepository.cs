using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface ILeaveApprovalHistoryRepository
{
    Task<LeaveApprovalHistory> CreateAsync(LeaveApprovalHistory history);
    Task<List<LeaveApprovalHistory>> GetByLeaveRequestIdAsync(int leaveRequestId);
    Task<LeaveApprovalHistory?> GetCurrentStepAsync(int leaveRequestId);
    Task<LeaveApprovalHistory?> GetLatestStepAsync(int leaveRequestId);
    Task UpdateAsync(LeaveApprovalHistory history);
}