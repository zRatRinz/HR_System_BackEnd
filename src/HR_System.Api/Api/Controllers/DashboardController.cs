using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Dashboard;
using HR_System.Application.UseCases.Dashboard;
using HR_System.Api.Filters;
using HR_System.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardUseCase _dashboardUseCase;

    public DashboardController(DashboardUseCase dashboardUseCase)
    {
        _dashboardUseCase = dashboardUseCase;
    }

    [HttpGet("stats")]
    [RequirePermission(Permissions.DashboardView)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStats()
    {
        var response = await _dashboardUseCase.GetStatsAsync();
        return Ok(ApiResponse<DashboardStatsResponse>.Success(response));
    }

    /// <summary>
    /// Get employee growth data for the last 6 months
    /// </summary>
    /// <remarks>
    /// Returns a list of employee counts per month for charting.
    /// Each entry contains: Label (e.g. "Jan 2024") and Value (employee count)
    /// </remarks>
    /// <response code="200">Returns employee growth chart data</response>
    /// <response code="401">Unauthorized - Invalid or missing token</response>
    //[HttpGet("employee-growth")]
    //[Authorize]
    //[ProducesResponseType(typeof(ApiResponse<EmployeeGrowthResponse>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    //public async Task<IActionResult> GetEmployeeGrowth()
    //{
    //    var response = await _dashboardUseCase.GetEmployeeGrowthAsync();
    //    return Ok(ApiResponse<EmployeeGrowthResponse>.Success(response));
    //}

    [HttpGet("departments")]
    [RequirePermission(Permissions.DashboardView)]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentData>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDepartments()
    {
        var response = await _dashboardUseCase.GetDepartmentsAsync();
        return Ok(ApiResponse<List<DepartmentData>>.Success(response.Departments));
    }
}
