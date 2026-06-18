using HR_System.Application.DTOs.Leave;
using HR_System.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HR_System.Application.UseCases.Leave;

public class LeaveBalanceUseCase
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly IConfiguration _configuration;

    public LeaveBalanceUseCase(ILeaveRepository leaveRepository, IConfiguration configuration)
    {
        _leaveRepository = leaveRepository;
        _configuration = configuration;
    }

    public async Task<LeaveBalanceDto> GetBalanceAsync(int employeeId)
    {
        var usedDays = await _leaveRepository.GetUsedDaysByTypeAsync(employeeId);
        var pendingRequestsCount = await _leaveRepository.GetPendingRequestsCountAsync(employeeId);
        var leaveTakenYtd = await _leaveRepository.GetLeaveTakenYtdAsync(employeeId);

        var leavePolicy = _configuration.GetSection("LeavePolicy");

        var annualTotal = leavePolicy.GetSection("Annual").GetValue<int>("Total");
        var sickTotal = leavePolicy.GetSection("Sick").GetValue<int>("Total");

        return new LeaveBalanceDto
        {
            Annual = new LeaveBalanceItemDto
            {
                Total = annualTotal,
                Used = usedDays.GetValueOrDefault("Annual", 0),
                Balance = annualTotal - usedDays.GetValueOrDefault("Annual", 0)
            },
            Sick = new LeaveBalanceItemDto
            {
                Total = sickTotal,
                Used = usedDays.GetValueOrDefault("Sick", 0),
                Balance = sickTotal - usedDays.GetValueOrDefault("Sick", 0)
            },
            PendingRequests = pendingRequestsCount,
            LeaveTakenYtd = leaveTakenYtd
        };
    }
}