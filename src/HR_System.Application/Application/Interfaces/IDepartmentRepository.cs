using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id);
    Task<List<Department>> GetAllAsync();
    Task<List<Department>> GetByDivisionIdAsync(int divisionId);
    Task<Department> CreateAsync(Department department);
    Task<Department> UpdateAsync(Department department);
    Task DeleteAsync(int id);
}