namespace HR_System.Application.DTOs.Leave;

public class LeaveBalanceDto
{
    public LeaveBalanceItemDto Annual { get; set; } = new();
    public LeaveBalanceItemDto Sick { get; set; } = new();
    public LeaveBalanceItemDto Personal { get; set; } = new();
}

public class LeaveBalanceItemDto
{
    public int Total { get; set; }
    public int Used { get; set; }
    public int Pending { get; set; }
    public int Balance { get; set; }
}