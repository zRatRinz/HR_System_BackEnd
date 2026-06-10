namespace HR_System.Domain.Entities;

public class LeaveApprovalHistory
{
    public int Id { get; set; }
    public Guid LeaveRequestId { get; set; }
    public int StepNumber { get; set; }
    public string ApproverRole { get; set; } = "";
    public int? ApproverId { get; set; }
    public string Status { get; set; } = "";
    public string? Comment { get; set; }
    public DateTime? ActionAt { get; set; }
    public DateTime CreatedAt { get; set; }
}