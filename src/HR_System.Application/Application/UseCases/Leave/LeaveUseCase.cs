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

        var roles = _scopeService.GetRoles();

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

        string initialApproverRole;
        int initialApproverId;

        if (roles.Any(r => r.Equals("HR", StringComparison.OrdinalIgnoreCase)))
        {
            initialApproverRole = "Manager";
            var managerId = await _employeeRepository.GetManagerEmployeeIdAsync();
            if (managerId.HasValue)
            {
                initialApproverId = managerId.Value;
            }
            else
            {
                throw new InvalidOperationException("No Manager found in the system. HR leave requests require a Manager approver.");
            }
        }
        else if (roles.Any(r => r.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
        {
            initialApproverRole = "HR";
            var hrId = await _employeeRepository.GetHrEmployeeIdAsync();
            if (hrId.HasValue)
            {
                initialApproverId = hrId.Value;
            }
            else
            {
                throw new InvalidOperationException("No HR found in the system");
            }
        }
        else if (roles.Any(r => r.Equals("HeadDepartment", StringComparison.OrdinalIgnoreCase)))
        {
            initialApproverRole = "HeadDivision";
            var headDivId = await _employeeRepository.GetHeadOfDivisionEmployeeIdAsync(employee.DivisionId);
            if (headDivId.HasValue)
            {
                initialApproverId = headDivId.Value;
            }
            else
            {
                var hrId = await _employeeRepository.GetHrEmployeeIdAsync();
                if (hrId.HasValue)
                {
                    initialApproverId = hrId.Value;
                    initialApproverRole = "HR";
                }
                else
                {
                    throw new InvalidOperationException("No approver found in the system");
                }
            }
        }
        else if (roles.Any(r => r.Equals("HeadDivision", StringComparison.OrdinalIgnoreCase)))
        {
            initialApproverRole = "HR";
            var hrId = await _employeeRepository.GetHrEmployeeIdAsync();
            if (hrId.HasValue)
            {
                initialApproverId = hrId.Value;
            }
            else
            {
                throw new InvalidOperationException("No approver found in the system");
            }
        }
        else
        {
            initialApproverRole = "HeadDepartment";
            var headDeptId = await _employeeRepository.GetHeadOfDepartmentEmployeeIdAsync(employee.DepartmentId);
            if (headDeptId.HasValue)
            {
                initialApproverId = headDeptId.Value;
            }
            else
            {
                var headDivId = await _employeeRepository.GetHeadOfDivisionEmployeeIdAsync(employee.DivisionId);
                if (headDivId.HasValue)
                {
                    initialApproverId = headDivId.Value;
                    initialApproverRole = "HeadDivision";
                }
                else
                {
                    var hrId = await _employeeRepository.GetHrEmployeeIdAsync();
                    if (hrId.HasValue)
                    {
                        initialApproverId = hrId.Value;
                        initialApproverRole = "HR";
                    }
                    else
                    {
                        throw new InvalidOperationException("No approver found in the system");
                    }
                }
            }
        }

        var history = new LeaveApprovalHistory
        {
            LeaveRequestId = leaveRequest.Id,
            StepNumber = 1,
            ApproverRole = initialApproverRole,
            ApproverId = initialApproverId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        await _approvalRepository.CreateHistoryAsync(history);

        var approvalItem = new ApprovalItem
        {
            LeaveRequestId = leaveRequest.Id,
            RequesterEmployeeId = employeeId,
            ApproverEmployeeId = initialApproverId,
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

    public async Task<LeaveRequestDto> CancelAsync(int leaveRequestId, int employeeId)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsDtoAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        if (leaveRequest.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own leave requests");
        }

        if (leaveRequest.Status.ToLower() != "pending")
        {
            throw new InvalidOperationException("Only pending requests can be cancelled");
        }

        await _leaveRepository.UpdateStatusAsync(leaveRequestId, LeaveStatus.Cancelled.ToString());

        var pendingItems = await _approvalRepository.GetByLeaveRequestIdAsync(leaveRequestId);
        foreach (var item in pendingItems.Where(i => i.Status == "Pending"))
        {
            await _approvalRepository.UpdateAsync(new ApprovalItem
            {
                LeaveRequestId = leaveRequestId,
                RequesterEmployeeId = leaveRequest.EmployeeId,
                ApproverEmployeeId = item.ApproverEmployeeId,
                Type = "Leave",
                Status = "Cancelled",
                Comment = "Cancelled by requester"
            });
        }

        var employee = await _employeeRepository.GetByIdAsDtoAsync(leaveRequest.EmployeeId);
        return new LeaveRequestDto
        {
            LeaveRequestId = leaveRequest.LeaveRequestId,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = employee?.Name ?? "",
            LeaveType = leaveRequest.LeaveType.ToLower(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Status = "cancelled",
            Reason = leaveRequest.Reason
        };
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