using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Approval;
using HR_System.Application.UseCases.Approval;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/approvals")]
public class ApprovalsController : ControllerBase
{
    private readonly ApprovalUseCase _approvalUseCase;

    public ApprovalsController(ApprovalUseCase approvalUseCase)
    {
        _approvalUseCase = approvalUseCase;
    }

    [HttpGet]
    [RequirePermission("leaves.approve")]
    [ProducesResponseType(typeof(ApiResponse<ApprovalListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var response = await _approvalUseCase.GetPendingForCurrentApproverAsync(page, limit);
        return Ok(ApiResponse<ApprovalListResponse>.Success(response));
    }

    [HttpGet("{leaveRequestId:int}")]
    [RequirePermission("leaves.approve")]
    [ProducesResponseType(typeof(ApiResponse<ApprovalListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByLeaveRequest(int leaveRequestId)
    {
        var response = await _approvalUseCase.GetByLeaveRequestIdAsync(leaveRequestId);
        return Ok(ApiResponse<ApprovalListResponse>.Success(response));
    }
}