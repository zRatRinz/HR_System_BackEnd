using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Holiday;
using HR_System.Application.UseCases.Holiday;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/holidays")]
public class HolidaysController : ControllerBase
{
    private readonly HolidayUseCase _holidayUseCase;

    public HolidaysController(HolidayUseCase holidayUseCase)
    {
        _holidayUseCase = holidayUseCase;
    }

    [HttpGet]
    [RequirePermission("leaves.view")]
    [ProducesResponseType(typeof(ApiResponse<HolidayListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? year)
    {
        HolidayListDto result;
        if (year.HasValue)
        {
            result = await _holidayUseCase.GetByYearAsync(year.Value);
        }
        else
        {
            result = await _holidayUseCase.GetAllAsync();
        }
        return Ok(ApiResponse<HolidayListDto>.Success(result));
    }
}