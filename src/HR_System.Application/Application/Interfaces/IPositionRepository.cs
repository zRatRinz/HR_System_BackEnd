using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IPositionRepository
{
    Task<Position> CreateAsync(Position position);
    Task<Position> UpdateAsync(Position position);
    Task<Position?> GetByIdAsync(int id);
    Task<Position?> GetByCodeAsync(string code);
    Task<List<Position>> GetAllAsync();
    Task<List<Position>> GetActiveAsync();
}