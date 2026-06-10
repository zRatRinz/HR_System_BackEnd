using HR_System.Application.DTOs.Position;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;

namespace HR_System.Application.UseCases.Position;

public class PositionUseCase
{
    private readonly IPositionRepository _positionRepository;

    public PositionUseCase(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<PositionListResponse> GetAllAsync()
    {
        var positions = await _positionRepository.GetAllAsync();
        return new PositionListResponse
        {
            Data = positions.Select(MapToDto).ToList(),
            Total = positions.Count
        };
    }

    public async Task<PositionListResponse> GetActiveAsync()
    {
        var positions = await _positionRepository.GetActiveAsync();
        return new PositionListResponse
        {
            Data = positions.Select(MapToDto).ToList(),
            Total = positions.Count
        };
    }

    public async Task<PositionDto> GetByIdAsync(int id)
    {
        var position = await _positionRepository.GetByIdAsync(id);
        if (position == null)
        {
            throw new KeyNotFoundException("Position not found");
        }
        return MapToDto(position);
    }

    public async Task<PositionDto> CreateAsync(CreatePositionRequest request)
    {
        var existing = await _positionRepository.GetByCodeAsync(request.Code);
        if (existing != null)
        {
            throw new InvalidOperationException("Position code already exists");
        }

        var position = new HR_System.Domain.Entities.Position
        {
            PositionName = request.Name,
            Code = request.Code,
            Description = request.Description,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        await _positionRepository.CreateAsync(position);
        return MapToDto(position);
    }

    public async Task<PositionDto> UpdateAsync(int id, UpdatePositionRequest request)
    {
        var position = await _positionRepository.GetByIdAsync(id);
        if (position == null)
        {
            throw new KeyNotFoundException("Position not found");
        }

        if (request.Name != null)
        {
            position.PositionName = request.Name;
        }

        if (request.Code != null)
        {
            var existing = await _positionRepository.GetByCodeAsync(request.Code);
            if (existing != null && existing.PositionId != id)
            {
                throw new InvalidOperationException("Position code already exists");
            }
            position.Code = request.Code;
        }

        if (request.Description != null)
        {
            position.Description = request.Description;
        }

        if (request.Status != null)
        {
            position.Status = request.Status;
        }

        await _positionRepository.UpdateAsync(position);
        return MapToDto(position);
    }

    private static PositionDto MapToDto(HR_System.Domain.Entities.Position position)
    {
        return new PositionDto
        {
            PositionId = position.PositionId,
            PositionName = position.PositionName,
            Code = position.Code,
            Description = position.Description,
            Status = position.Status
        };
    }
}