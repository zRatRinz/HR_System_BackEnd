using HR_System.Application.DTOs.Auth;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(int id);

    Task<UserDto?> GetByIdAsDtoAsync(int id);
    Task<List<UserDto>> GetAllAsDtoAsync();
    Task<UserDto?> GetByEmailAsDtoAsync(string email);
}
