using HR_System.Application.DTOs.Approval;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IApprovalRepository
{
    Task<ApprovalItem> CreateAsync(ApprovalItem item);
    Task<ApprovalItem> UpdateAsync(ApprovalItem item);
    Task<ApprovalItem?> GetApprovalItemByIdAsync(int approvalItemId);
    Task<(List<ApprovalItemDto> Items, int Total)> GetPendingByApproverEmployeeIdAsync(int approverEmployeeId, int page, int limit);
    Task<List<ApprovalItemDto>> GetByLeaveRequestIdAsync(int leaveRequestId);
    Task<int> GetPendingCountByApproverEmployeeIdAsync(int approverEmployeeId);

    Task<LeaveApprovalHistory> CreateHistoryAsync(LeaveApprovalHistory history);
    Task<List<LeaveApprovalHistory>> GetHistoryByLeaveRequestIdAsync(int leaveRequestId);
    Task<LeaveApprovalHistory?> GetCurrentStepAsync(int leaveRequestId);
    Task<LeaveApprovalHistory?> GetLatestStepAsync(int leaveRequestId);
    Task<LeaveApprovalHistory?> GetNextWaitingStepAsync(int leaveRequestId);
    Task UpdateHistoryAsync(LeaveApprovalHistory history);
}
