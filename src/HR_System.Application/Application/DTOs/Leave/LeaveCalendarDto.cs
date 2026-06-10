namespace HR_System.Application.DTOs.Leave;

public class LeaveCalendarDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public List<LeaveCalendarEventDto> Events { get; set; } = new();
}

public class LeaveCalendarEventDto
{
    public string Date { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? LeaveType { get; set; }
    public string? Name { get; set; }
}