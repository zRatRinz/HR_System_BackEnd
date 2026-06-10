namespace HR_System.Application.DTOs.Employee;

public class EmployeeListResponse
{
    public List<EmployeeListDto> Employees { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
}
