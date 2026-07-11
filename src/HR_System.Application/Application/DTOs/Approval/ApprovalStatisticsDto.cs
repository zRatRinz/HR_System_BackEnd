namespace HR_System.Application.DTOs.Approval;

public class ApprovalStatisticsDto
{
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int ThisMonthApproved { get; set; }
    public int ThisMonthRejected { get; set; }
}
