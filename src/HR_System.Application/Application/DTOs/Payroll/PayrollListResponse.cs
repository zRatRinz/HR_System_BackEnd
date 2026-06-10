namespace HR_System.Application.DTOs.Payroll;

public class PayrollListResponse
{
    public List<PayrollDto> Data { get; set; } = new();
    public int Total { get; set; }
}
