using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
}