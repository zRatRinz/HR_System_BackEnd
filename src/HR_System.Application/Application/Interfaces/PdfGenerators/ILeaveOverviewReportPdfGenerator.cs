using HR_System.Application.DTOs.Leave;
using HR_System.Application.DTOs.Reports;

namespace HR_System.Application.Interfaces;

public interface ILeaveOverviewReportPdfGenerator
{
    byte[] Generate(List<LeaveRequestDto> leaveRecords, LeaveOverviewReportOptions options);
}
