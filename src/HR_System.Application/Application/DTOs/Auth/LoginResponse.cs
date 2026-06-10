namespace HR_System.Application.DTOs.Auth;

public class LoginResponse
{
    public UserResponse User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
}
