using HR_System.Application.DTOs.Attendance;
using HR_System.Application.DTOs.Reports;

namespace HR_System.Application.Interfaces;

public interface IAttendanceOverviewReportPdfGenerator
{
    byte[] Generate(List<AttendanceDto> attendanceRecords, AttendanceOverviewReportOptions options);
}