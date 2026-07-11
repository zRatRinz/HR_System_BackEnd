using HR_System.Application.DTOs.Attendance;
using HR_System.Application.DTOs.Reports;

namespace HR_System.Application.Interfaces;

public interface IMyAttendanceReportPdfGenerator
{
    byte[] Generate(List<AttendanceDto> attendanceRecords, MyAttendanceReportOptions options);
}