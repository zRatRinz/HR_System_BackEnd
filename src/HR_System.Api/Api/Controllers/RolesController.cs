using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Role;
using HR_System.Application.UseCases.Role;
using HR_System.Api.Filters;
using HR_System.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly RoleUseCase _roleUseCase;

    public RolesController(RoleUseCase roleUseCase)
    {
        _roleUseCase = roleUseCase;
    }

    [HttpGet]
    [RequirePermission(Permissions.RolesView)]
    [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleUseCase.GetAllAsync();
        return Ok(ApiResponse.Success(roles));
        //return Ok(ApiResponse<List<RoleDto>>.Success(roles));
    }
}