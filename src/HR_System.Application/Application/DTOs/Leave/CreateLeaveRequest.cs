namespace HR_System.Application.DTOs.Leave;

public class CreateLeaveRequest
{
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}
