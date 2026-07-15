using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Position;
using HR_System.Application.UseCases.Position;
using HR_System.Api.Filters;
using HR_System.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly PositionUseCase _positionUseCase;

    public PositionsController(PositionUseCase positionUseCase)
    {
        _positionUseCase = positionUseCase;
    }

    [HttpGet]
    [RequirePermission(Permissions.PositionsView)]
    [ProducesResponseType(typeof(ApiResponse<List<PositionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var response = await _positionUseCase.GetAllAsync();
        return Ok(ApiResponse<List<PositionDto>>.Success(response.Data));
    }

    [HttpGet("active")]
    [RequirePermission(Permissions.PositionsView)]
    [ProducesResponseType(typeof(ApiResponse<List<PositionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var response = await _positionUseCase.GetActiveAsync();
        return Ok(ApiResponse<List<PositionDto>>.Success(response.Data));
    }

    [HttpGet("{id}")]
    [RequirePermission(Permissions.PositionsView)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var position = await _positionUseCase.GetByIdAsync(id);
        return Ok(ApiResponse<PositionDto>.Success(position));
    }

    [HttpPost]
    [RequirePermission(Permissions.PositionsCreate)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreatePositionRequest request)
    {
        var position = await _positionUseCase.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = position.PositionId }, ApiResponse<PositionDto>.Success(position, "Position created"));
    }

    [HttpPut("{id}")]
    [RequirePermission(Permissions.PositionsEdit)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePositionRequest request)
    {
        var position = await _positionUseCase.UpdateAsync(id, request);
        return Ok(ApiResponse<PositionDto>.Success(position, "Position updated"));
    }
}