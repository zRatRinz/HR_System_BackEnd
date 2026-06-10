namespace HR_System.Application.DTOs.Attendance;

public class AttendanceTodayStatsDto
{
    public int CheckedIn { get; set; }
    public int Late { get; set; }
    public int Absent { get; set; }
    public int OnLeave { get; set; }
}