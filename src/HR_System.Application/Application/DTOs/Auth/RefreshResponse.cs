namespace HR_System.Application.DTOs.Auth;

public class RefreshResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
}
