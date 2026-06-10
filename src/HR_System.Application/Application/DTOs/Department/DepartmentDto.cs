namespace HR_System.Application.DTOs.Department;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public int DivisionId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}