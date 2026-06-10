using System.Security.Claims;
using System.Text.Json;
using HR_System.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HR_System.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    public int? GetEmployeeId()
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue("employee_id");
        return int.TryParse(value, out var id) ? id : null;
    }

    public string? GetRole()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
    }

    public int? GetDivisionId()
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue("division_id");
        return int.TryParse(value, out var id) ? id : null;
    }

    public int? GetDepartmentId()
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue("department_id");
        return int.TryParse(value, out var id) ? id : null;
    }

    public bool HasPermission(string permission)
    {
        var permissionsJson = _httpContextAccessor.HttpContext?.User?.FindFirstValue("permissions");
        if (string.IsNullOrEmpty(permissionsJson)) return false;
        
        try
        {
            var permissions = JsonSerializer.Deserialize<string[]>(permissionsJson);
            return permissions?.Contains(permission) ?? false;
        }
        catch
        {
            return false;
        }
    }

    public List<string> GetRoles()
    {
        var roles = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        return roles ?? new List<string>();
    }
}