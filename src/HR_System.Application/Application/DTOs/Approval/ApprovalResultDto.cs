namespace HR_System.Application.DTOs.Approval;

public class ApprovalResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int LeaveRequestId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
}