namespace HR_System.Domain.Entities;

public class PayrollLockedLeaves
{
    public int PayrollLockedLeaveId { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public int LeaveRequestId { get; set; }
    public DateTime LockedAt { get; set; }
}
