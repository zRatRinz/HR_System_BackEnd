using HR_System.Application.Interfaces;

namespace HR_System.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash))
            return false;
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
