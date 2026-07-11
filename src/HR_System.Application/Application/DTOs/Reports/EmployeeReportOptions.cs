namespace HR_System.Application.DTOs.Reports;

public class EmployeeReportOptions
{
    public string? DivisionName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Status { get; set; }
    public DateTime GeneratedAt { get; set; }
}