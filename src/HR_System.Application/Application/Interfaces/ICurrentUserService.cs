namespace HR_System.Application.Interfaces;

public interface ICurrentUserService
{
    int? GetUserId();
    int? GetEmployeeId();
    string? GetRole();
    int? GetDivisionId();
    int? GetDepartmentId();
    bool HasPermission(string permission);
    List<string> GetRoles();
}