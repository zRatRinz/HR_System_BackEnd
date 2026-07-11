using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Reports;
using HR_System.Application.Interfaces;
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

    [HttpGet("employees/pdf")]
    [RequirePermission("reports.view")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployeeReportPdf(
        [FromQuery] int? division,
        [FromQuery] int? department,
        [FromQuery] string? status)
    {
        var query = new EmployeeReportQuery
        {
            DivisionId = division,
            DepartmentId = department,
            Status = status
        };

        var pdfBytes = await _reportUseCase.GetEmployeeReportPdfAsync(query);

        return File(pdfBytes, "application/pdf", $"employee-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf");
    }

    [HttpGet("my-attendance/pdf")]
    [RequirePermission("attendance.view")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyAttendancePdf(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var pdfBytes = await _reportUseCase.GetMyAttendancePdfAsync(startDate, endDate);
            return File(pdfBytes, "application/pdf", $"my-attendance-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("attendance-overview/pdf")]
    [RequirePermission("attendance.view_overview")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAttendanceOverviewPdf(
        [FromQuery] int? division,
        [FromQuery] int? department,
        [FromQuery] string? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var pdfBytes = await _reportUseCase.GetAttendanceOverviewPdfAsync(
            division,
            department,
            scopeEmployeeId: null,
            startDate,
            endDate,
            status);

        return File(pdfBytes, "application/pdf", $"attendance-overview-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf");
    }

    [HttpGet("my-leave/pdf")]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyLeavePdf(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var pdfBytes = await _reportUseCase.GetMyLeavePdfAsync(startDate, endDate);
            return File(pdfBytes, "application/pdf", $"my-leave-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("leave-overview/pdf")]
    [RequirePermission("leaves.view_overview")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaveOverviewPdf(
        [FromQuery] int? division,
        [FromQuery] int? department,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var pdfBytes = await _reportUseCase.GetLeaveOverviewPdfAsync(
            division,
            department,
            startDate,
            endDate);

        return File(pdfBytes, "application/pdf", $"leave-overview-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf");
    }

    [HttpGet("leave-certificate/{leaveRequestId}/pdf")]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaveCertificatePdf(int leaveRequestId)
    {
        try
        {
            var pdfBytes = await _reportUseCase.GetLeaveCertificatePdfAsync(leaveRequestId);
            return File(pdfBytes, "application/pdf", $"leave-certificate-{leaveRequestId}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail($"ไม่พบข้อมูลการลาที่ ID: {leaveRequestId}"));
        }
    }

    [HttpGet("payroll/{id}/pdf")]
    [RequirePermission("payroll.view")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayrollPdf(int id)
    {
        try
        {
            var pdfBytes = await _reportUseCase.GetPayrollPdfAsync(id);
            var fileName = $"payroll-payslip-{id}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Payroll record not found"));
        }
    }
}
