using HR_System.Application.DTOs.Approval;
using HR_System.Application.DTOs.Leave;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Approval;

public class ApprovalUseCase
{
    private readonly IApprovalRepository _approvalRepository;
    private readonly ILeaveRepository _leaveRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IScopeService _scopeService;

    public ApprovalUseCase(
        IApprovalRepository approvalRepository,
        ILeaveRepository leaveRepository,
        IEmployeeRepository employeeRepository,
        IScopeService scopeService)
    {
        _approvalRepository = approvalRepository;
        _leaveRepository = leaveRepository;
        _employeeRepository = employeeRepository;
        _scopeService = scopeService;
    }

    public async Task<ApprovalListResponse> GetPendingForCurrentApproverAsync(int page = 1, int limit = 10)
    {
        var employeeId = _scopeService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            return new ApprovalListResponse { Approvals = new List<ApprovalItemDto>() };
        }

        var (items, total) = await _approvalRepository.GetPendingByApproverEmployeeIdAsync(employeeId.Value, page, limit);

        return new ApprovalListResponse
        {
            Approvals = items,
            Total = total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<LeaveRequestApprovalDto> GetByLeaveRequestIdAsync(int leaveRequestId)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsDtoAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        var requester = await _employeeRepository.GetByIdAsDtoAsync(leaveRequest.EmployeeId);
        var history = await _approvalRepository.GetHistoryByLeaveRequestIdAsync(leaveRequestId);

        var currentEmployeeId = _scopeService.GetEmployeeId();
        if (!currentEmployeeId.HasValue)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        bool isRequester = leaveRequest.EmployeeId == currentEmployeeId.Value;
        bool isApprover = history.Any(h => h.ApproverId == currentEmployeeId.Value);

        if (!isRequester && !isApprover)
        {
            throw new UnauthorizedAccessException("You don't have permission to view this approval");
        }

        var timeline = new List<LeaveApprovalStepDto>();
        foreach (var h in history)
        {
            var approverName = "";
            if (h.ApproverId.HasValue)
            {
                var approver = await _employeeRepository.GetByIdAsDtoAsync(h.ApproverId.Value);
                approverName = approver?.Name ?? "";
            }
            timeline.Add(new LeaveApprovalStepDto
            {
                StepNumber = h.StepNumber,
                ApproverName = approverName,
                Status = h.Status,
                Comment = h.Comment,
                ActionAt = h.ActionAt,
                CreatedAt = h.CreatedAt
            });
        }

        return new LeaveRequestApprovalDto
        {
            LeaveRequestId = leaveRequestId,
            RequesterEmployeeId = leaveRequest.EmployeeId,
            RequesterName = requester?.Name ?? "",
            LeaveType = leaveRequest.LeaveType.ToString().ToLower(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Status = leaveRequest.Status.ToLower(),
            Reason = leaveRequest.Reason,
            Timeline = timeline
        };
    }

    public async Task<ApprovalResultDto> ApproveAsync(int approvalItemId, int approverId, string? comment)
    {
        var currentItem = await _approvalRepository.GetApprovalItemByIdAsync(approvalItemId);
        if (currentItem == null || currentItem.Status != "Pending")
        {
            throw new KeyNotFoundException("Approval item not found or already processed");
        }

        var leaveRequest = await _leaveRepository.GetByIdAsDtoAsync(currentItem.LeaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        if (leaveRequest.Status.ToLower() != "pending")
        {
            throw new InvalidOperationException("Only pending requests can be approved");
        }

        var pendingStep = await _approvalRepository.GetCurrentStepAsync(currentItem.LeaveRequestId);
        if (pendingStep == null)
        {
            throw new InvalidOperationException("No pending step found");
        }

        var roles = _scopeService.GetRoles();
        string approverRole;
        if (pendingStep.ApproverRole == "Manager" && roles.Any(r => r.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
        {
            approverRole = "Manager";
        }
        else if (pendingStep.ApproverRole == "HR" && roles.Any(r => r.Equals("HR", StringComparison.OrdinalIgnoreCase)))
        {
            approverRole = "HR";
        }
        else if (pendingStep.ApproverRole == "HeadDivision" && roles.Any(r => r.Equals("HeadDivision", StringComparison.OrdinalIgnoreCase)))
        {
            approverRole = "HeadDivision";
        }
        else if (pendingStep.ApproverRole == "HeadDepartment" && roles.Any(r => r.Equals("HeadDepartment", StringComparison.OrdinalIgnoreCase)))
        {
            approverRole = "HeadDepartment";
        }
        else
        {
            throw new InvalidOperationException($"You do not have the required role: {pendingStep.ApproverRole}");
        }

        pendingStep.Status = "Approved";
        pendingStep.ApproverId = approverId;
        pendingStep.Comment = comment;
        pendingStep.ActionAt = DateTime.UtcNow;
        await _approvalRepository.UpdateHistoryAsync(pendingStep);

        currentItem.Status = "Approved";
        currentItem.Comment = comment;
        await _approvalRepository.UpdateAsync(currentItem);

        string? nextApproverRole = pendingStep.ApproverRole switch
        {
            "Manager" => null,
            "HeadDepartment" => "HeadDivision",
            "HeadDivision" => "HR",
            "HR" => null,
            _ => throw new InvalidOperationException("Invalid approver role")
        };

        if (nextApproverRole != null)
        {
            int? nextApproverId = null;

            if (nextApproverRole == "HeadDivision")
            {
                var emp = await _employeeRepository.GetByIdAsDtoAsync(currentItem.RequesterEmployeeId);
                if (emp != null)
                {
                    nextApproverId = await _employeeRepository.GetHeadOfDivisionEmployeeIdAsync(emp.DivisionId);
                }
            }
            else if (nextApproverRole == "HR")
            {
                nextApproverId = await _employeeRepository.GetHrEmployeeIdAsync();
            }

            if (!nextApproverId.HasValue)
            {
                throw new InvalidOperationException($"No approver found for {nextApproverRole}");
            }

            var nextStep = new LeaveApprovalHistory
            {
                LeaveRequestId = currentItem.LeaveRequestId,
                StepNumber = pendingStep.StepNumber + 1,
                ApproverRole = nextApproverRole,
                ApproverId = nextApproverId.Value,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _approvalRepository.CreateHistoryAsync(nextStep);

            await _approvalRepository.CreateAsync(new ApprovalItem
            {
                LeaveRequestId = currentItem.LeaveRequestId,
                RequesterEmployeeId = currentItem.RequesterEmployeeId,
                ApproverEmployeeId = nextApproverId.Value,
                Type = "Leave",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            return new ApprovalResultDto
            {
                Success = true,
                Message = $"Approved. Forwarded to {nextApproverRole} for next step.",
                LeaveRequestId = currentItem.LeaveRequestId,
                NewStatus = "Pending"
            };
        }

        await _leaveRepository.UpdateStatusAsync(currentItem.LeaveRequestId, LeaveStatus.Approved.ToString());

        return new ApprovalResultDto
        {
            Success = true,
            Message = "Leave request fully approved",
            LeaveRequestId = currentItem.LeaveRequestId,
            NewStatus = "Approved"
        };
    }

    public async Task<ApprovalResultDto> RejectAsync(int approvalItemId, int approverId, string? comment)
    {
        var currentItem = await _approvalRepository.GetApprovalItemByIdAsync(approvalItemId);
        if (currentItem == null || currentItem.Status != "Pending")
        {
            throw new KeyNotFoundException("Approval item not found or already processed");
        }

        var leaveRequest = await _leaveRepository.GetByIdAsDtoAsync(currentItem.LeaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        if (leaveRequest.Status.ToLower() != "pending")
        {
            throw new InvalidOperationException("Only pending requests can be rejected");
        }

        var pendingStep = await _approvalRepository.GetCurrentStepAsync(currentItem.LeaveRequestId);
        if (pendingStep == null)
        {
            throw new InvalidOperationException("No pending step found");
        }

        pendingStep.Status = "Rejected";
        pendingStep.ApproverId = approverId;
        pendingStep.Comment = comment;
        pendingStep.ActionAt = DateTime.UtcNow;
        await _approvalRepository.UpdateHistoryAsync(pendingStep);

        currentItem.Status = "Rejected";
        currentItem.Comment = comment;
        await _approvalRepository.UpdateAsync(currentItem);

        await _leaveRepository.UpdateStatusAsync(currentItem.LeaveRequestId, LeaveStatus.Rejected.ToString());

        return new ApprovalResultDto
        {
            Success = true,
            Message = "Leave request rejected",
            LeaveRequestId = currentItem.LeaveRequestId,
            NewStatus = "Rejected"
        };
    }

    public async Task<LeaveTimelineDto> GetTimelineAsync(int leaveRequestId)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsDtoAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        var history = await _approvalRepository.GetHistoryByLeaveRequestIdAsync(leaveRequestId);

        var steps = new List<LeaveApprovalStepDto>();
        foreach (var h in history)
        {
            var approverName = "";
            if (h.ApproverId.HasValue)
            {
                var approver = await _employeeRepository.GetByIdAsDtoAsync(h.ApproverId.Value);
                approverName = approver?.Name ?? "";
            }
            steps.Add(new LeaveApprovalStepDto
            {
                StepNumber = h.StepNumber,
                ApproverName = approverName,
                Status = h.Status,
                Comment = h.Comment,
                ActionAt = h.ActionAt,
                CreatedAt = h.CreatedAt
            });
        }

        return new LeaveTimelineDto
        {
            LeaveRequestId = leaveRequestId,
            Steps = steps
        };
    }

    public async Task<ApprovalStatisticsDto> GetStatisticsAsync()
    {
        var employeeId = _scopeService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            return new ApprovalStatisticsDto();
        }

        var pending = await _approvalRepository.GetPendingCountForApproverAsync(employeeId.Value);
        var inProgress = await _approvalRepository.GetInProgressCountForApproverAsync(employeeId.Value);
        var thisMonthApproved = await _approvalRepository.GetThisMonthApprovedCountForApproverAsync(employeeId.Value);
        var thisMonthRejected = await _approvalRepository.GetThisMonthRejectedCountForApproverAsync(employeeId.Value);

        return new ApprovalStatisticsDto
        {
            Pending = pending,
            InProgress = inProgress,
            ThisMonthApproved = thisMonthApproved,
            ThisMonthRejected = thisMonthRejected
        };
    }
}