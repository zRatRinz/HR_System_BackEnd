using HR_System.Application.DTOs.Payroll;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace HR_System.Application.UseCases.Payroll;

public class PayrollUseCase
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRepository _leaveRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;

    public PayrollUseCase(
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRepository leaveRepository,
        IHolidayRepository holidayRepository,
        ICurrentUserService currentUserService,
        IConfiguration configuration)
    {
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
        _leaveRepository = leaveRepository;
        _holidayRepository = holidayRepository;
        _currentUserService = currentUserService;
        _configuration = configuration;
    }

    public async Task<PayrollListResponse> GetAllAsync(int? month, int? year, int? employeeId, int page, int limit)
    {
        var result = await _payrollRepository.GetAllAsDtoAsync(month, year, employeeId, page, limit);

        return new PayrollListResponse
        {
            Payrolls = result.Items,
            Total = result.Total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<PayrollDto?> GetMyPayrollAsync(int month, int year)
    {
        var employeeId = _currentUserService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            return null;
        }

        return await _payrollRepository.GetByMonthYearEmployeeAsync(month, year, employeeId.Value);
    }

    public async Task<PayrollDetailDto?> GetDetailAsync(int month, int year, int employeeId)
    {
        var role = _currentUserService.GetRole();
        var currentEmployeeId = _currentUserService.GetEmployeeId();

        if (role == "Employee" && currentEmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("Cannot view other employee's payroll");
        }

        if (role != "Admin" && role != "HR" && role != "Employee")
        {
            throw new UnauthorizedAccessException("Not authorized to view payroll");
        }

        return await _payrollRepository.GetDetailAsync(month, year, employeeId);
    }

    public async Task<ProcessPayrollResponse> ProcessAsync(int month, int year)
    {
        var existingRecords = await _payrollRepository.GetByMonthYearAsDtoAsync(month, year);
        if (existingRecords.Any())
        {
            throw new InvalidOperationException($"Payroll for {year}-{month:D2} already processed");
        }

        var role = _currentUserService.GetRole();
        var divisionId = _currentUserService.GetDivisionId();
        var departmentId = _currentUserService.GetDepartmentId();
        var userId = _currentUserService.GetUserId();
        var roles = _currentUserService.GetRoles();

        var employeeIds = await _employeeRepository.GetAllIdsAsync(divisionId, departmentId, role, userId, roles);
        var processed = 0;

        var leavePolicy = _configuration.GetSection("LeavePolicy");
        var annualQuota = leavePolicy.GetSection("Annual").GetValue<int>("Total");
        var sickQuota = leavePolicy.GetSection("Sick").GetValue<int>("Total");

        var startOfYear = new DateTime(year, 1, 1);
        var endOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        var holidays = await _holidayRepository.GetByYearAsync(year);
        var holidaysInMonth = holidays.Count(h => h.HolidayDate.Month == month && h.HolidayDate.Year == year);

        var workingDaysInMonth = CalculateWorkingDays(year, month, holidaysInMonth);

        var lockedLeaveIds = new List<int>();

        foreach (var empId in employeeIds)
        {
            var employee = await _employeeRepository.GetByIdAsDtoAsync(empId);
            if (employee == null) continue;

            var annualUsedYtd = await _leaveRepository.GetApprovedLeaveTotalDaysAsync(empId, startOfYear, endOfMonth, LeaveType.Annual.ToString());
            var sickUsedYtd = await _leaveRepository.GetApprovedLeaveTotalDaysAsync(empId, startOfYear, endOfMonth, LeaveType.Sick.ToString());

            var unpaidAnnualDays = Math.Max(0, annualUsedYtd - annualQuota);
            var unpaidSickDays = Math.Max(0, sickUsedYtd - sickQuota);
            var totalUnpaidDays = unpaidAnnualDays + unpaidSickDays;

            var dailyRate = employee.Salary / workingDaysInMonth;
            var deductionAmount = dailyRate * totalUnpaidDays;
            var netSalary = employee.Salary - deductionAmount;

            var payroll = new PayrollRecord
            {
                EmployeeId = empId,
                Month = month,
                Year = year,
                Period = $"{year}-{month:D2}",
                BasicSalary = employee.Salary,
                Allowance = 0,
                Deduction = deductionAmount,
                NetSalary = netSalary,
                UnpaidLeaveDays = totalUnpaidDays,
                Status = PayrollStatus.Draft,
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _payrollRepository.CreateAsync(payroll);
            processed++;
        }

        var approvedLeavesInMonth = await _leaveRepository.GetApprovedLeavesInMonthAsync(month, year);
        lockedLeaveIds = approvedLeavesInMonth.Select(l => l.LeaveRequestId).ToList();

        if (lockedLeaveIds.Any())
        {
            await _leaveRepository.LockLeavesAsync(month, year, lockedLeaveIds);
        }

        return new ProcessPayrollResponse
        {
            Processed = processed,
            Total = employeeIds.Count
        };
    }

    public async Task UnlockAsync(int month, int year)
    {
        var existingRecords = await _payrollRepository.GetByMonthYearAsDtoAsync(month, year);
        if (!existingRecords.Any())
        {
            throw new InvalidOperationException($"No processed payroll found for {year}-{month:D2}");
        }

        await _payrollRepository.DeleteByMonthYearAsync(month, year);
        await _leaveRepository.UnlockLeavesAsync(month, year);
    }

    public async Task ApproveAsync(int month, int year)
    {
        var role = _currentUserService.GetRole();
        if (role != "HR" && role != "Admin")
        {
            throw new UnauthorizedAccessException("Only HR or Admin can approve payroll");
        }

        var existingRecords = await _payrollRepository.GetByMonthYearAsDtoAsync(month, year);
        if (!existingRecords.Any())
        {
            throw new InvalidOperationException($"No payroll found for {year}-{month:D2}");
        }

        await _payrollRepository.ApproveAsync(month, year);
    }

    public async Task<int> ApproveByIdsAsync(int month, int year, List<int> payrollRecordIds)
    {
        var roles = _currentUserService.GetRoles();
        if (!roles.Contains("HR"))
        {
            throw new UnauthorizedAccessException("Only HR can approve payroll");
        }

        if (!payrollRecordIds.Any())
        {
            throw new InvalidOperationException("payrollRecordIds is required");
        }

        foreach (var id in payrollRecordIds)
        {
            var record = await _payrollRepository.GetByIdAsDtoAsync(id);
            if (record == null || record.Month != month || record.Year != year)
            {
                throw new InvalidOperationException("Some payroll records do not match the specified month/year");
            }
        }

        return await _payrollRepository.ApproveByIdsAsync(payrollRecordIds);
    }

    private int CalculateWorkingDays(int year, int month, int holidaysInMonth)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var saturdays = 0;
        var sundays = 0;

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            if (date.DayOfWeek == DayOfWeek.Saturday) saturdays++;
            if (date.DayOfWeek == DayOfWeek.Sunday) sundays++;
        }

        return daysInMonth - saturdays - sundays - holidaysInMonth;
    }
}
