namespace HR_System.Application.DTOs.Payroll;

public class PayrollDetailDto
{
    public int PayrollRecordId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal Allowance { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetSalary { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public string Status { get; set; } = string.Empty;
}
