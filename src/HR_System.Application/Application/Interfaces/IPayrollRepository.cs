using HR_System.Application.DTOs.Payroll;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IPayrollRepository
{
    Task<(List<PayrollDto> Items, int Total)> GetAllAsync(string? period, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId);
    Task<PayrollRecord> CreateAsync(PayrollRecord record);
    Task<PayrollRecord> UpdateAsync(PayrollRecord record);
    Task<List<PayrollDto>> GetByPeriodAsDtoAsync(string period);
    Task<decimal> GetTotalPayrollForPeriodAsync(string period);

    Task<List<PayrollDto>> GetAllAsDtoAsync(string? period, int? employeeId, int page, int limit);
}
