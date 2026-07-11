using HR_System.Application.DTOs.Leave;
using HR_System.Application.DTOs.Reports;

namespace HR_System.Application.Interfaces;

public interface ILeaveReportPdfGenerator
{
    byte[] GenerateMyLeave(List<LeaveRequestDto> leaveRecords, MyLeaveReportOptions options);
}
