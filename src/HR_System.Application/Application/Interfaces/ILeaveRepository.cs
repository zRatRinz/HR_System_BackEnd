using HR_System.Application.DTOs.Leave;
using HR_System.Application.DTOs.Reports;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface ILeaveRepository
{
    Task<(List<LeaveRequest> Items, int Total)> GetAllAsync(string? status, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId);
    Task<LeaveCertificateDto?> GetCertificateByIdAsync(int leaveRequestId);
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
    Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest);
    Task<int> GetPendingCountAsync();
    Task<int> GetOnLeaveCountTodayAsync();
    Task<int> GetOnLeaveCountTodayWithScopeAsync(int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope);
    Task<List<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<LeaveRequestDto>> GetByDateRangeDtoAsync(DateTime startDate, DateTime endDate);

    Task<LeaveRequestDto?> GetByIdAsDtoAsync(int id);
    Task<(List<LeaveRequestDto> Items, int Total)> GetAllAsDtoAsync(string? status, string? leaveType, DateTime? startDate, DateTime? endDate, int? employeeId, int page, int limit);
    Task<(List<LeaveRequestDto> Items, int Total)> GetAllAsDtoAsync(string? status, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope);
    Task<Dictionary<string, int>> GetUsedDaysByTypeAsync(int employeeId);
    Task<Dictionary<string, int>> GetPendingDaysByTypeAsync(int employeeId);
    Task<int> GetPendingRequestsCountAsync(int employeeId);
    Task<int> GetLeaveTakenYtdAsync(int employeeId);
    Task UpdateStatusAsync(int id, string status);
    Task<int> GetApprovedLeaveTotalDaysAsync(int employeeId, DateTime startDate, DateTime endDate, string leaveType);
    Task<List<LeaveRequestDto>> GetApprovedLeavesByEmployeeIdAsync(int employeeId, DateTime? startDate, DateTime? endDate);
    Task<List<LeaveRequestDto>> GetApprovedLeavesAsync(DateTime? startDate, DateTime? endDate, bool bypassScope, int? scopeDivisionId, int? scopeDepartmentId, int? scopeUserId);

    Task<bool> IsLeaveLockedAsync(int leaveRequestId);
    Task LockLeavesAsync(int month, int year, List<int> leaveRequestIds);
    Task UnlockLeavesAsync(int month, int year);
    Task<List<int>> GetLockedLeaveIdsAsync(int month, int year);
    Task<bool> IsPayrollProcessedAsync(int month, int year);
    Task<List<LeaveRequestDto>> GetApprovedLeavesInMonthAsync(int month, int year);
}
