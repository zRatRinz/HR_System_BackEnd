using HR_System.Application.DTOs.Leave;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface ILeaveRepository
{
    Task<(List<LeaveRequest> Items, int Total)> GetAllAsync(string? status, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId);
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
    Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest);
    Task<int> GetPendingCountAsync();
    Task<int> GetOnLeaveCountTodayAsync();
    Task<int> GetOnLeaveCountTodayWithScopeAsync(int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope);
    Task<List<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<LeaveRequestDto?> GetByIdAsDtoAsync(Guid id);
    Task<(List<LeaveRequestDto> Items, int Total)> GetAllAsDtoAsync(string? status, int? employeeId, int page, int limit);
    Task<Dictionary<string, int>> GetUsedDaysByTypeAsync(int employeeId);
    Task<Dictionary<string, int>> GetPendingDaysByTypeAsync(int employeeId);
    Task<LeaveRequest?> GetByIdAsync(Guid id);
    Task UpdateStatusAsync(Guid id, string status);
}
