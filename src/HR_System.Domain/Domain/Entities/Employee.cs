using HR_System.Domain.Enums;

namespace HR_System.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? PositionId { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? DivisionId { get; set; }
    public int? DepartmentId { get; set; }
}