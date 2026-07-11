namespace HR_System.Application.DTOs.Reports;

public class MyLeaveReportOptions
{
    public string? EmployeeName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int AnnualTotal { get; set; }
    public int AnnualUsed { get; set; }
    public int AnnualBalance { get; set; }
    public int SickTotal { get; set; }
    public int SickUsed { get; set; }
    public int SickBalance { get; set; }
    public int TotalUsedDays { get; set; }
    public int TotalBalance { get; set; }
}
