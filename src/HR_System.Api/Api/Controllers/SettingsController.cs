using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Settings;
using HR_System.Application.UseCases.Settings;
using HR_System.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly SettingsUseCase _settingsUseCase;

    public SettingsController(SettingsUseCase settingsUseCase)
    {
        _settingsUseCase = settingsUseCase;
    }

    [HttpGet]
    [RequirePermission("settings.view")]
    [ProducesResponseType(typeof(ApiResponse<SettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get()
    {
        var settings = await _settingsUseCase.GetAsync();
        return Ok(ApiResponse<SettingsDto>.Success(settings));
    }

    [HttpPut]
    [RequirePermission("settings.edit")]
    [ProducesResponseType(typeof(ApiResponse<SettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromBody] SettingsDto settings)
    {
        var updated = await _settingsUseCase.UpdateAsync(settings);
        return Ok(ApiResponse<SettingsDto>.Success(updated, "Settings updated"));
    }
}
