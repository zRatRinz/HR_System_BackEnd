namespace HR_System.Application.DTOs.Payroll;

public class ApprovePayrollRequest
{
    public List<int> PayrollRecordIds { get; set; } = new();
}
