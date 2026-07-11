namespace HR_System.Application.DTOs.Reports;

public class MyAttendanceReportOptions
{
    public string? EmployeeName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int OnTimeCount { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int SickLeaveDays { get; set; }
    public int PersonalLeaveDays { get; set; }
}