using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HR_System.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace HR_System.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey = "PulsePoint-HR-Secret-Key-2024-This-Is-A-Very-Long-Secret-Key-For-JWT-Token";
    private readonly string _issuer = "PulsePointHR";
    private readonly string _audience = "PulsePointHR";

    public string GenerateAccessToken(int userId, string email, IEnumerable<string> roles, int? divisionId, int? departmentId, int? employeeId, string[] permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("employee_id", employeeId?.ToString() ?? ""),
            new Claim("division_id", divisionId?.ToString() ?? ""),
            new Claim("department_id", departmentId?.ToString() ?? ""),
            new Claim("permissions", JsonSerializer.Serialize(permissions))
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public (int UserId, string Email, string Roles)? GetTokenPayload(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userId = int.Parse(jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            var email = jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
            var roles = string.Join(",", jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));

            return (userId, email, roles);
        }
        catch
        {
            return null;
        }
    }
}