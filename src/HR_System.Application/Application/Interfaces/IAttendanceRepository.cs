using HR_System.Application.DTOs.Attendance;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IAttendanceRepository
{
    Task<AttendanceDto?> GetByEmployeeAndDateAsync(int employeeId, DateTime date);
    Task<AttendanceRecord> CreateAsync(AttendanceRecord record);
    Task<AttendanceRecord> UpdateAsync(AttendanceRecord record);
    Task<decimal> GetAttendanceRateAsync();

    Task<List<AttendanceDto>> GetAllAsDtoAsync(DateTime? startDate, DateTime? endDate, int? employeeId, int page, int limit);
    Task<(List<AttendanceDto> Items, int Total)> GetAllAsDtoAsync(DateTime? startDate, DateTime? endDate, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope, int? scopeEmployeeId, string? status);
    Task<(List<AttendanceDto> Items, int Total)> GetByEmployeeIdAsync(int employeeId, DateTime? startDate, DateTime? endDate, int page, int limit);
    Task<(int CheckedIn, int Late)> GetTodayStatsAsync(int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope);
    Task<int> GetActiveEmployeeCountAsync(int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope);
}
