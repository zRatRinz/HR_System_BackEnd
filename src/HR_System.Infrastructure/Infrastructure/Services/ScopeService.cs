using HR_System.Application.Interfaces;

namespace HR_System.Infrastructure.Services;

public class ScopeService : IScopeService
{
    private readonly ICurrentUserService _currentUserService;

    public ScopeService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public int? GetDivisionId() => _currentUserService.GetDivisionId();
    public int? GetDepartmentId() => _currentUserService.GetDepartmentId();
    public int? GetEmployeeId() => _currentUserService.GetEmployeeId();
    public string GetRole() => _currentUserService.GetRole() ?? "";
    public int GetUserId() => _currentUserService.GetUserId() ?? 0;
    public bool HasPermission(string permission) => _currentUserService.HasPermission(permission);
    public List<string> GetRoles() => _currentUserService.GetRoles();
}