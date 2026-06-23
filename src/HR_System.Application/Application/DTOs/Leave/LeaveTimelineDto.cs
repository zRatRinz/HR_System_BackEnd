namespace HR_System.Application.DTOs.Leave;

public class LeaveTimelineDto
{
    public int LeaveRequestId { get; set; }
    public List<LeaveApprovalStepDto> Steps { get; set; } = new();
}

public class LeaveApprovalStepDto
{
    public int StepNumber { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime? ActionAt { get; set; }
    public DateTime CreatedAt { get; set; }
}