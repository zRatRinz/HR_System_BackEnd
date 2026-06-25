using HR_System.Application.DTOs.Leave;
using HR_System.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace HR_System.Application.UseCases.Leave;

public class LeaveCalendarUseCase
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IScopeService _scopeService;

    public LeaveCalendarUseCase(
        ILeaveRepository leaveRepository,
        IHolidayRepository holidayRepository,
        IEmployeeRepository employeeRepository,
        IScopeService scopeService)
    {
        _leaveRepository = leaveRepository;
        _holidayRepository = holidayRepository;
        _employeeRepository = employeeRepository;
        _scopeService = scopeService;
    }

    public async Task<LeaveCalendarDto> GetCalendarAsync(int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var leaveRequests = await _leaveRepository.GetByDateRangeDtoAsync(startDate, endDate);
        var approvedLeaves = leaveRequests.Where(l => l.Status.ToLower() == "approved").ToList();

        var roles = _scopeService.GetRoles();
        var currentEmployeeId = _scopeService.GetEmployeeId();
        var divisionId = _scopeService.GetDivisionId();
        var departmentId = _scopeService.GetDepartmentId();

        bool bypassScope = roles.Any(r => r == "Admin" || r == "Manager" || r == "HR");

        if (!bypassScope)
        {
            if (roles.Any(r => r == "HeadDivision") && divisionId.HasValue)
            {
                var employeeIds = await _employeeRepository.GetEmployeeIdsByDivisionAsync(divisionId.Value);
                approvedLeaves = approvedLeaves.Where(l => employeeIds.Contains(l.EmployeeId)).ToList();
            }
            else if (roles.Any(r => r == "HeadDepartment") && departmentId.HasValue)
            {
                var employeeIds = await _employeeRepository.GetEmployeeIdsByDepartmentAsync(departmentId.Value);
                approvedLeaves = approvedLeaves.Where(l => employeeIds.Contains(l.EmployeeId)).ToList();
            }
            else if (currentEmployeeId.HasValue)
            {
                approvedLeaves = approvedLeaves.Where(l => l.EmployeeId == currentEmployeeId.Value).ToList();
            }
            else
            {
                approvedLeaves = new List<LeaveRequestDto>();
            }
        }

        var leaves = new List<LeaveCalendarItemDto>();
        foreach (var request in approvedLeaves)
        {
            leaves.Add(new LeaveCalendarItemDto
            {
                EmployeeName = request.EmployeeName,
                LeaveType = request.LeaveType,
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                EndDate = request.EndDate.ToString("yyyy-MM-dd"),
                Days = request.Days,
                Reason = request.Reason
            });
        }

        var holidays = await _holidayRepository.GetByYearAsync(year);
        var holidayDtos = new List<HolidayCalendarDto>();
        foreach (var holiday in holidays)
        {
            var holidayDate = DateOnly.FromDateTime(holiday.HolidayDate);
            if (holidayDate.Month == month && holidayDate.Year == year)
            {
                holidayDtos.Add(new HolidayCalendarDto
                {
                    Date = holidayDate.ToString("yyyy-MM-dd"),
                    Name = holiday.HolidayName
                });
            }
        }

        return new LeaveCalendarDto
        {
            Month = month,
            Year = year,
            Holidays = holidayDtos.OrderBy(h => h.Date).ToList(),
            Leaves = leaves.OrderBy(l => l.StartDate).ToList()
        };
    }
}
