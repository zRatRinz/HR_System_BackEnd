namespace HR_System.Domain.Enums;

public static class Permissions
{
    public const string EmployeesView = "employees.view";
    public const string EmployeesCreate = "employees.create";
    public const string EmployeesEdit = "employees.edit";
    public const string EmployeesDelete = "employees.delete";

    public const string LeavesView = "leaves.view";
    public const string LeavesCreate = "leaves.create";
    public const string LeavesApprove = "leaves.approve";

    public const string AttendanceView = "attendance.view";
    public const string AttendanceEdit = "attendance.edit";
    public const string AttendanceCheckIn = "attendance.checkin";
    public const string AttendanceCheckOut = "attendance.checkout";

    public const string PositionsView = "positions.view";
    public const string PositionsCreate = "positions.create";
    public const string PositionsEdit = "positions.edit";

    public const string RolesView = "roles.view";

    public const string SettingsView = "settings.view";
    public const string SettingsEdit = "settings.edit";

    public const string ReportsView = "reports.view";

    public const string PayrollView = "payroll.view";
    public const string PayrollProcess = "payroll.process";

    public const string DivisionsView = "divisions.view";
    public const string DepartmentsView = "departments.view";

    public const string DashboardView = "dashboard.view";
    public const string DashboardViewAll = "dashboard.view_all";

    public const string UsersManage = "users.manage";

    public static readonly string[] All =
    [
        EmployeesView, EmployeesCreate, EmployeesEdit, EmployeesDelete,
        LeavesView, LeavesCreate, LeavesApprove,
        AttendanceView, AttendanceEdit, AttendanceCheckIn, AttendanceCheckOut,
        PositionsView, PositionsCreate, PositionsEdit,
        RolesView,
        SettingsView, SettingsEdit,
        ReportsView,
        PayrollView, PayrollProcess,
        DivisionsView, DepartmentsView,
        DashboardView, DashboardViewAll,
        UsersManage
    ];
}