namespace HR_System.Domain.Entities;

public class Department
{
    public int DepartmentId { get; set; }
    public int DivisionId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Division? Division { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<User> Users { get; set; } = new List<User>();
}