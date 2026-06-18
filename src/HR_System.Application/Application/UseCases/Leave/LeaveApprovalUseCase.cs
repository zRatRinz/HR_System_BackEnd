using HR_System.Application.DTOs.Leave;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Leave;

public class LeaveApprovalUseCase
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly ILeaveApprovalHistoryRepository _approvalHistoryRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IApprovalRepository _approvalRepository;

    public LeaveApprovalUseCase(
        ILeaveRepository leaveRepository,
        ILeaveApprovalHistoryRepository approvalHistoryRepository,
        IEmployeeRepository employeeRepository,
        IApprovalRepository approvalRepository)
    {
        _leaveRepository = leaveRepository;
        _approvalHistoryRepository = approvalHistoryRepository;
        _employeeRepository = employeeRepository;
        _approvalRepository = approvalRepository;
    }

    public async Task<LeaveRequestDto> ApproveAsync(int leaveRequestId, string approverRole, int approverId, string? comment)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be approved");
        }

        var latestStep = await _approvalHistoryRepository.GetLatestStepAsync(leaveRequestId);

        int nextStepNumber;
        string nextApproverRole;

        if (latestStep == null)
        {
            nextStepNumber = 1;
            nextApproverRole = "HeadDepartment";
        }
        else if (latestStep.Status == "Approved" && latestStep.StepNumber == 1)
        {
            nextStepNumber = 2;
            nextApproverRole = "HeadDivision";
        }
        else if (latestStep.Status == "Approved" && latestStep.StepNumber == 2)
        {
            nextStepNumber = 3;
            nextApproverRole = "HR";
        }
        else
        {
            throw new InvalidOperationException("Cannot approve at this step");
        }

        if (!string.Equals(approverRole, nextApproverRole, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"This step requires approval from {nextApproverRole}, not {approverRole}");
        }

        var history = new LeaveApprovalHistory
        {
            LeaveRequestId = leaveRequestId,
            StepNumber = nextStepNumber,
            ApproverRole = nextApproverRole,
            ApproverId = approverId,
            Status = "Approved",
            Comment = comment,
            ActionAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _approvalHistoryRepository.CreateAsync(history);

        await _approvalRepository.UpdateAsync(new ApprovalItem
        {
            LeaveRequestId = leaveRequestId,
            RequesterEmployeeId = leaveRequest.EmployeeId,
            ApproverEmployeeId = approverId,
            Type = "Leave",
            Status = "Approved",
            Comment = comment
        });

        if (nextStepNumber == 3)
        {
            await _leaveRepository.UpdateStatusAsync(leaveRequestId, LeaveStatus.Approved.ToString());
        }
        else
        {
            int? nextApproverEmployeeId = null;
            if (nextStepNumber == 2)
            {
                var employee = await _employeeRepository.GetByIdAsDtoAsync(leaveRequest.EmployeeId);
                if (employee != null)
                {
                    nextApproverEmployeeId = await _employeeRepository.GetHeadOfDivisionEmployeeIdAsync(employee.DivisionId);
                }
            }
            else if (nextStepNumber == 3)
            {
                nextApproverEmployeeId = await _employeeRepository.GetHrEmployeeIdAsync();
            }

            if (nextApproverEmployeeId.HasValue)
            {
                await _approvalRepository.CreateAsync(new ApprovalItem
                {
                    LeaveRequestId = leaveRequestId,
                    RequesterEmployeeId = leaveRequest.EmployeeId,
                    ApproverEmployeeId = nextApproverEmployeeId.Value,
                    Type = "Leave",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var emp = await _employeeRepository.GetByIdAsDtoAsync(leaveRequest.EmployeeId);
        return new LeaveRequestDto
        {
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = emp?.Name ?? "",
            LeaveType = leaveRequest.LeaveType.ToString().ToLower(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Status = nextStepNumber == 3 ? "approved" : "pending",
            Reason = leaveRequest.Reason
        };
    }

    public async Task<LeaveRequestDto> RejectAsync(int leaveRequestId, string approverRole, int approverId, string? comment)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be rejected");
        }

        var latestStep = await _approvalHistoryRepository.GetLatestStepAsync(leaveRequestId);
        int currentStep = latestStep?.StepNumber ?? 0;

        var history = new LeaveApprovalHistory
        {
            LeaveRequestId = leaveRequestId,
            StepNumber = currentStep == 0 ? 1 : currentStep,
            ApproverRole = approverRole,
            ApproverId = approverId,
            Status = "Rejected",
            Comment = comment,
            ActionAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _approvalHistoryRepository.CreateAsync(history);

        await _approvalRepository.UpdateAsync(new ApprovalItem
        {
            LeaveRequestId = leaveRequestId,
            RequesterEmployeeId = leaveRequest.EmployeeId,
            ApproverEmployeeId = approverId,
            Type = "Leave",
            Status = "Rejected",
            Comment = comment
        });

        await _leaveRepository.UpdateStatusAsync(leaveRequestId, LeaveStatus.Rejected.ToString());

        var employee = await _employeeRepository.GetByIdAsDtoAsync(leaveRequest.EmployeeId);
        return new LeaveRequestDto
        {
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = employee?.Name ?? "",
            LeaveType = leaveRequest.LeaveType.ToString().ToLower(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Status = "rejected",
            Reason = leaveRequest.Reason
        };
    }

    public async Task<LeaveRequestDto> CancelAsync(int leaveRequestId, int employeeId)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        if (leaveRequest.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own leave requests");
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
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
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = employee?.Name ?? "",
            LeaveType = leaveRequest.LeaveType.ToString().ToLower(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Days = leaveRequest.Days,
            Status = "cancelled",
            Reason = leaveRequest.Reason
        };
    }

    public async Task<LeaveTimelineDto> GetTimelineAsync(int leaveRequestId)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found");
        }

        var history = await _approvalHistoryRepository.GetByLeaveRequestIdAsync(leaveRequestId);

        var steps = history.Select(h => new LeaveApprovalStepDto
        {
            StepNumber = h.StepNumber,
            ApproverRole = h.ApproverRole,
            Status = h.Status,
            Comment = h.Comment,
            ActionAt = h.ActionAt,
            CreatedAt = h.CreatedAt
        }).ToList();

        return new LeaveTimelineDto
        {
            LeaveRequestId = leaveRequestId,
            Steps = steps
        };
    }
}