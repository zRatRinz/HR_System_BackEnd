using HR_System.Api.Api.Common;
using HR_System.Application.DTOs.Auth;
using HR_System.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HR_System.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthUseCase _authUseCase;

    public AuthController(AuthUseCase authUseCase)
    {
        _authUseCase = authUseCase;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authUseCase.LoginAsync(request);
        return Ok(ApiResponse<LoginResponse>.Success(response, "Login successful"));
    }

    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        return Ok(ApiResponse.Success(new { success = true }, "Logout successful"));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<RefreshResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var response = await _authUseCase.RefreshAsync(request);
        return Ok(ApiResponse<RefreshResponse>.Success(response, "Token refreshed"));
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token"));
        }

        var user = await _authUseCase.GetCurrentUserAsync(userId);
        return Ok(ApiResponse<UserResponse>.Success(user));
    }
}