namespace HR_System.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(int userId, string email, IEnumerable<string> roles, int? divisionId, int? departmentId, int? employeeId, string[] permissions);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    (int UserId, string Email, string Roles)? GetTokenPayload(string token);
}
