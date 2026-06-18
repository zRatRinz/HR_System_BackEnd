namespace HR_System.Domain.Entities;

public class ApprovalItem
{
    public int ApprovalItemId { get; set; }
    public int LeaveRequestId { get; set; }
    public int RequesterEmployeeId { get; set; }
    public int ApproverEmployeeId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}