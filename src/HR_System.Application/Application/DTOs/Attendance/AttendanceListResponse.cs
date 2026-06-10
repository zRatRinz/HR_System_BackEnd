namespace HR_System.Application.DTOs.Attendance;

public class AttendanceListResponse
{
    public List<AttendanceDto> Attendance { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
}
