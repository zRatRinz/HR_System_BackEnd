using HR_System.Application.DTOs.Leave;

namespace HR_System.Application.DTOs.Approval;

public class LeaveRequestApprovalDto
{
    public int LeaveRequestId { get; set; }
    public int RequesterEmployeeId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public List<LeaveApprovalStepDto> Timeline { get; set; } = new();
}
