using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Division;
using HR_System.Application.UseCases.Division;
using HR_System.Api.Filters;
using HR_System.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/divisions")]
public class DivisionsController : ControllerBase
{
    private readonly DivisionUseCase _divisionUseCase;

    public DivisionsController(DivisionUseCase divisionUseCase)
    {
        _divisionUseCase = divisionUseCase;
    }

    [HttpGet]
    [RequirePermission(Permissions.DivisionsView)]
    [ProducesResponseType(typeof(ApiResponse<List<DivisionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var divisions = await _divisionUseCase.GetAllAsync();
        return Ok(ApiResponse<List<DivisionDto>>.Success(divisions));
    }

    [HttpGet("{id}")]
    [RequirePermission(Permissions.DivisionsView)]
    [ProducesResponseType(typeof(ApiResponse<DivisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var division = await _divisionUseCase.GetByIdAsync(id);
        if (division == null)
        {
            return NotFound(ApiResponse.Fail("Division not found"));
        }
        return Ok(ApiResponse<DivisionDto>.Success(division));
    }
}