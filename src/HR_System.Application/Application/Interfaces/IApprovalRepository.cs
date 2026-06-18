using HR_System.Application.DTOs.Approval;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IApprovalRepository
{
    Task<ApprovalItem> CreateAsync(ApprovalItem item);
    Task<ApprovalItem> UpdateAsync(ApprovalItem item);
    Task<(List<ApprovalItemDto> Items, int Total)> GetPendingByApproverEmployeeIdAsync(int approverEmployeeId, int page, int limit);
    Task<List<ApprovalItemDto>> GetByLeaveRequestIdAsync(int leaveRequestId);
    Task<int> GetPendingCountByApproverEmployeeIdAsync(int approverEmployeeId);
}