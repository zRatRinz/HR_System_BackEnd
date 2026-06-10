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
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var response = await _approvalUseCase.GetAllAsync(status);
        return Ok(ApiResponse<ApprovalListResponse>.Success(response));
    }

    [HttpPut("{id}")]
    [RequirePermission("leaves.approve")]
    [ProducesResponseType(typeof(ApiResponse<ApprovalItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApprovalRequest request)
    {
        var item = await _approvalUseCase.UpdateStatusAsync(id, request);
        return Ok(ApiResponse<ApprovalItemDto>.Success(item, $"Approval {request.Status.ToString().ToLower()}"));
    }
}
