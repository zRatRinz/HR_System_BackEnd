using HR_System.Application.DTOs.Employee;
using HR_System.Application.DTOs.Reports;

namespace HR_System.Application.Interfaces;

public interface IEmployeeReportPdfGenerator
{
    byte[] Generate(List<EmployeeListDto> employees, EmployeeReportOptions options);
}