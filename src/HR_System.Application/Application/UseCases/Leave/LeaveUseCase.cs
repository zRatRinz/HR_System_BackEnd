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

    public async Task<LeaveListResponse> GetMyLeavesAsync(string? status, int page, int limit)
    {
        var employeeId = _scopeService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            throw new UnauthorizedAccessException("Employee not found in token");
        }

        var (items, total) = await _leaveRepository.GetAllAsDtoAsync(status, employeeId.Value, page, limit);
        return new LeaveListResponse
        {
            Requests = items,
            Total = total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<LeaveListResponse> GetTeamLeavesAsync(string? status, int? employeeId, int? divisionId, int? departmentId, int page, int limit)
    {
        var roles = _scopeService.GetRoles();
        var bypassScope = roles.Any(r => r == "Admin" || r == "HR" || r == "Manager");

        int? scopeDivisionId;
        int? scopeDepartmentId;

        if (!bypassScope)
        {
            var tokenDivisionId = _scopeService.GetDivisionId();
            var tokenDepartmentId = _scopeService.GetDepartmentId();

            if ((divisionId.HasValue && divisionId != tokenDivisionId) ||
                (departmentId.HasValue && departmentId != tokenDepartmentId))
            {
                return new LeaveListResponse
                {
                    Requests = new List<LeaveRequestDto>(),
                    Total = 0,
                    Page = page,
                    Limit = limit
                };
            }

            scopeDivisionId = tokenDivisionId;
            scopeDepartmentId = tokenDepartmentId;
        }
        else
        {
            scopeDivisionId = divisionId;
            scopeDepartmentId = departmentId;
        }

        var (items, total) = await _leaveRepository.GetAllAsDtoAsync(
            status, employeeId, page, limit,
            scopeDivisionId, scopeDepartmentId, bypassScope);

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

        int approverEmployeeId;
        var headDeptId = await _employeeRepository.GetHeadOfDepartmentEmployeeIdAsync(employee.DepartmentId);
        if (headDeptId.HasValue)
        {
            approverEmployeeId = headDeptId.Value;
        }
        else
        {
            var headDivId = await _employeeRepository.GetHeadOfDivisionEmployeeIdAsync(employee.DivisionId);
            if (headDivId.HasValue)
            {
                approverEmployeeId = headDivId.Value;
            }
            else
            {
                var hrId = await _employeeRepository.GetHrEmployeeIdAsync();
                if (hrId.HasValue)
                {
                    approverEmployeeId = hrId.Value;
                }
                else
                {
                    throw new InvalidOperationException("No approver found in the system");
                }
            }
        }

        var days = (int)(request.EndDate - request.StartDate).TotalDays + 1;

        var leaveRequest = new LeaveRequest
        {
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
            LeaveRequestId = leaveRequest.Id,
            RequesterEmployeeId = employeeId,
            ApproverEmployeeId = approverEmployeeId,
            Type = "Leave",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _approvalRepository.CreateAsync(approvalItem);

        return new LeaveRequestDto
        {
            LeaveRequestId = leaveRequest.Id,
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

    public async Task<LeaveRequestDto> UpdateStatusAsync(int id, UpdateLeaveRequest request)
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
            LeaveRequestId = leaveRequest.Id,
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