using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Attendance;
using HR_System.Application.UseCases.Attendance;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceUseCase _attendanceUseCase;
    private readonly AttendanceTodayStatsUseCase _attendanceTodayStatsUseCase;

    public AttendanceController(AttendanceUseCase attendanceUseCase, AttendanceTodayStatsUseCase attendanceTodayStatsUseCase)
    {
        _attendanceUseCase = attendanceUseCase;
        _attendanceTodayStatsUseCase = attendanceTodayStatsUseCase;
    }

    [HttpGet]
    [RequirePermission("attendance.view")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? date,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var response = await _attendanceUseCase.GetMyAttendanceAsync(date, page, limit);
        return Ok(ApiResponse<AttendanceListResponse>.Success(response));
    }

    [HttpGet("overview")]
    [RequirePermission("attendance.view_overview")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOverview(
        [FromQuery] DateTime? date,
        [FromQuery] int? employeeId,
        [FromQuery] string? status,
        [FromQuery] int? divisionId,
        [FromQuery] int? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var response = await _attendanceUseCase.GetTeamAttendanceAsync(
            date, employeeId, status, divisionId, departmentId, page, limit);
        return Ok(ApiResponse<AttendanceListResponse>.Success(response));
    }

    [HttpPost("checkin")]
    [RequirePermission("attendance.checkin")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest? request)
    {
        var record = await _attendanceUseCase.CheckInAsync();
        return Ok(ApiResponse<AttendanceDto>.Success(record, "Check-in successful"));
    }

    [HttpPost("checkout")]
    [RequirePermission("attendance.checkout")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest? request)
    {
        var record = await _attendanceUseCase.CheckOutAsync();
        return Ok(ApiResponse<AttendanceDto>.Success(record, "Check-out successful"));
    }

    [HttpGet("status/today")]
    [RequirePermission("attendance.view")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayStatus()
    {
        var status = await _attendanceUseCase.GetTodayStatusAsync();
        return Ok(ApiResponse<AttendanceStatusResponse>.Success(status));
    }

    [HttpGet("today-stats")]
    [RequirePermission("attendance.view")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceTodayStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayStats()
    {
        var stats = await _attendanceTodayStatsUseCase.GetTodayStatsAsync();
        return Ok(ApiResponse<AttendanceTodayStatsDto>.Success(stats));
    }
}