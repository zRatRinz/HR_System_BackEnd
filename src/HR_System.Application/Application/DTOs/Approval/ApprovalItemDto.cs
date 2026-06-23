namespace HR_System.Application.DTOs.Approval;

public class ApprovalItemDto
{
    public int ApprovalItemId { get; set; }
    public int LeaveRequestId { get; set; }
    public int RequesterEmployeeId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public int ApproverEmployeeId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Reason { get; set; }
}