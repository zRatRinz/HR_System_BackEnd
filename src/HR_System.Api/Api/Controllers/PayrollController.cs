using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Payroll;
using HR_System.Application.UseCases.Payroll;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/payroll")]
public class PayrollController : ControllerBase
{
    private readonly PayrollUseCase _payrollUseCase;

    public PayrollController(PayrollUseCase payrollUseCase)
    {
        _payrollUseCase = payrollUseCase;
    }

    [HttpGet]
    [RequirePermission("payroll.view")]
    [ProducesResponseType(typeof(ApiResponse<PayrollListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? period,
        [FromQuery] int? employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var response = await _payrollUseCase.GetAllAsync(period, employeeId, page, limit);
        return Ok(ApiResponse<PayrollListResponse>.Success(response));
    }

    [HttpPost("process")]
    [RequirePermission("payroll.process")]
    [ProducesResponseType(typeof(ApiResponse<ProcessPayrollResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Process([FromBody] ProcessPayrollRequest request)
    {
        var result = await _payrollUseCase.ProcessAsync(request.Period);
        return Ok(ApiResponse<ProcessPayrollResponse>.Success(result, "Payroll processed successfully"));
    }
}
