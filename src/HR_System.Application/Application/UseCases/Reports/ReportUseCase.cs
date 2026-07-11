using HR_System.Application.DTOs.Reports;
using HR_System.Application.Interfaces;
using HR_System.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace HR_System.Application.UseCases.Reports;

public class ReportUseCase
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IPayrollRepository _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDivisionRepository _divisionRepository;
    private readonly IEmployeeReportPdfGenerator _employeeReportPdfGenerator;
    private readonly IMyAttendanceReportPdfGenerator _myAttendanceReportPdfGenerator;
    private readonly IAttendanceOverviewReportPdfGenerator _attendanceOverviewReportPdfGenerator;
    private readonly ILeaveReportPdfGenerator _leaveReportPdfGenerator;
    private readonly ILeaveOverviewReportPdfGenerator _leaveOverviewReportPdfGenerator;
    private readonly ILeaveCertificatePdfGenerator _leaveCertificatePdfGenerator;
    private readonly IPayrollPdfGenerator _payrollPdfGenerator;
    private readonly IScopeService _scopeService;
    private readonly IConfiguration _configuration;

    public ReportUseCase(
        ILeaveRepository leaveRepository,
        IHolidayRepository holidayRepository,
        IAttendanceRepository attendanceRepository,
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IDivisionRepository divisionRepository,
        IEmployeeReportPdfGenerator employeeReportPdfGenerator,
        IMyAttendanceReportPdfGenerator myAttendanceReportPdfGenerator,
        IAttendanceOverviewReportPdfGenerator attendanceOverviewReportPdfGenerator,
        ILeaveReportPdfGenerator leaveReportPdfGenerator,
        ILeaveOverviewReportPdfGenerator leaveOverviewReportPdfGenerator,
        ILeaveCertificatePdfGenerator leaveCertificatePdfGenerator,
        IPayrollPdfGenerator payrollPdfGenerator,
        IScopeService scopeService,
        IConfiguration configuration)
    {
        _leaveRepository = leaveRepository;
        _holidayRepository = holidayRepository;
        _attendanceRepository = attendanceRepository;
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _divisionRepository = divisionRepository;
        _employeeReportPdfGenerator = employeeReportPdfGenerator;
        _myAttendanceReportPdfGenerator = myAttendanceReportPdfGenerator;
        _attendanceOverviewReportPdfGenerator = attendanceOverviewReportPdfGenerator;
        _leaveReportPdfGenerator = leaveReportPdfGenerator;
        _leaveOverviewReportPdfGenerator = leaveOverviewReportPdfGenerator;
        _leaveCertificatePdfGenerator = leaveCertificatePdfGenerator;
        _payrollPdfGenerator = payrollPdfGenerator;
        _scopeService = scopeService;
        _configuration = configuration;
    }

    public Task<ReportResponse> GetLeaveSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        return Task.FromResult(new ReportResponse
        {
            Data = new { },
            Summary = new { TotalRequests = 0, Approved = 0, Rejected = 0, Pending = 0 }
        });
    }

    public Task<ReportResponse> GetAttendanceSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        return Task.FromResult(new ReportResponse
        {
            Data = new { },
            Summary = new { TotalRecords = 0, OnTime = 0, Late = 0, Absent = 0 }
        });
    }

    public Task<ReportResponse> GetPayrollSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        return Task.FromResult(new ReportResponse
        {
            Data = new { },
            Summary = new { TotalPayroll = 0, TotalEmployees = 0, AverageSalary = 0 }
        });
    }

    public Task<ReportResponse> GetEmployeeTurnoverAsync(DateTime? startDate, DateTime? endDate)
    {
        return Task.FromResult(new ReportResponse
        {
            Data = new { },
            Summary = new { TotalTurnover = 0, NewHires = 0, Departures = 0 }
        });
    }

    public async Task<byte[]> GetEmployeeReportPdfAsync(EmployeeReportQuery query)
    {
        var (employees, total) = await _employeeRepository.GetAllAsDtoAsync(
            search: null,
            department: query.DepartmentId,
            division: query.DivisionId,
            position: null,
            status: query.Status,
            page: 1,
            limit: 10000
        );

        var divisionName = query.DivisionId.HasValue
            ? (await _divisionRepository.GetByIdAsync(query.DivisionId.Value))?.DivisionName
            : "ทั้งหมด";

        var departmentName = query.DepartmentId.HasValue
            ? (await _departmentRepository.GetByIdAsync(query.DepartmentId.Value))?.DepartmentName
            : "ทั้งหมด";

        var pdfBytes = _employeeReportPdfGenerator.Generate(employees, new EmployeeReportOptions
        {
            DivisionName = divisionName,
            DepartmentName = departmentName,
            Status = query.Status ?? "ทั้งหมด",
            GeneratedAt = DateTime.UtcNow
        });

        return pdfBytes;
    }

    public async Task<byte[]> GetMyAttendancePdfAsync(DateTime? startDate, DateTime? endDate)
    {
        var employeeId = _scopeService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            throw new InvalidOperationException("ไม่พบข้อมูลพนักงาน");
        }

        var employee = await _employeeRepository.GetByIdAsDtoAsync(employeeId.Value);
        var employeeName = employee?.Name ?? "-";

        var effectiveStartDate = startDate;
        var effectiveEndDate = endDate;

        var attendanceRecords = await _attendanceRepository.GetByEmployeeIdAsync(employeeId.Value, effectiveStartDate, effectiveEndDate, 1, 10000);

        var summaryStartDate = effectiveStartDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
        var summaryEndDate = effectiveEndDate ?? DateTime.UtcNow.Date;

        var holidayCount = await _holidayRepository.CountHolidaysInRangeAsync(summaryStartDate, summaryEndDate);

        var onTimeCount = attendanceRecords.Items.Count(r => r.Status == "OnTime");
        var lateCount = attendanceRecords.Items.Count(r => r.Status == "Late");
        var absentCount = attendanceRecords.Items.Count(r => r.Status == "Absent");

        var sickLeaveDays = await _leaveRepository.GetApprovedLeaveTotalDaysAsync(employeeId.Value, summaryStartDate, summaryEndDate, "Sick");
        var personalLeaveDays = await _leaveRepository.GetApprovedLeaveTotalDaysAsync(employeeId.Value, summaryStartDate, summaryEndDate, "Personal");

        var pdfBytes = _myAttendanceReportPdfGenerator.Generate(attendanceRecords.Items, new MyAttendanceReportOptions
        {
            EmployeeName = employeeName,
            StartDate = summaryStartDate,
            EndDate = summaryEndDate,
            GeneratedAt = DateTime.UtcNow,
            OnTimeCount = onTimeCount,
            LateCount = lateCount,
            AbsentCount = absentCount,
            SickLeaveDays = sickLeaveDays,
            PersonalLeaveDays = personalLeaveDays
        });

        return pdfBytes;
    }

    public async Task<byte[]> GetMyLeavePdfAsync(DateTime? startDate, DateTime? endDate)
    {
        var employeeId = _scopeService.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            throw new InvalidOperationException("ไม่พบข้อมูลพนักงาน");
        }

        var employee = await _employeeRepository.GetByIdAsDtoAsync(employeeId.Value);
        var employeeName = employee?.Name ?? "-";

        var effectiveStartDate = startDate;
        var effectiveEndDate = endDate;

        var leavePolicy = _configuration.GetSection("LeavePolicy");
        var annualTotal = leavePolicy.GetSection("Annual").GetValue<int>("Total");
        var sickTotal = leavePolicy.GetSection("Sick").GetValue<int>("Total");

        var usedDays = await _leaveRepository.GetUsedDaysByTypeAsync(employeeId.Value);
        var annualUsed = usedDays.GetValueOrDefault("Annual", 0);
        var sickUsed = usedDays.GetValueOrDefault("Sick", 0);

        var leaves = await _leaveRepository.GetApprovedLeavesByEmployeeIdAsync(employeeId.Value, effectiveStartDate, effectiveEndDate);

        var pdfBytes = _leaveReportPdfGenerator.GenerateMyLeave(leaves, new MyLeaveReportOptions
        {
            EmployeeName = employeeName,
            StartDate = effectiveStartDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1),
            EndDate = effectiveEndDate ?? DateTime.UtcNow.Date,
            GeneratedAt = DateTime.UtcNow,
            AnnualTotal = annualTotal,
            AnnualUsed = annualUsed,
            AnnualBalance = Math.Max(0, annualTotal - annualUsed),
            SickTotal = sickTotal,
            SickUsed = sickUsed,
            SickBalance = Math.Max(0, sickTotal - sickUsed),
            TotalUsedDays = annualUsed + sickUsed,
            TotalBalance = Math.Max(0, (annualTotal - annualUsed) + (sickTotal - sickUsed))
        });

        return pdfBytes;
    }

    public async Task<byte[]> GetLeaveOverviewPdfAsync(
        int? division,
        int? department,
        DateTime? startDate,
        DateTime? endDate)
    {
        var roles = _scopeService.GetRoles();
        var userDivisionId = _scopeService.GetDivisionId();
        var userDepartmentId = _scopeService.GetDepartmentId();
        var userId = _scopeService.GetUserId();

        var bypassScope = roles.Contains("Admin") || roles.Contains("HR");

        int? effectiveDivisionId;
        int? effectiveDepartmentId;

        if (bypassScope)
        {
            effectiveDivisionId = division;
            effectiveDepartmentId = department;
        }
        else
        {
            effectiveDivisionId = division ?? userDivisionId;
            effectiveDepartmentId = department ?? userDepartmentId;
        }

        var leaves = await _leaveRepository.GetApprovedLeavesAsync(
            startDate, endDate,
            bypassScope, effectiveDivisionId, effectiveDepartmentId, userId);

        var scopeDisplay = "ทั้งหมด";
        if (!bypassScope)
        {
            if (department.HasValue)
            {
                var dept = await _departmentRepository.GetByIdAsync(department.Value);
                scopeDisplay = dept?.DepartmentName ?? department.Value.ToString();
            }
            else if (division.HasValue)
            {
                var div = await _divisionRepository.GetByIdAsync(division.Value);
                scopeDisplay = div?.DivisionName ?? division.Value.ToString();
            }
        }

        string divisionName;
        string departmentName;

        if (division.HasValue)
        {
            divisionName = (await _divisionRepository.GetByIdAsync(division.Value))?.DivisionName ?? "ทั้งหมด";
        }
        else if (roles.Contains("HR") || roles.Contains("Admin"))
        {
            divisionName = "ทั้งหมด";
        }
        else if (roles.Contains("HeadDivision"))
        {
            divisionName = userDivisionId.HasValue
                ? (await _divisionRepository.GetByIdAsync(userDivisionId.Value))?.DivisionName ?? "ทั้งหมด"
                : "ทั้งหมด";
        }
        else if (roles.Contains("HeadDepartment"))
        {
            var dept = userDepartmentId.HasValue
                ? await _departmentRepository.GetByIdAsync(userDepartmentId.Value)
                : null;
            divisionName = dept?.DivisionId != null
                ? (await _divisionRepository.GetByIdAsync(dept.DivisionId))?.DivisionName ?? "ทั้งหมด"
                : "ทั้งหมด";
        }
        else
        {
            divisionName = "ทั้งหมด";
        }

        if (department.HasValue)
        {
            departmentName = (await _departmentRepository.GetByIdAsync(department.Value))?.DepartmentName ?? "ทั้งหมด";
        }
        else if (roles.Contains("HR") || roles.Contains("Admin"))
        {
            departmentName = "ทั้งหมด";
        }
        else if (roles.Contains("HeadDepartment"))
        {
            departmentName = userDepartmentId.HasValue
                ? (await _departmentRepository.GetByIdAsync(userDepartmentId.Value))?.DepartmentName ?? "ทั้งหมด"
                : "ทั้งหมด";
        }
        else
        {
            departmentName = "ทั้งหมด";
        }

        var options = new LeaveOverviewReportOptions
        {
            DivisionName = divisionName,
            DepartmentName = departmentName,
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow,
            ScopeDisplay = scopeDisplay
        };

        return _leaveOverviewReportPdfGenerator.Generate(leaves, options);
    }

    public async Task<byte[]> GetLeaveCertificatePdfAsync(int leaveRequestId)
    {
        var certificate = await _leaveRepository.GetCertificateByIdAsync(leaveRequestId);

        if (certificate == null)
        {
            throw new KeyNotFoundException($"Leave request with ID {leaveRequestId} not found");
        }

        return _leaveCertificatePdfGenerator.Generate(certificate);
    }

    public async Task<byte[]> GetPayrollPdfAsync(int payrollRecordId)
    {
        var payroll = await _payrollRepository.GetByIdAsDtoAsync(payrollRecordId);

        if (payroll == null)
        {
            throw new KeyNotFoundException($"Payroll record with ID {payrollRecordId} not found");
        }

        return _payrollPdfGenerator.Generate(payroll);
    }

    public async Task<byte[]> GetAttendanceOverviewPdfAsync(
        int? division,
        int? department,
        int? scopeEmployeeId,
        DateTime? startDate,
        DateTime? endDate,
        string? status)
    {
        var roles = _scopeService.GetRoles();
        var userDivisionId = _scopeService.GetDivisionId();
        var userDepartmentId = _scopeService.GetDepartmentId();

        bool bypassScope = roles.Contains("HR") || roles.Contains("Admin");

        int? scopeDivisionId;
        int? scopeDepartmentId;

        if (roles.Contains("HR") || roles.Contains("Admin"))
        {
            bypassScope = true;
            scopeDivisionId = division;
            scopeDepartmentId = department;
        }
        else if (roles.Contains("HeadDivision"))
        {
            bypassScope = false;
            scopeDivisionId = userDivisionId;

            if (department.HasValue)
            {
                var dept = await _departmentRepository.GetByIdAsync(department.Value);
                if (dept != null && dept.DivisionId == userDivisionId)
                {
                    scopeDepartmentId = department.Value;
                }
                else
                {
                    scopeDepartmentId = null;
                }
            }
            else
            {
                scopeDepartmentId = null;
            }
        }
        else if (roles.Contains("HeadDepartment"))
        {
            bypassScope = false;
            scopeDivisionId = null;
            scopeDepartmentId = userDepartmentId;
        }
        else
        {
            bypassScope = false;
            scopeDivisionId = null;
            scopeDepartmentId = null;
        }

        bool isOutOfScope = false;

        if (!bypassScope)
        {
            if (roles.Contains("HeadDivision"))
            {
                if (division.HasValue && division.Value != userDivisionId)
                {
                    isOutOfScope = true;
                }
                if (department.HasValue)
                {
                    var dept = await _departmentRepository.GetByIdAsync(department.Value);
                    if (dept == null || dept.DivisionId != userDivisionId)
                    {
                        isOutOfScope = true;
                    }
                }
            }
            else if (roles.Contains("HeadDepartment"))
            {
                if (division.HasValue)
                {
                    var dept = await _departmentRepository.GetByIdAsync(userDepartmentId.Value);
                    if (dept == null || dept.DivisionId != division.Value)
                    {
                        isOutOfScope = true;
                    }
                }
                if (department.HasValue)
                {
                    if (department.Value != userDepartmentId)
                    {
                        isOutOfScope = true;
                    }
                    else
                    {
                        var dept = await _departmentRepository.GetByIdAsync(department.Value);
                        var userDept = await _departmentRepository.GetByIdAsync(userDepartmentId.Value);
                        if (dept == null || userDept == null || dept.DivisionId != userDept.DivisionId)
                        {
                            isOutOfScope = true;
                        }
                    }
                }
            }
        }

        if (isOutOfScope)
        {
            var outOfScopeStartDate = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var outOfScopeEndDate = endDate ?? DateTime.UtcNow.Date;

            var emptyPdfBytes = _attendanceOverviewReportPdfGenerator.Generate(new List<Application.DTOs.Attendance.AttendanceDto>(), new AttendanceOverviewReportOptions
            {
                DivisionName = division.HasValue
                    ? (await _divisionRepository.GetByIdAsync(division.Value))?.DivisionName ?? "ทั้งหมด"
                    : "ทั้งหมด",
                DepartmentName = department.HasValue
                    ? (await _departmentRepository.GetByIdAsync(department.Value))?.DepartmentName ?? "ทั้งหมด"
                    : "ทั้งหมด",
                Status = status ?? "ทั้งหมด",
                StartDate = outOfScopeStartDate,
                EndDate = outOfScopeEndDate,
                GeneratedAt = DateTime.UtcNow
            });
            return emptyPdfBytes;
        }

        var (attendanceRecords, total) = await _attendanceRepository.GetAllAsDtoAsync(
            startDate: startDate,
            endDate: endDate,
            employeeId: null,
            page: 1,
            limit: 10000,
            scopeDivisionId: scopeDivisionId,
            scopeDepartmentId: scopeDepartmentId,
            bypassScope: bypassScope,
            scopeEmployeeId: scopeEmployeeId,
            status: status);

        string divisionName;
        string departmentName;

        if (division.HasValue)
        {
            divisionName = (await _divisionRepository.GetByIdAsync(division.Value))?.DivisionName ?? "ทั้งหมด";
        }
        else if (roles.Contains("HR") || roles.Contains("Admin"))
        {
            divisionName = "ทั้งหมด";
        }
        else if (roles.Contains("HeadDivision"))
        {
            divisionName = userDivisionId.HasValue
                ? (await _divisionRepository.GetByIdAsync(userDivisionId.Value))?.DivisionName ?? "ทั้งหมด"
                : "ทั้งหมด";
        }
        else if (roles.Contains("HeadDepartment"))
        {
            if (userDepartmentId.HasValue)
            {
                var dept = await _departmentRepository.GetByIdAsync(userDepartmentId.Value);
                var divId = dept?.DivisionId;
                divisionName = divId.HasValue
                    ? (await _divisionRepository.GetByIdAsync(divId.Value))?.DivisionName ?? "ทั้งหมด"
                    : "ทั้งหมด";
            }
            else
            {
                divisionName = "ทั้งหมด";
            }
        }
        else
        {
            divisionName = "ทั้งหมด";
        }

        if (department.HasValue)
        {
            departmentName = (await _departmentRepository.GetByIdAsync(department.Value))?.DepartmentName ?? "ทั้งหมด";
        }
        else if (roles.Contains("HR") || roles.Contains("Admin"))
        {
            departmentName = "ทั้งหมด";
        }
        else if (roles.Contains("HeadDepartment"))
        {
            departmentName = userDepartmentId.HasValue
                ? (await _departmentRepository.GetByIdAsync(userDepartmentId.Value))?.DepartmentName ?? "ทั้งหมด"
                : "ทั้งหมด";
        }
        else
        {
            departmentName = "ทั้งหมด";
        }

        var effectiveStartDate = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var effectiveEndDate = endDate ?? DateTime.UtcNow.Date;

        var pdfBytes = _attendanceOverviewReportPdfGenerator.Generate(attendanceRecords, new AttendanceOverviewReportOptions
        {
            DivisionName = divisionName,
            DepartmentName = departmentName,
            Status = status ?? "ทั้งหมด",
            StartDate = effectiveStartDate,
            EndDate = effectiveEndDate,
            GeneratedAt = DateTime.UtcNow
        });

        return pdfBytes;
    }
}
