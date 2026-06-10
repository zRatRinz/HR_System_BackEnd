using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Employee;
using HR_System.Application.UseCases.Employee;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeUseCase _employeeUseCase;

    public EmployeesController(EmployeeUseCase employeeUseCase)
    {
        _employeeUseCase = employeeUseCase;
    }

    [HttpGet]
    [RequirePermission("employees.view")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? department,
        [FromQuery] int? division,
        [FromQuery] int? position,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var response = await _employeeUseCase.GetAllAsync(search, department, division, position, status, page, limit);
        return Ok(ApiResponse<EmployeeListResponse>.Success(response));
    }

    [HttpGet("{id}")]
    [RequirePermission("employees.view")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeUseCase.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeDto>.Success(employee));
    }

    [HttpGet("search")]
    [RequirePermission("employees.view")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(ApiResponse<List<EmployeeSearchDto>>.Success(new List<EmployeeSearchDto>()));

        var results = await _employeeUseCase.SearchAsync(q);
        return Ok(ApiResponse<List<EmployeeSearchDto>>.Success(results));
    }

    [HttpPost]
    [RequirePermission("employees.create")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        var employee = await _employeeUseCase.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, ApiResponse<EmployeeDto>.Success(employee, "Employee created"));
    }

    [HttpPatch("{id}")]
    [RequirePermission("employees.edit")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request)
    {
        try
        {
            var employee = await _employeeUseCase.UpdateAsync(id, request);
            return Ok(ApiResponse<EmployeeDto>.Success(employee, "Employee updated"));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No fields"))
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Employee not found"));
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("employees.delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _employeeUseCase.DeleteAsync(id);
        return Ok(ApiResponse.Success(null, "Employee deleted"));
    }
}