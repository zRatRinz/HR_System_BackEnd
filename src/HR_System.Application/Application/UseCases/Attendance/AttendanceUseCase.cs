using HR_System.Application.DTOs.Attendance;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Attendance;

public class AttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IScopeService _scopeService;

    public AttendanceUseCase(
        IAttendanceRepository attendanceRepository,
        IEmployeeRepository employeeRepository,
        IScopeService scopeService)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
        _scopeService = scopeService;
    }

    public async Task<AttendanceListResponse> GetAllAsync(DateTime? date, int? employeeId, int page, int limit)
    {
        var roles = _scopeService.GetRoles();
        var bypassScope = roles.Any(r => r == "Admin" || r == "HR");
        var isEmployee = roles.Any(r => r == "Employee");

        int? scopeEmployeeId = null;
        int? divisionId = null;
        int? departmentId = null;

        if (!bypassScope)
        {
            if (isEmployee)
            {
                scopeEmployeeId = _scopeService.GetEmployeeId();
            }
            else
            {
                divisionId = _scopeService.GetDivisionId();
                departmentId = _scopeService.GetDepartmentId();
            }
        }

        var items = await _attendanceRepository.GetAllAsDtoAsync(
            date, employeeId, page, limit,
            divisionId, departmentId, bypassScope,
            scopeEmployeeId);
        var total = items.Count;

        return new AttendanceListResponse
        {
            Attendance = items,
            Total = total,
            Page = page
        };
    }

    public async Task<AttendanceDto> CheckInAsync()
    {
        var employeeId = _scopeService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            throw new UnauthorizedAccessException("Employee not found in token");
        }

        var employee = await _employeeRepository.GetByIdAsDtoAsync(employeeId.Value);
        if (employee == null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        var today = DateTime.UtcNow.Date;
        var existingRecord = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId.Value, today);

        if (existingRecord != null)
        {
            throw new InvalidOperationException("Already checked in today");
        }

        var utcNow = DateTime.UtcNow;
        var thaiZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var thaiNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, thaiZone);

        var status = thaiNow.TimeOfDay <= new TimeSpan(9, 0, 0)
            ? AttendanceStatus.OnTime
            : AttendanceStatus.Late;

        var record = new AttendanceRecord
        {
            EmployeeId = employeeId.Value,
            Date = today,
            CheckIn = utcNow,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await _attendanceRepository.CreateAsync(record);

        return new AttendanceDto
        {
            AttendanceRecordId = record.AttendanceRecordId,
            EmployeeId = employeeId.Value,
            EmployeeName = employee.Name,
            Date = record.Date,
            CheckIn = record.CheckIn,
            CheckOut = null,
            Status = record.Status.ToString().ToLower()
        };
    }

    public async Task<AttendanceDto> CheckOutAsync()
    {
        var employeeId = _scopeService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            throw new UnauthorizedAccessException("Employee not found in token");
        }

        var today = DateTime.UtcNow.Date;
        var record = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId.Value, today);

        if (record == null)
        {
            throw new KeyNotFoundException("No check-in record found for today");
        }

        if (record.CheckOut != null)
        {
            throw new InvalidOperationException("Already checked out today");
        }

        var updateRecord = new AttendanceRecord
        {
            AttendanceRecordId = record.AttendanceRecordId,
            EmployeeId = record.EmployeeId,
            Date = record.Date,
            CheckIn = record.CheckIn,
            CheckOut = DateTime.UtcNow,
            Status = Enum.Parse<AttendanceStatus>(record.Status, true)
        };
        await _attendanceRepository.UpdateAsync(updateRecord);

        return new AttendanceDto
        {
            AttendanceRecordId = record.AttendanceRecordId,
            EmployeeId = employeeId.Value,
            EmployeeName = record.EmployeeName,
            Date = record.Date,
            CheckIn = record.CheckIn,
            CheckOut = updateRecord.CheckOut,
            Status = record.Status
        };
    }

    public async Task<AttendanceStatusResponse> GetTodayStatusAsync()
    {
        var employeeId = _scopeService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            throw new UnauthorizedAccessException("Employee not found in token");
        }

        var today = DateTime.UtcNow.Date;
        var record = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId.Value, today);

        if (record == null)
        {
            return new AttendanceStatusResponse
            {
                HasCheckedIn = false,
                HasCheckedOut = false,
                CheckInTime = null,
                CheckOutTime = null,
                Status = string.Empty
            };
        }

        return new AttendanceStatusResponse
        {
            HasCheckedIn = true,
            HasCheckedOut = record.CheckOut != null,
            CheckInTime = record.CheckIn,
            CheckOutTime = record.CheckOut,
            Status = record.Status
        };
    }
}