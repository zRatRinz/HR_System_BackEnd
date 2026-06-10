namespace HR_System.Application.DTOs.Employee;

public class EmployeeDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public int? PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AvatarColor { get; set; }
    public string? Phone { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Address { get; set; }
    public decimal Salary { get; set; }
    public List<int> RoleIds { get; set; } = new();
}