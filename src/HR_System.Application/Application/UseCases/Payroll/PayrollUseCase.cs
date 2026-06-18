using HR_System.Application.DTOs.Payroll;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Payroll;

public class PayrollUseCase
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IScopeService _scopeService;

    public PayrollUseCase(
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository,
        IScopeService scopeService)
    {
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
        _scopeService = scopeService;
    }

    public async Task<PayrollListResponse> GetAllAsync(string? period, int? employeeId, int page, int limit)
    {
        var role = _scopeService.GetRole();
        var divisionId = _scopeService.GetDivisionId();
        var departmentId = _scopeService.GetDepartmentId();
        var userId = _scopeService.GetUserId();

        var bypassScope = role == "Admin" || role == "HR";

        List<PayrollDto> items;
        int total;

        if (bypassScope)
        {
            items = await _payrollRepository.GetAllAsDtoAsync(period, employeeId, page, limit);
            total = items.Count;
        }
        else
        {
            var (dbItems, dbTotal) = await _payrollRepository.GetAllAsync(period, employeeId, page, limit, divisionId, departmentId, role, userId);
            items = dbItems;
            total = dbTotal;
        }

        return new PayrollListResponse
        {
            Data = items,
            Total = total
        };
    }

    public async Task<ProcessPayrollResponse> ProcessAsync(string period)
    {
        var existingRecords = await _payrollRepository.GetByPeriodAsDtoAsync(period);
        if (existingRecords.Any())
        {
            throw new InvalidOperationException($"Payroll for period {period} already processed");
        }

        var role = _scopeService.GetRole();
        var divisionId = _scopeService.GetDivisionId();
        var departmentId = _scopeService.GetDepartmentId();
        var userId = _scopeService.GetUserId();

        var employeeIds = await _employeeRepository.GetAllIdsAsync(divisionId, departmentId, role, userId);
        var processed = 0;

        foreach (var empId in employeeIds)
        {
            var payroll = new PayrollRecord
            {
                EmployeeId = empId,
                Period = period,
                BasicSalary = 50000,
                Allowance = 0,
                Deduction = 5000,
                NetSalary = 45000,
                Status = PayrollStatus.Processed,
                CreatedAt = DateTime.UtcNow
            };

            await _payrollRepository.CreateAsync(payroll);
            processed++;
        }

        return new ProcessPayrollResponse
        {
            Processed = processed,
            Total = employeeIds.Count
        };
    }
}
