using HR_System.Application.DTOs.Payroll;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IPayrollRepository
{
    Task<(List<PayrollDto> Items, int Total)> GetAllAsync(int? month, int? year, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId);
    Task<PayrollRecord> CreateAsync(PayrollRecord record);
    Task<PayrollRecord> UpdateAsync(PayrollRecord record);
    Task<List<PayrollDto>> GetByMonthYearAsDtoAsync(int month, int year);
    Task<decimal> GetTotalPayrollForMonthYearAsync(int month, int year);
    Task<(List<PayrollDto> Items, int Total)> GetAllAsDtoAsync(int? month, int? year, int? employeeId, int page, int limit);
    Task<PayrollDto?> GetByIdAsDtoAsync(int id);
    Task<PayrollDto?> GetByMonthYearEmployeeAsync(int month, int year, int employeeId);
    Task DeleteByMonthYearAsync(int month, int year);
    Task ApproveAsync(int month, int year);
    Task<int> ApproveByIdsAsync(List<int> payrollRecordIds);
    Task<PayrollDetailDto?> GetDetailAsync(int month, int year, int employeeId);
}
