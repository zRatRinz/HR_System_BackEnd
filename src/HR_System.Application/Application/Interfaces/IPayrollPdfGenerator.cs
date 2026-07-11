using HR_System.Application.DTOs.Payroll;

namespace HR_System.Application.Interfaces;

public interface IPayrollPdfGenerator
{
    byte[] Generate(PayrollDto payroll, string? companyName = null);
}
