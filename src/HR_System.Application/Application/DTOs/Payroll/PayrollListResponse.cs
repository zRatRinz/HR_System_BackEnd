namespace HR_System.Application.DTOs.Payroll;

public class PayrollListResponse
{
    public List<PayrollDto> Payrolls { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}
