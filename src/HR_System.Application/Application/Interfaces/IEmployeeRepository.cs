using HR_System.Application.DTOs.Employee;
using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee> CreateAsync(Employee employee);
    Task<bool> UpdateAsync(int employeeId,
        string? firstName, string? lastName,
        int? divisionId, int? departmentId, int? positionId,
        string? status,
        DateTime? hireDate, decimal? salary, string? phone, string? address,
        int? userId, string? email);
    Task<int> GetTotalCountAsync();
    Task<int> GetActiveCountAsync();
    Task<Dictionary<string, int>> GetDepartmentDistributionAsync();

    Task<EmployeeDto?> GetByIdAsDtoAsync(int id);
    Task<EmployeeDto?> GetByUserIdAsDtoAsync(int userId);
    Task<(List<EmployeeListDto> Employees, int Total)> GetAllAsDtoAsync(string? search, int? department, int? division, int? position, string? status, int page, int limit, int? scopeDivisionId = null, int? scopeDepartmentId = null, List<string>? roles = null);
    Task<(List<EmployeeDto> Employees, int Total)> GetAllAsync(string? search, string? department, string? status, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId);
    Task<List<int>> GetAllIdsAsync(int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId, List<string>? roles = null);
    Task<List<EmployeeSearchDto>> SearchAsync(string query);
    Task<int?> GetHeadOfDepartmentEmployeeIdAsync(int? departmentId);
    Task<int?> GetHeadOfDivisionEmployeeIdAsync(int? divisionId);
    Task<int?> GetHrEmployeeIdAsync();
    Task<int?> GetManagerEmployeeIdAsync();
    Task<List<int>> GetEmployeeIdsByDivisionAsync(int divisionId);
    Task<List<int>> GetEmployeeIdsByDepartmentAsync(int departmentId);
}
