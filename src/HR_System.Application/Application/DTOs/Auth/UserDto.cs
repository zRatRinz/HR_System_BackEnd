namespace HR_System.Application.DTOs.Auth;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Name { get; set; } = "";
    public List<string> Roles { get; set; } = new();
    public string? Role { get; set; }
    public int? DivisionId { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public string? Position { get; set; }
    public string? Division { get; set; }
    public string? Department { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}