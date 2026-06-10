namespace HR_System.Application.DTOs.Leave;

public class LeaveListResponse
{
    public List<LeaveRequestDto> Requests { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}
