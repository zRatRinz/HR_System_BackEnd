using HR_System.Application.Interfaces;
using HR_System.Domain.Enums;

namespace HR_System.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    public string[] GetPermissionsForRole(UserRole role) => role switch
    {
        UserRole.Admin => Permissions.All,
        UserRole.Manager => new[]
        {
            Permissions.EmployeesView,
            Permissions.LeavesView, Permissions.LeavesCreate, Permissions.LeavesViewOverview, Permissions.LeavesApprove,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview, Permissions.AttendanceCheckIn, Permissions.AttendanceCheckOut,
            Permissions.DashboardView,
            Permissions.PositionsView, Permissions.RolesView, Permissions.SettingsView
        },
        UserRole.HeadDivision => new[]
        {
            Permissions.EmployeesView, Permissions.EmployeesEdit,
            Permissions.LeavesView, Permissions.LeavesCreate, Permissions.LeavesViewOverview, Permissions.LeavesApprove,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview, Permissions.AttendanceCheckIn, Permissions.AttendanceCheckOut,
            Permissions.DashboardView,
            Permissions.DivisionsView, Permissions.DepartmentsView,
            Permissions.PositionsView, Permissions.RolesView, Permissions.SettingsView
        },
        UserRole.HeadDepartment => new[]
        {
            Permissions.EmployeesView, Permissions.EmployeesEdit,
            Permissions.LeavesView, Permissions.LeavesCreate, Permissions.LeavesViewOverview, Permissions.LeavesApprove,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview, Permissions.AttendanceCheckIn, Permissions.AttendanceCheckOut,
            Permissions.DashboardView,
            Permissions.DivisionsView, Permissions.DepartmentsView,
            Permissions.PositionsView, Permissions.RolesView, Permissions.SettingsView
        },
        UserRole.HR => new[]
        {
            Permissions.EmployeesView, Permissions.EmployeesCreate, Permissions.EmployeesEdit,
            Permissions.LeavesView, Permissions.LeavesViewOverview,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview,
            Permissions.PayrollView, Permissions.DashboardView,
            Permissions.PositionsView, Permissions.PositionsCreate, Permissions.PositionsEdit,
            Permissions.RolesView, Permissions.SettingsView,
            Permissions.DivisionsView, Permissions.DepartmentsView
        },
        UserRole.Audit => new[]
        {
            Permissions.PayrollView, Permissions.DashboardViewAll,
            Permissions.ReportsView, Permissions.SettingsView
        },
        UserRole.Employee => new[]
        {
            Permissions.LeavesView, Permissions.LeavesCreate,
            Permissions.AttendanceView, Permissions.AttendanceCheckIn, Permissions.AttendanceCheckOut,
            Permissions.DashboardView,
            Permissions.SettingsView
        },
        _ => Array.Empty<string>()
    };

    public string[] GetPermissionsForRoles(IEnumerable<UserRole> roles)
    {
        var permissions = roles.SelectMany(r => GetPermissionsForRole(r));
        return permissions.Distinct().ToArray();
    }

    public bool HasPermission(string[] userPermissions, string requiredPermission)
    {
        return userPermissions.Contains(requiredPermission);
    }
}