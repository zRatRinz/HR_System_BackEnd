using HR_System.Application.DTOs.Approval;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;

namespace HR_System.Application.Interfaces;

public interface IApprovalRepository
{
    Task<(List<ApprovalItemDto> Items, int Total)> GetAllAsync(ApprovalStatus? status, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId);
    Task<ApprovalItem> CreateAsync(ApprovalItem item);
    Task<ApprovalItem> UpdateAsync(ApprovalItem item);
    Task<int> GetPendingCountAsync();

    Task<List<ApprovalItemDto>> GetAllAsDtoAsync(ApprovalStatus? status);
}
