using HR_System.Application.DTOs.Employee;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class EmployeeRepository : BaseRepository, IEmployeeRepository
{
    public EmployeeRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        var sql = @"
            INSERT INTO Employees (UserId, FirstName, LastName, DivisionId, DepartmentId, PositionId, HireDate, Salary, Status, Phone, Address, CreatedAt)
            VALUES (@UserId, @FirstName, @LastName, @DivisionId, @DepartmentId, @PositionId, @HireDate, @Salary, @Status, @Phone, @Address, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            UserId = employee.UserId,
            employee.FirstName,
            employee.LastName,
            employee.DivisionId,
            employee.DepartmentId,
            employee.PositionId,
            employee.HireDate,
            employee.Salary,
            Status = employee.Status.ToString(),
            employee.Phone,
            employee.Address,
            CreatedAt = DateTime.UtcNow
        });

        employee.Id = newId;
        return employee;
    }

    public async Task<bool> UpdateAsync(int employeeId,
        string? firstName, string? lastName,
        int? divisionId, int? departmentId, int? positionId,
        string? status,
        DateTime? hireDate, decimal? salary, string? phone, string? address,
        int? userId, string? email)
    {
        var setClauses = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("EmployeeId", employeeId);

        if (firstName != null) { setClauses.Add("FirstName = @FirstName"); parameters.Add("FirstName", firstName); }
        if (lastName != null) { setClauses.Add("LastName = @LastName"); parameters.Add("LastName", lastName); }
        if (divisionId != null) { setClauses.Add("DivisionId = @DivisionId"); parameters.Add("DivisionId", divisionId); }
        if (departmentId != null) { setClauses.Add("DepartmentId = @DepartmentId"); parameters.Add("DepartmentId", departmentId); }
        if (positionId != null) { setClauses.Add("PositionId = @PositionId"); parameters.Add("PositionId", positionId); }
        if (status != null) { setClauses.Add("Status = @Status"); parameters.Add("Status", status); }
        if (hireDate.HasValue) { setClauses.Add("HireDate = @HireDate"); parameters.Add("HireDate", hireDate.Value); }
        if (salary.HasValue) { setClauses.Add("Salary = @Salary"); parameters.Add("Salary", salary.Value); }
        if (phone != null) { setClauses.Add("Phone = @Phone"); parameters.Add("Phone", phone); }
        if (address != null) { setClauses.Add("Address = @Address"); parameters.Add("Address", address); }
        if (userId.HasValue) { setClauses.Add("UserId = @UserId"); parameters.Add("UserId", userId.Value); }

        setClauses.Add("UpdatedAt = @UpdatedAt");
        parameters.Add("UpdatedAt", DateTime.UtcNow);

        var sql = $"UPDATE Employees SET {string.Join(", ", setClauses)} WHERE EmployeeId = @EmployeeId";
        var rowsAffected = await ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        var sql = "SELECT COUNT(*) FROM Employees";
        return await ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetActiveCountAsync()
    {
        var sql = "SELECT COUNT(*) FROM Employees WHERE Status = 'Active'";
        return await ExecuteScalarAsync<int>(sql);
    }

    public async Task<Dictionary<string, int>> GetDepartmentDistributionAsync()
    {
        var sql = "SELECT d.DepartmentName as Department, COUNT(*) as Count FROM Employees e LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId GROUP BY d.DepartmentName";
        var results = await QueryAsync<DepartmentCount>(sql);
        return results.Where(x => !string.IsNullOrEmpty(x.Department)).ToDictionary(x => x.Department, x => x.Count);
    }

    public async Task<EmployeeDto?> GetByIdAsDtoAsync(int id)
    {
        var sql = @"
            SELECT e.EmployeeId as Id, e.UserId,
                   (e.FirstName + ' ' + e.LastName) as Name,
                   e.DivisionId,
                   dv.DivisionName,
                   d.DepartmentName,
                   e.PositionId,
                   p.PositionName,
                   u.Email,
                   e.Status,
                   e.DepartmentId,
                   e.Phone,
                   e.HireDate,
                   e.Address,
                   e.Salary
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN Divisions dv ON e.DivisionId = dv.DivisionId
            LEFT JOIN Positions p ON e.PositionId = p.PositionId
            LEFT JOIN Users u ON e.UserId = u.UserId
            WHERE e.EmployeeId = @EmployeeId";

        return await QuerySingleOrDefaultAsync<EmployeeDto>(sql, new { EmployeeId = id });
    }

    public async Task<EmployeeDto?> GetByUserIdAsDtoAsync(int userId)
    {
        var sql = @"
            SELECT e.EmployeeId as Id, e.UserId,
                   (e.FirstName + ' ' + e.LastName) as Name,
                   e.DivisionId,
                   dv.DivisionName,
                   d.DepartmentName,
                   e.PositionId,
                   p.PositionName,
                   u.Email,
                   e.Status,
                   e.DepartmentId,
                   e.Phone,
                   e.HireDate,
                   e.Address,
                   e.Salary
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN Divisions dv ON e.DivisionId = dv.DivisionId
            LEFT JOIN Positions p ON e.PositionId = p.PositionId
            LEFT JOIN Users u ON e.UserId = u.UserId
            WHERE e.UserId = @UserId";

        return await QuerySingleOrDefaultAsync<EmployeeDto>(sql, new { UserId = userId });
    }

    public async Task<(List<EmployeeListDto> Employees, int Total)> GetAllAsDtoAsync(string? search, int? department, int? division, int? position, string? status, int page, int limit, int? scopeDivisionId = null, int? scopeDepartmentId = null, List<string>? roles = null)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        var bypassScope = roles != null && (roles.Contains("admin", StringComparer.OrdinalIgnoreCase) ||
                                             roles.Contains("hr", StringComparer.OrdinalIgnoreCase) ||
                                             roles.Contains("audit", StringComparer.OrdinalIgnoreCase));

        if (!bypassScope)
        {
            if (roles != null && roles.Contains("headdivision", StringComparer.OrdinalIgnoreCase) && scopeDivisionId.HasValue)
            {
                whereClause += " AND e.DivisionId = @ScopeDivisionId";
                parameters.Add("ScopeDivisionId", scopeDivisionId.Value);
            }
            else if (roles != null && (roles.Contains("headdepartment", StringComparer.OrdinalIgnoreCase) || roles.Contains("manager", StringComparer.OrdinalIgnoreCase)) && scopeDepartmentId.HasValue)
            {
                whereClause += " AND e.DepartmentId = @ScopeDepartmentId";
                parameters.Add("ScopeDepartmentId", scopeDepartmentId.Value);
            }
        }

        if (!string.IsNullOrEmpty(search))
        {
            whereClause += " AND ((e.FirstName + ' ' + e.LastName) LIKE @Search OR u.Email LIKE @Search)";
            parameters.Add("Search", $"%{search}%");
        }

        if (department.HasValue)
        {
            whereClause += " AND e.DepartmentId = @DepartmentId";
            parameters.Add("DepartmentId", department.Value);
        }

        if (division.HasValue)
        {
            whereClause += " AND e.DivisionId = @DivisionId";
            parameters.Add("DivisionId", division.Value);
        }

        if (position.HasValue)
        {
            whereClause += " AND e.PositionId = @PositionId";
            parameters.Add("PositionId", position.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            var statusValue = status.ToLower() switch
            {
                "on-leave" => "OnLeave",
                "inactive" => "Inactive",
                _ => "Active"
            };
            whereClause += " AND e.Status = @Status";
            parameters.Add("Status", statusValue);
        }

        var countSql = $@"
            SELECT COUNT(*)
            FROM Employees e
            LEFT JOIN Users u ON e.UserId = u.UserId
            {whereClause}";

        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var sql = $@"
            SELECT e.EmployeeId as Id,
                   (e.FirstName + ' ' + e.LastName) as Name,
                   e.DivisionId,
                   dv.DivisionName,
                   d.DepartmentName,
                   e.PositionId,
                   p.PositionName,
                   u.Email,
                   e.Status,
                   e.DepartmentId,
                   e.Phone
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN Divisions dv ON e.DivisionId = dv.DivisionId
            LEFT JOIN Positions p ON e.PositionId = p.PositionId
            LEFT JOIN Users u ON e.UserId = u.UserId
            {whereClause}
            ORDER BY e.EmployeeId DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<EmployeeListDto>(sql, parameters);
        return (results.ToList(), total);
    }

    public async Task<(List<EmployeeDto> Employees, int Total)> GetAllAsync(string? search, string? department, string? status, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        var bypassScope = role == "Admin" || role == "HR";

        if (!bypassScope)
        {
            if (role == "HeadDivision" && scopeDivisionId.HasValue)
            {
                whereClause += " AND e.DivisionId = @ScopeDivisionId";
                parameters.Add("ScopeDivisionId", scopeDivisionId.Value);
            }
            else if ((role == "HeadDepartment" || role == "Manager") && scopeDepartmentId.HasValue)
            {
                whereClause += " AND e.DepartmentId = @ScopeDepartmentId";
                parameters.Add("ScopeDepartmentId", scopeDepartmentId.Value);
            }
            else if (role == "Employee" && scopeUserId.HasValue)
            {
                whereClause += " AND e.UserId = @ScopeUserId";
                parameters.Add("ScopeUserId", scopeUserId.Value);
            }
        }

        if (!string.IsNullOrEmpty(search))
        {
            whereClause += " AND ((e.FirstName + ' ' + e.LastName) LIKE @Search OR u.Email LIKE @Search)";
            parameters.Add("Search", $"%{search}%");
        }

        int? deptId = null;
        if (!string.IsNullOrEmpty(department))
        {
            if (int.TryParse(department, out var parsed))
            {
                deptId = parsed;
                whereClause += " AND e.DepartmentId = @DepartmentId";
                parameters.Add("DepartmentId", deptId);
            }
        }

        if (!string.IsNullOrEmpty(status))
        {
            var statusValue = status.ToLower() switch
            {
                "on-leave" => "OnLeave",
                "inactive" => "Inactive",
                _ => "Active"
            };
            whereClause += " AND e.Status = @Status";
            parameters.Add("Status", statusValue);
        }

        var countSql = $@"
            SELECT COUNT(*)
            FROM Employees e
            LEFT JOIN Users u ON e.UserId = u.UserId
            {whereClause}";

        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var sql = $@"
            SELECT e.EmployeeId as Id, e.UserId,
                   (e.FirstName + ' ' + e.LastName) as Name,
                   e.DivisionId,
                   dv.DivisionName,
                   e.DepartmentId,
                   d.DepartmentName,
                   e.PositionId,
                   p.PositionName,
                   u.Email,
                   e.Status,
                   e.Phone,
                   e.HireDate,
                   e.Address,
                   e.Salary
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN Divisions dv ON e.DivisionId = dv.DivisionId
            LEFT JOIN Positions p ON e.PositionId = p.PositionId
            LEFT JOIN Users u ON e.UserId = u.UserId
            {whereClause}
            ORDER BY e.EmployeeId DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<EmployeeDto>(sql, parameters);

        return (results.ToList(), total);
    }

    public async Task<List<EmployeeSearchDto>> SearchAsync(string query)
    {
        var sql = @"
            SELECT TOP 10 e.EmployeeId, (e.FirstName + ' ' + e.LastName) as FullName
            FROM Employees e
            WHERE (e.FirstName + ' ' + e.LastName) LIKE @Query
            ORDER BY e.FirstName, e.LastName";

        return (await QueryAsync<EmployeeSearchDto>(sql, new { Query = $"%{query}%" })).ToList();
    }

    public async Task<List<int>> GetAllIdsAsync(int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId, List<string>? roles = null)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        var bypassScope = roles != null && roles.Contains("HR");

        if (!bypassScope)
        {
            if (role == "HeadDivision" && scopeDivisionId.HasValue)
            {
                whereClause += " AND e.DivisionId = @ScopeDivisionId";
                parameters.Add("ScopeDivisionId", scopeDivisionId.Value);
            }
            else if ((role == "HeadDepartment" || role == "Manager") && scopeDepartmentId.HasValue)
            {
                whereClause += " AND e.DepartmentId = @ScopeDepartmentId";
                parameters.Add("ScopeDepartmentId", scopeDepartmentId.Value);
            }
            else if (role == "Employee" && scopeUserId.HasValue)
            {
                whereClause += " AND e.UserId = @ScopeUserId";
                parameters.Add("ScopeUserId", scopeUserId.Value);
            }
        }

        var sql = $"SELECT e.EmployeeId FROM Employees e {whereClause}";
        var results = await QueryAsync<int>(sql, parameters);
        return results.ToList();
    }

    public async Task<int?> GetHeadOfDepartmentEmployeeIdAsync(int? departmentId)
    {
        if (!departmentId.HasValue)
            return null;

        var sql = @"
            SELECT TOP 1 e.EmployeeId
            FROM Employees e
            INNER JOIN UserRoles ur ON e.UserId = ur.UserId
            INNER JOIN Roles r ON ur.RoleId = r.RoleId
            WHERE e.DepartmentId = @DepartmentId AND r.RoleName = 'HeadDepartment'";

        var result = await QuerySingleOrDefaultAsync<int?>(sql, new { DepartmentId = departmentId.Value });
        return result;
    }

    public async Task<int?> GetHeadOfDivisionEmployeeIdAsync(int? divisionId)
    {
        if (!divisionId.HasValue)
            return null;

        var sql = @"
            SELECT TOP 1 e.EmployeeId
            FROM Employees e
            INNER JOIN UserRoles ur ON e.UserId = ur.UserId
            INNER JOIN Roles r ON ur.RoleId = r.RoleId
            WHERE e.DivisionId = @DivisionId AND r.RoleName = 'HeadDivision'";

        var result = await QuerySingleOrDefaultAsync<int?>(sql, new { DivisionId = divisionId.Value });
        return result;
    }

    public async Task<int?> GetHrEmployeeIdAsync()
    {
        var sql = @"
            SELECT TOP 1 e.EmployeeId
            FROM Employees e
            INNER JOIN UserRoles ur ON e.UserId = ur.UserId
            INNER JOIN Roles r ON ur.RoleId = r.RoleId
            WHERE r.RoleName = 'HR'";

        var result = await QuerySingleOrDefaultAsync<int?>(sql);
        return result;
    }

    public async Task<int?> GetManagerEmployeeIdAsync()
    {
        var sql = @"
            SELECT TOP 1 e.EmployeeId
            FROM Employees e
            INNER JOIN UserRoles ur ON e.UserId = ur.UserId
            INNER JOIN Roles r ON ur.RoleId = r.RoleId
            WHERE r.RoleName = 'Manager'";

        var result = await QuerySingleOrDefaultAsync<int?>(sql);
        return result;
    }

    public async Task<List<int>> GetEmployeeIdsByDivisionAsync(int divisionId)
    {
        var sql = "SELECT EmployeeId FROM Employees WHERE DivisionId = @DivisionId";
        var results = await QueryAsync<int>(sql, new { DivisionId = divisionId });
        return results.ToList();
    }

    public async Task<List<int>> GetEmployeeIdsByDepartmentAsync(int departmentId)
    {
        var sql = "SELECT EmployeeId FROM Employees WHERE DepartmentId = @DepartmentId";
        var results = await QueryAsync<int>(sql, new { DepartmentId = departmentId });
        return results.ToList();
    }

    private class DepartmentCount
    {
        public string Department { get; set; } = "";
        public int Count { get; set; }
    }
}