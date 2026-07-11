using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Payroll;
using HR_System.Application.Interfaces;
using HR_System.Application.UseCases.Payroll;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/payroll")]
public class PayrollController : ControllerBase
{
    private readonly PayrollUseCase _payrollUseCase;
    private readonly ICurrentUserService _currentUserService;

    public PayrollController(
        PayrollUseCase payrollUseCase,
        ICurrentUserService currentUserService)
    {
        _payrollUseCase = payrollUseCase;
        _currentUserService = currentUserService;
    }

    [HttpGet("my")]
    [RequirePermission("payroll.view")]
    [ProducesResponseType(typeof(ApiResponse<PayrollListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyPayroll(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var employeeId = _currentUserService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            return BadRequest(ApiResponse.Fail("Employee ID not found"));
        }

        var result = await _payrollUseCase.GetAllAsync(month, year, employeeId.Value, page, limit);
        return Ok(ApiResponse<PayrollListResponse>.Success(result));
    }

    [HttpGet("overview")]
    [RequirePermission("payroll.process")]
    [ProducesResponseType(typeof(ApiResponse<PayrollListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] int? employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var response = await _payrollUseCase.GetAllAsync(month, year, employeeId, page, limit);
        return Ok(ApiResponse<PayrollListResponse>.Success(response));
    }

    [HttpGet("{month}/{year}/{employeeId}")]
    [RequirePermission("payroll.view")]
    [ProducesResponseType(typeof(ApiResponse<PayrollDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int month, int year, int employeeId)
    {
        try
        {
            var result = await _payrollUseCase.GetDetailAsync(month, year, employeeId);
            if (result == null)
            {
                return NotFound(ApiResponse.Fail("Payroll not found"));
            }
            return Ok(ApiResponse<PayrollDetailDto>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("process")]
    [RequirePermission("payroll.process")]
    [ProducesResponseType(typeof(ApiResponse<ProcessPayrollResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Process([FromBody] ProcessPayrollRequest request)
    {
        try
        {
            var result = await _payrollUseCase.ProcessAsync(request.Month, request.Year);
            return Ok(ApiResponse<ProcessPayrollResponse>.Success(result, $"Payroll for {request.Year}-{request.Month:D2} processed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("unlock")]
    [RequirePermission("payroll.process")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Unlock([FromBody] UnlockPayrollRequest request)
    {
        try
        {
            await _payrollUseCase.UnlockAsync(request.Month, request.Year);
            return Ok(ApiResponse.Success($"Payroll for {request.Year}-{request.Month:D2} unlocked successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("{month}/{year}/approve")]
    [RequirePermission("payroll.process")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Approve(int month, int year, [FromBody] ApprovePayrollRequest request)
    {
        try
        {
            var approvedCount = await _payrollUseCase.ApproveByIdsAsync(month, year, request.PayrollRecordIds);
            return Ok(ApiResponse.Success(new { approvedCount }, $"Payroll approved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
