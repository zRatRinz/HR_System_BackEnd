namespace HR_System.Application.DTOs.Employee;

public class CreateEmployeeRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public int? DivisionId { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public DateTime HireDate { get; set; }
    public decimal? Salary { get; set; }
    public List<int> RoleIds { get; set; } = new();
}