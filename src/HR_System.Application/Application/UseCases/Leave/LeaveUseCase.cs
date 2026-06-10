using HR_System.Application.DTOs.Leave;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Leave;

public class LeaveUseCase
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IApprovalRepository _approvalRepository;
    private readonly IScopeService _scopeService;

    public LeaveUseCase(
        ILeaveRepository leaveRepository,
        IEmployeeRepository employeeRepository,
        IApprovalRepository approvalRepository,
        IScopeService scopeService)
    {
        _leaveRepository = leaveRepository;
        _employeeRepository = employeeRepository;
        _approvalRepository = approvalRepository;
        _scopeService = scopeService;
    }

    public async Task<LeaveListResponse> GetAllAsync(string? status, int? employeeId, int page, int limit)
    {
        var role = _scopeService.GetRole();
        var divisionId = _scopeService.GetDivisionId();
        var departmentId = _scopeService.GetDepartmentId();
        var userId = _scopeService.GetUserId();

        var bypassScope = role == "Admin" || role == "HR";

        List<LeaveRequestDto> items;
        int total;

        if (bypassScope)
        {
            var (dtoItems, dtoTotal) = await _leaveRepository.GetAllAsDtoAsync(status, employeeId, page, limit);
            items = dtoItems;
            total = dtoTotal;
        }
        else
        {
            var (dbItems, dbTotal) = await _leaveRepository.GetAllAsync(status, employeeId, page, limit, divisionId, departmentId, role, userId);
            items = dbItems.Select(MapToDto).ToList();
            total = dbTotal;
        }

        return new LeaveListResponse
        {
            Requests = items,
            Total = total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<LeaveRequestDto> CreateAsync(int employeeId, CreateLeaveRequest request)
    {
        var employee = await _employeeRepository.GetByIdAsDtoAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        var days = (int)(request.EndDate - request.StartDate).TotalDays + 1;

        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = Enum.Parse<LeaveType>(request.LeaveType, true),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = days,
            Status = LeaveStatus.Pending,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        };

        await _leaveRepository.CreateAsync(leaveRequest);

        var approvalItem = new ApprovalItem
        {
            Id = Guid.NewGuid(),
            Type = ApprovalType.Leave,
            EmployeeId = employeeId,
            Title = $"{request.LeaveType} Leave",
            Detail = $"{request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd} ({days} days)",
            Status = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _approvalRepository.CreateAsync(approvalItem);

        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = employeeId,
            EmployeeName = employee.Name,
            LeaveType = leaveRequest.LeaveType.ToString().ToLower(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Status = leaveRequest.Status.ToString().ToLower(),
            Reason = leaveRequest.Reason
        };
    }

    public async Task<LeaveRequestDto> UpdateStatusAsync(Guid id, UpdateLeaveRequest request)
    {
        var leaveRequestDto = await _leaveRepository.GetByIdAsDtoAsync(id);
        if (leaveRequestDto == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        var leaveRequest = new LeaveRequest
        {
            Id = id,
            EmployeeId = leaveRequestDto.EmployeeId,
            Status = Enum.Parse<LeaveStatus>(request.Status, true)
        };

        await _leaveRepository.UpdateAsync(leaveRequest);

        leaveRequestDto.Status = request.Status.ToLower();
        return leaveRequestDto;
    }

    private static LeaveRequestDto MapToDto(LeaveRequest leaveRequest)
    {
        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = "",
            LeaveType = leaveRequest.LeaveType.ToString().ToLower(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Status = leaveRequest.Status.ToString().ToLower(),
            Reason = leaveRequest.Reason
        };
    }
}
