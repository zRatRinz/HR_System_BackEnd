namespace HR_System.Application.DTOs.Reports;

public class AttendanceOverviewReportOptions
{
    public string? DivisionName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
}