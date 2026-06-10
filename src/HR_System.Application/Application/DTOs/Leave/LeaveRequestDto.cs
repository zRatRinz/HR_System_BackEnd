namespace HR_System.Application.DTOs.Leave;

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
