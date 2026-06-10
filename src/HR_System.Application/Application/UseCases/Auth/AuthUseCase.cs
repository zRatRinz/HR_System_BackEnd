using HR_System.Application.DTOs.Auth;
using HR_System.Application.Interfaces;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Auth;

public class AuthUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPermissionService _permissionService;

    public AuthUseCase(
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IPermissionService permissionService)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _permissionService = permissionService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsDtoAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var passwordValid = await VerifyPasswordFromUser(request.Password, user.Email);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var divisionId = user.DivisionId;
        var departmentId = user.DepartmentId;
        var roles = user.Roles.Select(r => Enum.Parse<UserRole>(r, true)).ToList();
        var permissions = _permissionService.GetPermissionsForRoles(roles);

        var employee = await _employeeRepository.GetByUserIdAsDtoAsync(user.Id);
        var employeeId = employee?.Id;

        var accessToken = _jwtService.GenerateAccessToken(
            user.Id, user.Email, user.Roles,
            divisionId, departmentId, employeeId, permissions);

        var refreshToken = _jwtService.GenerateRefreshToken();

        return new LoginResponse
        {
            User = MapToUserResponse(user),
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private async Task<bool> VerifyPasswordFromUser(string password, string email)
    {
        var userDto = await _userRepository.GetByEmailAsDtoAsync(email);
        return userDto != null && _passwordHasher.Verify(password, userDto.PasswordHash);
    }

    public async Task<RefreshResponse> RefreshAsync(RefreshRequest request)
    {
        var payload = _jwtService.GetTokenPayload(request.RefreshToken);
        if (payload == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = await _userRepository.GetByIdAsDtoAsync(payload.Value.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var divisionId = user.DivisionId;
        var departmentId = user.DepartmentId;
        var roles = user.Roles.Select(r => Enum.Parse<UserRole>(r, true)).ToList();
        var permissions = _permissionService.GetPermissionsForRoles(roles);

        var employee = await _employeeRepository.GetByUserIdAsDtoAsync(user.Id);
        var employeeId = employee?.Id;

        var accessToken = _jwtService.GenerateAccessToken(
            user.Id, user.Email, user.Roles,
            divisionId, departmentId, employeeId, permissions);

        var refreshToken = _jwtService.GenerateRefreshToken();

        return new RefreshResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<UserResponse> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsDtoAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return MapToUserResponse(user);
    }

    private static UserResponse MapToUserResponse(UserDto user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Roles = user.Roles.Select(r => r.ToLower()).ToList(),
            PositionName = user.Position,
            DivisionName = user.Division,
            DepartmentName = user.Department
        };
    }
}