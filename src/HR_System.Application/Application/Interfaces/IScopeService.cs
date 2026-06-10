namespace HR_System.Application.Interfaces;

public interface IScopeService
{
    int? GetEmployeeId();
    int? GetDivisionId();
    int? GetDepartmentId();
    string GetRole();
    int GetUserId();
    bool HasPermission(string permission);
    List<string> GetRoles();
}