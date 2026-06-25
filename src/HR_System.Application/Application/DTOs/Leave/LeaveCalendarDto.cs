namespace HR_System.Application.DTOs.Leave;

public class LeaveCalendarDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public List<HolidayCalendarDto> Holidays { get; set; } = new();
    public List<LeaveCalendarItemDto> Leaves { get; set; } = new();
}

public class HolidayCalendarDto
{
    public string Date { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LeaveCalendarItemDto
{
    public string? EmployeeName { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public int Days { get; set; }
    public string? Reason { get; set; }
}
