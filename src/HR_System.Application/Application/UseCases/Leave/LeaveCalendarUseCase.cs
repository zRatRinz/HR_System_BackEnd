using HR_System.Application.DTOs.Leave;
using HR_System.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HR_System.Application.UseCases.Leave;

public class LeaveCalendarUseCase
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly IConfiguration _configuration;

    public LeaveCalendarUseCase(ILeaveRepository leaveRepository, IConfiguration configuration)
    {
        _leaveRepository = leaveRepository;
        _configuration = configuration;
    }

    public async Task<LeaveCalendarDto> GetCalendarAsync(int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var leaveRequests = await _leaveRepository.GetByDateRangeAsync(startDate, endDate);

        var events = new List<LeaveCalendarEventDto>();

        foreach (var request in leaveRequests.Where(l => l.Status == Domain.Enums.LeaveStatus.Approved))
        {
            for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
            {
                events.Add(new LeaveCalendarEventDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Type = "leave",
                    LeaveType = request.LeaveType.ToString()
                });
            }
        }

        var holidays = _configuration.GetSection("Holidays").Get<List<HolidayConfig>>() ?? new();
        foreach (var holiday in holidays.Where(h => h.Date.HasValue))
        {
            var holidayDate = holiday.Date!.Value;
            if (holidayDate.Month == month && holidayDate.Year == year)
            {
                events.Add(new LeaveCalendarEventDto
                {
                    Date = holidayDate.ToString("yyyy-MM-dd"),
                    Type = "holiday",
                    Name = holiday.Name
                });
            }
        }

        return new LeaveCalendarDto
        {
            Month = month,
            Year = year,
            Events = events.OrderBy(e => e.Date).ToList()
        };
    }

    private class HolidayConfig
    {
        public DateTime? Date { get; set; }
        public string? Name { get; set; }
    }
}