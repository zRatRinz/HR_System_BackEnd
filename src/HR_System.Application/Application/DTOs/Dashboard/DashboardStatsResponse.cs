namespace HR_System.Application.DTOs.Dashboard;

public class DashboardStatsResponse
{
    public int TotalEmployees { get; set; }
    public int OnLeaveToday { get; set; }
    public int PendingApprovals { get; set; }
    public string AttendanceRate { get; set; } = string.Empty;
    public string MonthlyPayroll { get; set; } = string.Empty;
}
