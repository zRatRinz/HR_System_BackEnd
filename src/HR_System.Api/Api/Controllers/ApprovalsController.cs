using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Approval;
using HR_System.Application.DTOs.Leave;
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
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestApprovalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLeaveRequest(int leaveRequestId)
    {
        try
        {
            var response = await _approvalUseCase.GetByLeaveRequestIdAsync(leaveRequestId);
            return Ok(ApiResponse<LeaveRequestApprovalDto>.Success(response));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/approve")]
    [RequirePermission("leaves.approve")]
    [ProducesResponseType(typeof(ApiResponse<ApprovalResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Approve(int id, [FromBody] ApprovalActionRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token"));
        }

        try
        {
            var result = await _approvalUseCase.ApproveAsync(id, userId, request.Comment);
            return Ok(ApiResponse<ApprovalResultDto>.Success(result, "Approval processed"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/reject")]
    [RequirePermission("leaves.approve")]
    [ProducesResponseType(typeof(ApiResponse<ApprovalResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalActionRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token"));
        }

        try
        {
            var result = await _approvalUseCase.RejectAsync(id, userId, request.Comment);
            return Ok(ApiResponse<ApprovalResultDto>.Success(result, "Rejection processed"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("{leaveRequestId}/timeline")]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(ApiResponse<LeaveTimelineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimeline(int leaveRequestId)
    {
        try
        {
            var timeline = await _approvalUseCase.GetTimelineAsync(leaveRequestId);
            return Ok(ApiResponse<LeaveTimelineDto>.Success(timeline));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }
}