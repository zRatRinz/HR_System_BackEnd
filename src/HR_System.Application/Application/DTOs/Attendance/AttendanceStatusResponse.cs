namespace HR_System.Application.DTOs.Attendance;

public class AttendanceStatusResponse
{
    public bool HasCheckedIn { get; set; }
    public bool HasCheckedOut { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Status { get; set; } = string.Empty;
}