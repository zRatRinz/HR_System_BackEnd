using HR_System.Application.DTOs.Approval;
using HR_System.Application.Interfaces;

namespace HR_System.Application.UseCases.Approval;

public class ApprovalUseCase
{
    private readonly IApprovalRepository _approvalRepository;
    private readonly IScopeService _scopeService;

    public ApprovalUseCase(
        IApprovalRepository approvalRepository,
        IScopeService scopeService)
    {
        _approvalRepository = approvalRepository;
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

    public async Task<ApprovalListResponse> GetByLeaveRequestIdAsync(int leaveRequestId)
    {
        var items = await _approvalRepository.GetByLeaveRequestIdAsync(leaveRequestId);
        return new ApprovalListResponse { Approvals = items };
    }
}