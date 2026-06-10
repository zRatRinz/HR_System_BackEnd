using HR_System.Application.DTOs.Attendance;
using HR_System.Application.Interfaces;

namespace HR_System.Application.UseCases.Attendance;

public class AttendanceTodayStatsUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILeaveRepository _leaveRepository;
    private readonly IScopeService _scopeService;

    public AttendanceTodayStatsUseCase(
        IAttendanceRepository attendanceRepository,
        ILeaveRepository leaveRepository,
        IScopeService scopeService)
    {
        _attendanceRepository = attendanceRepository;
        _leaveRepository = leaveRepository;
        _scopeService = scopeService;
    }

    public async Task<AttendanceTodayStatsDto> GetTodayStatsAsync()
    {
        var roles = _scopeService.GetRoles();
        var bypassScope = roles.Any(r => r == "HR" || r == "Manager" || r == "Admin");

        var divisionId = bypassScope ? null : _scopeService.GetDivisionId();
        var departmentId = bypassScope ? null : _scopeService.GetDepartmentId();

        var (checkedIn, late) = await _attendanceRepository.GetTodayStatsAsync(divisionId, departmentId, bypassScope);
        var onLeave = await _leaveRepository.GetOnLeaveCountTodayWithScopeAsync(divisionId, departmentId, bypassScope);
        var totalActiveEmployees = await _attendanceRepository.GetActiveEmployeeCountAsync(divisionId, departmentId, bypassScope);

        var absent = totalActiveEmployees - checkedIn - onLeave;
        if (absent < 0) absent = 0;

        return new AttendanceTodayStatsDto
        {
            CheckedIn = checkedIn,
            Late = late,
            Absent = absent,
            OnLeave = onLeave
        };
    }
}