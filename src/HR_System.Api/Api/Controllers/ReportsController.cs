using HR_System.Api.Api.Common;
using HR_System.Application.UseCases.Reports;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportUseCase _reportUseCase;

    public ReportsController(ReportUseCase reportUseCase)
    {
        _reportUseCase = reportUseCase;
    }

    [HttpGet("{type}")]
    [RequirePermission("reports.view")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReport(
        string type,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var response = type.ToLower() switch
        {
            "leave-summary" => await _reportUseCase.GetLeaveSummaryAsync(startDate, endDate),
            "attendance-summary" => await _reportUseCase.GetAttendanceSummaryAsync(startDate, endDate),
            "payroll-summary" => await _reportUseCase.GetPayrollSummaryAsync(startDate, endDate),
            "employee-turnover" => await _reportUseCase.GetEmployeeTurnoverAsync(startDate, endDate),
            _ => throw new ArgumentException($"Invalid report type: {type}")
        };

        return Ok(ApiResponse.Success(response));
    }
}
