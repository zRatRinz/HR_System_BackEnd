using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Leave;
using HR_System.Application.UseCases.Leave;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly LeaveUseCase _leaveUseCase;
    private readonly LeaveBalanceUseCase _leaveBalanceUseCase;
    private readonly LeaveCalendarUseCase _leaveCalendarUseCase;
    private readonly LeaveApprovalUseCase _leaveApprovalUseCase;

    public LeaveController(
        LeaveUseCase leaveUseCase,
        LeaveBalanceUseCase leaveBalanceUseCase,
        LeaveCalendarUseCase leaveCalendarUseCase,
        LeaveApprovalUseCase leaveApprovalUseCase)
    {
        _leaveUseCase = leaveUseCase;
        _leaveBalanceUseCase = leaveBalanceUseCase;
        _leaveCalendarUseCase = leaveCalendarUseCase;
        _leaveApprovalUseCase = leaveApprovalUseCase;
    }

    [HttpGet]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(ApiResponse<LeaveListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var response = await _leaveUseCase.GetMyLeavesAsync(status, page, limit);
        return Ok(ApiResponse<LeaveListResponse>.Success(response));
    }

    [HttpGet("overview")]
    [RequirePermission("leaves.view_overview")]
    [ProducesResponseType(typeof(ApiResponse<LeaveListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOverview(
        [FromQuery] string? status,
        [FromQuery] int? employeeId,
        [FromQuery] int? divisionId,
        [FromQuery] int? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var response = await _leaveUseCase.GetTeamLeavesAsync(status, employeeId, divisionId, departmentId, page, limit);
        return Ok(ApiResponse<LeaveListResponse>.Success(response));
    }

    [HttpPost]
    [RequirePermission("leaves.create")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token"));
        }

        var leaveRequest = await _leaveUseCase.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetAll), new { id = leaveRequest.LeaveRequestId }, ApiResponse<LeaveRequestDto>.Success(leaveRequest, "Leave request created"));
    }

    [HttpPut("{id}")]
    [RequirePermission("leaves.approve")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateLeaveRequest request)
    {
        var leaveRequest = await _leaveUseCase.UpdateStatusAsync(id, request);
        return Ok(ApiResponse<LeaveRequestDto>.Success(leaveRequest, $"Leave request {request.Status}"));
    }

    [HttpGet("balance")]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(ApiResponse<LeaveBalanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token"));
        }

        var balance = await _leaveBalanceUseCase.GetBalanceAsync(userId);
        return Ok(ApiResponse<LeaveBalanceDto>.Success(balance));
    }

    [HttpGet("calendar")]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(ApiResponse<LeaveCalendarDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar([FromQuery] int month, [FromQuery] int year)
    {
        var calendar = await _leaveCalendarUseCase.GetCalendarAsync(month, year);
        return Ok(ApiResponse<LeaveCalendarDto>.Success(calendar));
    }

    [HttpPut("requests/{id}/approve")]
    [RequirePermission("leaves.approve")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveLeaveRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token"));
        }

        var roles = User.FindFirst("roles")?.Value?.Split(',') ?? Array.Empty<string>();
        var approverRole = roles.FirstOrDefault(r =>
            r.Equals("HeadDepartment", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("HeadDivision", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("HR", StringComparison.OrdinalIgnoreCase)) ?? "";

        try
        {
            var result = await _leaveApprovalUseCase.ApproveAsync(id, approverRole, userId, request.Comment);
            return Ok(ApiResponse<LeaveRequestDto>.Success(result, "Leave request approved"));
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

    [HttpPut("requests/{id}/reject")]
    [RequirePermission("leaves.approve")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectLeaveRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token"));
        }

        var roles = User.FindFirst("roles")?.Value?.Split(',') ?? Array.Empty<string>();
        var approverRole = roles.FirstOrDefault(r =>
            r.Equals("HeadDepartment", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("HeadDivision", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("HR", StringComparison.OrdinalIgnoreCase)) ?? "";

        try
        {
            var result = await _leaveApprovalUseCase.RejectAsync(id, approverRole, userId, request.Comment);
            return Ok(ApiResponse<LeaveRequestDto>.Success(result, "Leave request rejected"));
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

    [HttpPut("requests/{id}/cancel")]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token"));
        }

        try
        {
            var result = await _leaveApprovalUseCase.CancelAsync(id, userId);
            return Ok(ApiResponse<LeaveRequestDto>.Success(result, "Leave request cancelled"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("requests/{id}/timeline")]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(ApiResponse<LeaveTimelineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimeline(int id)
    {
        try
        {
            var timeline = await _leaveApprovalUseCase.GetTimelineAsync(id);
            return Ok(ApiResponse<LeaveTimelineDto>.Success(timeline));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }
}