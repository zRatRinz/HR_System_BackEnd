namespace HR_System.Application.DTOs.Approval;

public class ApprovalListResponse
{
    public List<ApprovalItemDto> Approvals { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}
