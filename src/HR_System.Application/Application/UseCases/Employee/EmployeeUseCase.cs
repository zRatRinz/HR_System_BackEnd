using HR_System.Application.DTOs.Employee;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Employee;

public class EmployeeUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IScopeService _scopeService;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDivisionRepository _divisionRepository;

    public EmployeeUseCase(
        IEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        IPositionRepository positionRepository,
        IPasswordHasher passwordHasher,
        IScopeService scopeService,
        IDepartmentRepository departmentRepository,
        IDivisionRepository divisionRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _positionRepository = positionRepository;
        _passwordHasher = passwordHasher;
        _scopeService = scopeService;
        _departmentRepository = departmentRepository;
        _divisionRepository = divisionRepository;
    }

    public async Task<EmployeeListResponse> GetAllAsync(string? search, int? department, int? division, int? position, string? status, int page, int limit)
    {
        var roles = _scopeService.GetRoles();
        var divisionId = _scopeService.GetDivisionId();
        var departmentId = _scopeService.GetDepartmentId();

        var (employees, total) = await _employeeRepository.GetAllAsDtoAsync(search, department, division, position, status, page, limit, divisionId, departmentId, roles);

        foreach (var emp in employees)
        {
            emp.Initials = GetInitials(emp.Name);
            emp.AvatarColor = GenerateAvatarColor();
        }

        return new EmployeeListResponse
        {
            Employees = employees,
            Total = total,
            Page = page
        };
    }

    public async Task<EmployeeDto> GetByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsDtoAsync(id);
        if (employee == null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        employee.AvatarColor = GenerateAvatarColor();
        return employee;
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
    {
        var roles = _scopeService.GetRoles();
        if (!roles.Contains("admin", StringComparer.OrdinalIgnoreCase) && !roles.Contains("hr", StringComparer.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Access denied. Only admin or HR can create employees.");
        }

        // var userRole = (UserRole)request.RoleId;
        // TODO: Frontend ต้องส่ง email, password มาเอง
        // var email = $"{request.FirstName}.{request.LastName}@company.com".ToLower();
        // var password = Guid.NewGuid().ToString("N")[..12];

        // var user = new User
        // {
        //     Id = 0,
        //     Email = email,
        //     PasswordHash = _passwordHasher.Hash(password),
        //     Name = $"{request.FirstName} {request.LastName}",
        //     Role = userRole,
        //     Status = "Active",
        //     CreatedAt = DateTime.UtcNow
        // };

        // await _userRepository.CreateAsync(user);

        var employee = new Domain.Entities.Employee
        {
            Id = 0,
            UserId = null, // รอ frontend ส่ง email/password มาสร้าง user
            FirstName = request.FirstName,
            LastName = request.LastName,
            DivisionId = request.DivisionId,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            HireDate = request.HireDate,
            Salary = request.Salary ?? 0,
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _employeeRepository.CreateAsync(employee);

        string positionName = "";
        string departmentName = "";
        string divisionName = "";

        if (request.PositionId.HasValue)
        {
            var position = await _positionRepository.GetByIdAsync(request.PositionId.Value);
            positionName = position?.PositionName ?? "";
        }

        if (request.DepartmentId.HasValue)
        {
            var dept = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value);
            departmentName = dept?.DepartmentName ?? "";
            if (dept?.DivisionId != null)
            {
                var div = await _divisionRepository.GetByIdAsync(dept.DivisionId);
                divisionName = div?.DivisionName ?? "";
            }
        }

        return new EmployeeDto
        {
            Id = employee.Id,
            Name = $"{request.FirstName} {request.LastName}",
            Email = null, // รอ frontend ส่ง email มา
            Phone = request.Phone,
            Address = request.Address,
            Initials = GetInitials($"{request.FirstName} {request.LastName}"),
            DivisionId = request.DivisionId,
            DivisionName = divisionName,
            DepartmentId = request.DepartmentId,
            DepartmentName = departmentName,
            PositionId = request.PositionId,
            PositionName = positionName,
            HireDate = request.HireDate,
            Salary = request.Salary ?? 0,
            Status = "Active",
            AvatarColor = GenerateAvatarColor(),
            RoleIds = request.RoleIds
        };
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeRequest request)
    {
        var roles = _scopeService.GetRoles();

        var isAdminOrHR = roles.Any(r => r.Equals("admin", StringComparison.OrdinalIgnoreCase) || r.Equals("hr", StringComparison.OrdinalIgnoreCase));

        if (!isAdminOrHR)
        {
            throw new UnauthorizedAccessException("Access denied. Only admin or HR can update employees.");
        }

        var hasAnyFieldToUpdate = request.Name != null || request.Email != null ||
            request.DivisionId != null || request.DepartmentId != null ||
            request.PositionId != null || request.Status != null ||
            request.HireDate.HasValue || request.Salary.HasValue ||
            request.Phone != null || request.Address != null;

        if (!hasAnyFieldToUpdate)
        {
            throw new InvalidOperationException("No fields to update. Provide at least one field.");
        }

        var employeeDto = await _employeeRepository.GetByIdAsDtoAsync(id);
        if (employeeDto == null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        string? firstName = null;
        string? lastName = null;
        int? divisionId = null;
        int? departmentId = null;
        int? positionId = null;
        string? status = null;
        DateTime? hireDate = null;
        decimal? salary = null;
        string? phone = null;
        string? address = null;

        if (request.Name != null)
        {
            var nameParts = request.Name.Split(' ', 2);
            firstName = nameParts[0];
            lastName = nameParts.Length > 1 ? nameParts[1] : null;
        }

        if (request.DivisionId != null) divisionId = request.DivisionId;
        if (request.DepartmentId != null) departmentId = request.DepartmentId;
        if (request.PositionId != null) positionId = request.PositionId;
        if (request.Status != null) status = request.Status;
        if (request.HireDate.HasValue) hireDate = request.HireDate;
        if (request.Salary.HasValue) salary = request.Salary;
        if (request.Phone != null) phone = request.Phone;
        if (request.Address != null) address = request.Address;

        var success = await _employeeRepository.UpdateAsync(id, firstName, lastName, divisionId, departmentId, positionId, status, hireDate, salary, phone, address, null, request.Email);
        if (!success)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        if (request.Email != null)
        {
            var userDto = await _userRepository.GetByIdAsDtoAsync(employeeDto.UserId ?? 0);
            if (userDto != null)
            {
                userDto.Email = request.Email;
                userDto.UpdatedAt = DateTime.UtcNow;
                var userRoles = userDto.Roles.Select(r => Enum.Parse<UserRole>(r, true)).ToList();
                await _userRepository.UpdateAsync(new User
                {
                    Id = userDto.Id,
                    Email = userDto.Email,
                    Name = userDto.Name,
                    PasswordHash = userDto.PasswordHash,
                    Roles = userRoles,
                    Status = "Active",
                    UpdatedAt = userDto.UpdatedAt
                });
            }
        }

return await GetByIdAsync(id);
    }

    public async Task<List<EmployeeSearchDto>> SearchAsync(string query)
    {
        return await _employeeRepository.SearchAsync(query);
    }

    public async Task DeleteAsync(int id)
    {
        var roles = _scopeService.GetRoles();
        if (!roles.Any(r => r.Equals("admin", StringComparison.OrdinalIgnoreCase) || r.Equals("hr", StringComparison.OrdinalIgnoreCase)))
        {
            throw new UnauthorizedAccessException("Access denied. Only admin or HR can delete employees.");
        }

        var employeeDto = await _employeeRepository.GetByIdAsDtoAsync(id);
        if (employeeDto == null)
        {
            throw new KeyNotFoundException("Employee not found");
        }

        await _employeeRepository.UpdateAsync(id, null, null, null, null, null, "Inactive", null, null, null, null, null, null);
    }

    private static string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        }
        return name.Length >= 2 ? name[..2].ToUpper() : name.ToUpper();
    }

    private static string GenerateAvatarColor()
    {
        var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFEAA7", "#DDA0DD", "#98D8C8", "#F7DC6F" };
        return colors[new Random().Next(colors.Length)];
    }
}