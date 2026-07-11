using HR_System.Domain.Enums;

namespace HR_System.Domain.Entities;

public class PayrollRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal Allowance { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetSalary { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public PayrollStatus Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}