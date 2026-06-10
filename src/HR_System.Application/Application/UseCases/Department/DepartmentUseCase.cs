using HR_System.Application.DTOs.Department;
using HR_System.Application.Interfaces;
using DepartmentEntity = HR_System.Domain.Entities.Department;

namespace HR_System.Application.UseCases.Department;

public class DepartmentUseCase
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IScopeService _scopeService;

    public DepartmentUseCase(IDepartmentRepository departmentRepository, IScopeService scopeService)
    {
        _departmentRepository = departmentRepository;
        _scopeService = scopeService;
    }

    public async Task<List<DepartmentDto>> GetAllAsync(int? divisionId = null)
    {
        var roles = _scopeService.GetRoles();
        var isBypassScope = roles.Any(r =>
            r.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("hr", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("manager", StringComparison.OrdinalIgnoreCase));
        var isHeadDivision = roles.Any(r => r.Equals("headdivision", StringComparison.OrdinalIgnoreCase));

        List<DepartmentEntity> departments;

        if (isBypassScope)
        {
            departments = divisionId.HasValue
                ? await _departmentRepository.GetByDivisionIdAsync(divisionId.Value)
                : await _departmentRepository.GetAllAsync();
        }
        else if (isHeadDivision)
        {
            var scopeDivisionId = _scopeService.GetDivisionId();
            if (scopeDivisionId.HasValue)
            {
                departments = await _departmentRepository.GetByDivisionIdAsync(scopeDivisionId.Value);
            }
            else
            {
                departments = new List<DepartmentEntity>();
            }
        }
        else
        {
            var departmentId = _scopeService.GetDepartmentId();
            if (departmentId.HasValue)
            {
                var dept = await _departmentRepository.GetByIdAsync(departmentId.Value);
                return dept != null ? new List<DepartmentDto> { MapToDto(dept) } : new List<DepartmentDto>();
            }
            return new List<DepartmentDto>();
        }

        return departments.Select(MapToDto).ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null) return null;
        return MapToDto(department);
    }

    private static DepartmentDto MapToDto(DepartmentEntity dept) => new DepartmentDto
    {
        DepartmentId = dept.DepartmentId,
        DivisionId = dept.DivisionId,
        DepartmentName = dept.DepartmentName,
        Code = dept.Code,
        Description = dept.Description,
        Status = dept.Status,
        CreatedAt = dept.CreatedAt,
        UpdatedAt = dept.UpdatedAt
    };
}