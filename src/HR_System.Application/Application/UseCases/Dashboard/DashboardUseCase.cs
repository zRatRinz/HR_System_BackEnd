using HR_System.Application.DTOs.Dashboard;
using HR_System.Application.Interfaces;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Dashboard;

public class DashboardUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRepository _leaveRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IApprovalRepository _approvalRepository;
    private readonly IPayrollRepository _payrollRepository;
    private readonly IScopeService _scopeService;

    public DashboardUseCase(
        IEmployeeRepository employeeRepository,
        ILeaveRepository leaveRepository,
        IAttendanceRepository attendanceRepository,
        IApprovalRepository approvalRepository,
        IPayrollRepository payrollRepository,
        IScopeService scopeService)
    {
        _employeeRepository = employeeRepository;
        _leaveRepository = leaveRepository;
        _attendanceRepository = attendanceRepository;
        _approvalRepository = approvalRepository;
        _payrollRepository = payrollRepository;
        _scopeService = scopeService;
    }

    public async Task<DashboardStatsResponse> GetStatsAsync()
    {
        var totalEmployees = await _employeeRepository.GetTotalCountAsync();
        var onLeaveToday = await _leaveRepository.GetOnLeaveCountTodayAsync();
        var employeeId = _scopeService.GetEmployeeId();
        var pendingApprovals = employeeId.HasValue
            ? await _approvalRepository.GetPendingCountByApproverEmployeeIdAsync(employeeId.Value)
            : 0;
        var attendanceRate = await _attendanceRepository.GetAttendanceRateAsync();

        var currentPeriod = DateTime.UtcNow.ToString("yyyy-MM");
        var monthlyPayroll = await _payrollRepository.GetTotalPayrollForPeriodAsync(currentPeriod);

        return new DashboardStatsResponse
        {
            TotalEmployees = totalEmployees,
            OnLeaveToday = onLeaveToday,
            PendingApprovals = pendingApprovals,
            AttendanceRate = $"{attendanceRate:F1}%",
            MonthlyPayroll = $"${monthlyPayroll:N0}"
        };
    }

    /// <summary>
    /// Retrieves employee growth data for the last 6 months.
    /// Returns a list of ChartData containing month labels and employee counts.
    /// Note: Currently returns the same total count for each month as the repository
    /// does not yet track historical employee counts by month.
    /// </summary>
    public async Task<EmployeeGrowthResponse> GetEmployeeGrowthAsync()
    {
        var months = new List<ChartData>();
        var currentDate = DateTime.UtcNow;

        for (int i = 5; i >= 0; i--)
        {
            var date = currentDate.AddMonths(-i);
            var label = date.ToString("MMM yyyy");
            months.Add(new ChartData
            {
                Label = label,
                Value = await _employeeRepository.GetTotalCountAsync()
            });
        }

        return new EmployeeGrowthResponse { Data = months };
    }

    public async Task<DepartmentResponse> GetDepartmentsAsync()
    {
        var distribution = await _employeeRepository.GetDepartmentDistributionAsync();

        var departments = distribution.Select(kvp => new DepartmentData
        {
            Name = kvp.Key,
            EmployeeCount = kvp.Value
        }).ToList();

        return new DepartmentResponse { Departments = departments };
    }
}
