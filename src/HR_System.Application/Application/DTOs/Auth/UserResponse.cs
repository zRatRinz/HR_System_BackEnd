namespace HR_System.Application.DTOs.Auth;

public class UserResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string? PositionName { get; set; }
    public string? DivisionName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Avatar { get; set; }
}
