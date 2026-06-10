using HR_System.Application.DTOs.Role;
using HR_System.Application.Interfaces;

namespace HR_System.Application.UseCases.Role;

public class RoleUseCase
{
    private readonly IRoleRepository _roleRepository;

    public RoleUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        return roles.Select(r => new RoleDto
        {
            RoleId = r.RoleId,
            RoleName = r.RoleName
        }).ToList();
    }
}