using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Department;
using HR_System.Application.UseCases.Department;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentUseCase _departmentUseCase;

    public DepartmentsController(DepartmentUseCase departmentUseCase)
    {
        _departmentUseCase = departmentUseCase;
    }

    [HttpGet]
    [RequirePermission("departments.view")]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? divisionId)
    {
        var departments = await _departmentUseCase.GetAllAsync(divisionId);
        return Ok(ApiResponse<List<DepartmentDto>>.Success(departments));
    }

    [HttpGet("{id}")]
    [RequirePermission("departments.view")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _departmentUseCase.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound(ApiResponse.Fail("Department not found"));
        }
        return Ok(ApiResponse<DepartmentDto>.Success(department));
    }
}