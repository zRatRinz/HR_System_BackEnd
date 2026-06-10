using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IDivisionRepository
{
    Task<Division?> GetByIdAsync(int id);
    Task<List<Division>> GetAllAsync();
    Task<Division> CreateAsync(Division division);
    Task<Division> UpdateAsync(Division division);
    Task DeleteAsync(int id);
}