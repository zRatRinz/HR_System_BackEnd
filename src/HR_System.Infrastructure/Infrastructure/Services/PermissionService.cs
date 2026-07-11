using HR_System.Application.Interfaces;
using HR_System.Domain.Enums;

namespace HR_System.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    public string[] GetPermissionsForRole(UserRole role) => role switch
    {
        UserRole.Admin => new[]
        {
            Permissions.EmployeesView, Permissions.EmployeesCreate, Permissions.EmployeesEdit, Permissions.EmployeesDelete,
            Permissions.LeavesView, Permissions.LeavesViewOverview, Permissions.LeavesCreate, Permissions.LeavesApprove,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview, Permissions.AttendanceEdit, Permissions.AttendanceCheckIn, Permissions.AttendanceCheckOut,
            Permissions.PositionsView, Permissions.PositionsCreate, Permissions.PositionsEdit,
            Permissions.RolesView,
            Permissions.SettingsView, Permissions.SettingsEdit,
            Permissions.ReportsView,
            Permissions.PayrollView,
            Permissions.DivisionsView, Permissions.DepartmentsView,
            Permissions.DashboardView, Permissions.DashboardViewAll,
            Permissions.UsersManage
        },
        UserRole.Manager => new[]
        {
            Permissions.EmployeesView,
            Permissions.LeavesView, Permissions.LeavesCreate, Permissions.LeavesViewOverview, Permissions.LeavesApprove,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview, Permissions.AttendanceCheckIn, Permissions.AttendanceCheckOut,
            Permissions.DashboardView,
            Permissions.PayrollView,
            Permissions.PositionsView, Permissions.RolesView, Permissions.SettingsView
        },
        UserRole.HeadDivision => new[]
        {
            Permissions.EmployeesView, Permissions.EmployeesEdit,
            Permissions.LeavesView, Permissions.LeavesCreate, Permissions.LeavesViewOverview, Permissions.LeavesApprove,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview, Permissions.AttendanceCheckIn, Permissions.AttendanceCheckOut,
            Permissions.DashboardView,
            Permissions.PayrollView,
            Permissions.DivisionsView, Permissions.DepartmentsView,
            Permissions.PositionsView, Permissions.RolesView, Permissions.SettingsView,
            Permissions.ReportsView
        },
        UserRole.HeadDepartment => new[]
        {
            Permissions.EmployeesView, Permissions.EmployeesEdit,
            Permissions.LeavesView, Permissions.LeavesCreate, Permissions.LeavesViewOverview, Permissions.LeavesApprove,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview, Permissions.AttendanceCheckIn, Permissions.AttendanceCheckOut,
            Permissions.DashboardView,
            Permissions.PayrollView,
            Permissions.DivisionsView, Permissions.DepartmentsView,
            Permissions.PositionsView, Permissions.RolesView, Permissions.SettingsView,
            Permissions.ReportsView
        },
        UserRole.HR => new[]
        {
            Permissions.EmployeesView, Permissions.EmployeesCreate, Permissions.EmployeesEdit,
            Permissions.LeavesView, Permissions.LeavesViewOverview,
            Permissions.AttendanceView, Permissions.AttendanceViewOverview,
            Permissions.PayrollView, Permissions.PayrollProcess, Permissions.DashboardView,
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
            Permissions.SettingsView,
            Permissions.PayrollView
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