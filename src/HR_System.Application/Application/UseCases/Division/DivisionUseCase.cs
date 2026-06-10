using HR_System.Application.DTOs.Division;
using HR_System.Application.Interfaces;

namespace HR_System.Application.UseCases.Division;

public class DivisionUseCase
{
    private readonly IDivisionRepository _divisionRepository;

    public DivisionUseCase(IDivisionRepository divisionRepository)
    {
        _divisionRepository = divisionRepository;
    }

    public async Task<List<DivisionDto>> GetAllAsync()
    {
        var divisions = await _divisionRepository.GetAllAsync();
        return divisions.Select(d => new DivisionDto
        {
            DivisionId = d.DivisionId,
            DivisionName = d.DivisionName,
            Code = d.Code,
            Description = d.Description,
            Status = d.Status,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        }).ToList();
    }

    public async Task<DivisionDto?> GetByIdAsync(int id)
    {
        var division = await _divisionRepository.GetByIdAsync(id);
        if (division == null) return null;
        return new DivisionDto
        {
            DivisionId = division.DivisionId,
            DivisionName = division.DivisionName,
            Code = division.Code,
            Description = division.Description,
            Status = division.Status,
            CreatedAt = division.CreatedAt,
            UpdatedAt = division.UpdatedAt
        };
    }
}