using HR_System.Application.DTOs.Leave;
using HR_System.Application.DTOs.Reports;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class LeaveRepository : BaseRepository, ILeaveRepository
{
    public LeaveRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<(List<LeaveRequest> Items, int Total)> GetAllAsync(string? status, int? employeeId, int page, int limit)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(status))
        {
            whereClause += " AND l.Status = @Status";
            parameters.Add("Status", status);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND l.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM LeaveRequests l {whereClause}";
        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var dataSql = $@"
            SELECT l.LeaveRequestId, l.EmployeeId, l.LeaveType, l.StartDate, l.EndDate,
                   l.TotalDays, l.Reason, l.Status, l.ApprovedBy, l.CreatedAt,
                h.ActionAt as ApprovedAt, l.CreatedAt, l.UpdatedAt,
                   e.EmployeeId, e.UserId, e.FirstName, e.LastName, e.DivisionId, e.DepartmentId, e.PositionId,
                   e.Status, e.CreatedAt, e.UpdatedAt,
                   u.UserId, u.Email, u.Name, u.Status
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            LEFT JOIN Users u ON e.UserId = u.UserId
            {whereClause}
            ORDER BY l.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<LeaveDto>(dataSql, parameters);
        var items = results.Select(MapToLeaveRequest).ToList();

        return (items, total);
    }

    public async Task<(List<LeaveRequest> Items, int Total)> GetAllAsync(string? status, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, string? role, int? scopeUserId)
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
                whereClause += " AND l.EmployeeId IN (SELECT EmployeeId FROM Employees WHERE UserId = @ScopeUserId)";
                parameters.Add("ScopeUserId", scopeUserId.Value);
            }
        }

        if (!string.IsNullOrEmpty(status))
        {
            whereClause += " AND l.Status = @Status";
            parameters.Add("Status", status);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND l.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM LeaveRequests l INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId {whereClause}";
        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var dataSql = $@"
            SELECT l.LeaveRequestId, l.EmployeeId, l.LeaveType, l.StartDate, l.EndDate,
                   l.TotalDays, l.Reason, l.Status, l.ApprovedBy, l.CreatedAt,
                h.ActionAt as ApprovedAt, l.CreatedAt, l.UpdatedAt,
                   e.EmployeeId, e.UserId, e.FirstName, e.LastName, e.DivisionId, e.DepartmentId, e.PositionId,
                   e.Status, e.CreatedAt, e.UpdatedAt,
                   u.UserId, u.Email, u.Name, u.Status
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            LEFT JOIN Users u ON e.UserId = u.UserId
            {whereClause}
            ORDER BY l.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<LeaveDto>(dataSql, parameters);
        var items = results.Select(MapToLeaveRequest).ToList();

        return (items, total);
    }

    public async Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest)
    {
        var sql = @"
            INSERT INTO LeaveRequests (EmployeeId, LeaveType, StartDate, EndDate, TotalDays, Reason, Status, CreatedAt)
            VALUES (@EmployeeId, @LeaveType, @StartDate, @EndDate, @TotalDays, @Reason, @Status, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var newId = await ExecuteScalarAsync<int>(sql, new
        {
            EmployeeId = leaveRequest.EmployeeId,
            LeaveType = leaveRequest.LeaveType.ToString(),
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            TotalDays = leaveRequest.Days,
            leaveRequest.Reason,
            Status = leaveRequest.Status.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        leaveRequest.Id = newId;
        return leaveRequest;
    }

    public async Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest)
    {
        var sql = @"
            UPDATE LeaveRequests
            SET LeaveType = @LeaveType, StartDate = @StartDate, EndDate = @EndDate,
                TotalDays = @TotalDays, Reason = @Reason, Status = @Status,
                ApprovedBy = @ApprovedBy, ApprovedAt = @ApprovedAt, UpdatedAt = @UpdatedAt
            WHERE LeaveRequestId = @LeaveRequestId";

        await ExecuteAsync(sql, new
        {
            LeaveRequestId = leaveRequest.Id.ToString(),
            LeaveType = leaveRequest.LeaveType.ToString(),
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            TotalDays = leaveRequest.Days,
            leaveRequest.Reason,
            Status = leaveRequest.Status.ToString(),
            ApprovedBy = leaveRequest.ApprovedBy?.ToString(),
            leaveRequest.ApprovedAt,
            UpdatedAt = DateTime.UtcNow
        });

        return leaveRequest;
    }

    public async Task<int> GetPendingCountAsync()
    {
        var sql = "SELECT COUNT(*) FROM LeaveRequests WHERE Status = 'Pending'";
        return await ExecuteScalarAsync<int>(sql);
    }

public async Task<int> GetOnLeaveCountTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var sql = @"
            SELECT COUNT(*) FROM LeaveRequests
            WHERE Status = 'Approved'
            AND StartDate <= @Today AND EndDate >= @Today";

        return await ExecuteScalarAsync<int>(sql, new { Today = today });
    }

    public async Task<int> GetOnLeaveCountTodayWithScopeAsync(int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope)
    {
        var today = DateTime.UtcNow.Date;
        var whereClause = "WHERE l.Status = 'Approved' AND l.StartDate <= @Today AND l.EndDate >= @Today";
        var parameters = new DynamicParameters();
        parameters.Add("Today", today);

        if (!bypassScope)
        {
            if (scopeDepartmentId.HasValue)
            {
                whereClause += " AND e.DepartmentId = @DepartmentId";
                parameters.Add("DepartmentId", scopeDepartmentId.Value);
            }
            else if (scopeDivisionId.HasValue)
            {
                whereClause += " AND e.DivisionId = @DivisionId";
                parameters.Add("DivisionId", scopeDivisionId.Value);
            }
        }

        var sql = $@"
            SELECT COUNT(*) FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            {whereClause}";

        return await ExecuteScalarAsync<int>(sql, parameters);
    }

public async Task<List<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT l.LeaveRequestId, l.EmployeeId, l.LeaveType, l.StartDate, l.EndDate,
                   l.TotalDays, l.Reason, l.Status, l.ApprovedBy, l.CreatedAt,
                h.ActionAt as ApprovedAt, l.CreatedAt, l.UpdatedAt,
                   e.EmployeeId, e.UserId, e.FirstName, e.LastName, e.DivisionId, e.DepartmentId, e.PositionId,
                   e.Status, e.CreatedAt, e.UpdatedAt,
                   u.UserId, u.Email, u.Name, u.Status
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            LEFT JOIN Users u ON e.UserId = u.UserId
            WHERE l.StartDate >= @StartDate AND l.EndDate <= @EndDate
            ORDER BY l.StartDate";

        var results = await QueryAsync<LeaveDto>(sql, new { StartDate = startDate, EndDate = endDate });
        return results.Select(MapToLeaveRequest).ToList();
    }

    public async Task<List<LeaveRequestDto>> GetByDateRangeDtoAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT l.LeaveRequestId as LeaveRequestId, l.EmployeeId, l.LeaveType, 
                   l.StartDate, l.EndDate, l.TotalDays as Days, l.Reason, l.Status,
                   l.CreatedAt, (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            WHERE l.StartDate <= @EndDate AND l.EndDate >= @StartDate
            ORDER BY l.StartDate";

        return (await QueryAsync<LeaveRequestDto>(sql, new { StartDate = startDate, EndDate = endDate })).ToList();
    }

    public async Task<LeaveRequestDto?> GetByIdAsDtoAsync(int id)
    {
        var sql = @"
            SELECT l.LeaveRequestId as LeaveRequestId, l.EmployeeId, l.LeaveType, l.StartDate, l.EndDate,
                   l.TotalDays as Days, l.Reason, l.Status, l.CreatedAt,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            WHERE l.LeaveRequestId = @LeaveRequestId";

        return await QuerySingleOrDefaultAsync<LeaveRequestDto>(sql, new { LeaveRequestId = id });
    }

    public async Task<(List<LeaveRequestDto> Items, int Total)> GetAllAsDtoAsync(string? status, string? leaveType, DateTime? startDate, DateTime? endDate, int? employeeId, int page, int limit)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(status))
        {
            whereClause += " AND l.Status = @Status";
            parameters.Add("Status", status);
        }

        if (!string.IsNullOrEmpty(leaveType))
        {
            whereClause += " AND l.LeaveType = @LeaveType";
            parameters.Add("LeaveType", leaveType);
        }

        if (startDate.HasValue)
        {
            whereClause += " AND l.StartDate >= @StartDate";
            parameters.Add("StartDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            whereClause += " AND l.EndDate <= @EndDate";
            parameters.Add("EndDate", endDate.Value);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND l.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM LeaveRequests l INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId {whereClause}";
        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var sql = $@"
            SELECT l.LeaveRequestId as LeaveRequestId, l.EmployeeId, l.LeaveType, l.StartDate, l.EndDate,
                   l.TotalDays as Days, l.Reason, l.Status, l.CreatedAt,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY l.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<LeaveRequestDto>(sql, parameters);
        return (results.ToList(), total);
    }

    public async Task<(List<LeaveRequestDto> Items, int Total)> GetAllAsDtoAsync(string? status, int? employeeId, int page, int limit, int? scopeDivisionId, int? scopeDepartmentId, bool bypassScope)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (scopeDepartmentId.HasValue)
        {
            whereClause += " AND e.DepartmentId = @ScopeDepartmentId";
            parameters.Add("ScopeDepartmentId", scopeDepartmentId.Value);
        }
        else if (scopeDivisionId.HasValue)
        {
            whereClause += " AND e.DivisionId = @ScopeDivisionId";
            parameters.Add("ScopeDivisionId", scopeDivisionId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            whereClause += " AND l.Status = @Status";
            parameters.Add("Status", status);
        }

        if (employeeId.HasValue)
        {
            whereClause += " AND l.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", employeeId.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM LeaveRequests l INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId {whereClause}";
        var total = await ExecuteScalarAsync<int>(countSql, parameters);

        var sql = $@"
            SELECT l.LeaveRequestId as LeaveRequestId, l.EmployeeId, l.LeaveType, l.StartDate, l.EndDate,
                   l.TotalDays as Days, l.Reason, l.Status, l.CreatedAt,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            {whereClause}
            ORDER BY l.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        parameters.Add("Offset", (page - 1) * limit);
        parameters.Add("Limit", limit);

        var results = await QueryAsync<LeaveRequestDto>(sql, parameters);
        return (results.ToList(), total);
    }

    public async Task<Dictionary<string, int>> GetUsedDaysByTypeAsync(int employeeId)
    {
        var sql = @"
            SELECT LeaveType, SUM(TotalDays) as TotalDays
            FROM LeaveRequests
            WHERE EmployeeId = @EmployeeId AND Status = 'Approved'
            GROUP BY LeaveType";

        var results = await QueryAsync<LeaveTypeSum>(sql, new { EmployeeId = employeeId });
        return results.ToDictionary(r => r.LeaveType, r => r.TotalDays);
    }

    public async Task<Dictionary<string, int>> GetPendingDaysByTypeAsync(int employeeId)
    {
        var sql = @"
            SELECT LeaveType, SUM(TotalDays) as TotalDays
            FROM LeaveRequests
            WHERE EmployeeId = @EmployeeId AND Status = 'Pending'
            GROUP BY LeaveType";

        var results = await QueryAsync<LeaveTypeSum>(sql, new { EmployeeId = employeeId });
        return results.ToDictionary(r => r.LeaveType, r => r.TotalDays);
    }

    public async Task<int> GetPendingRequestsCountAsync(int employeeId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM LeaveRequests
            WHERE EmployeeId = @EmployeeId AND Status = 'Pending'";

        return await ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetLeaveTakenYtdAsync(int employeeId)
    {
        var sql = @"
            SELECT ISNULL(SUM(TotalDays), 0)
            FROM LeaveRequests
            WHERE EmployeeId = @EmployeeId
              AND Status = 'Approved'
              AND YEAR(CreatedAt) = @Year";

        return await ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId, Year = DateTime.UtcNow.Year });
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        var sql = @"
            UPDATE LeaveRequests
            SET Status = @Status, UpdatedAt = @UpdatedAt
            WHERE LeaveRequestId = @LeaveRequestId";

        await ExecuteAsync(sql, new
        {
            LeaveRequestId = id,
            Status = status,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public async Task<int> GetApprovedLeaveTotalDaysAsync(int employeeId, DateTime startDate, DateTime endDate, string leaveType)
    {
        var sql = @"
            SELECT ISNULL(SUM(TotalDays), 0)
            FROM LeaveRequests
            WHERE EmployeeId = @EmployeeId
              AND Status = 'Approved'
              AND LeaveType = @LeaveType
              AND StartDate <= @EndDate
              AND EndDate >= @StartDate";

        return await ExecuteScalarAsync<int>(sql, new
        {
            EmployeeId = employeeId,
            StartDate = startDate,
            EndDate = endDate,
            LeaveType = leaveType
        });
    }

    public async Task<List<LeaveRequestDto>> GetApprovedLeavesByEmployeeIdAsync(int employeeId, DateTime? startDate, DateTime? endDate)
    {
        var whereClause = "WHERE l.EmployeeId = @EmployeeId AND l.Status = 'Approved'";
        var parameters = new DynamicParameters();
        parameters.Add("EmployeeId", employeeId);

        if (startDate.HasValue)
        {
            whereClause += " AND l.StartDate >= @StartDate";
            parameters.Add("StartDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            whereClause += " AND l.EndDate <= @EndDate";
            parameters.Add("EndDate", endDate.Value);
        }

        var sql = $@"
            SELECT l.LeaveRequestId as LeaveRequestId, l.EmployeeId, l.LeaveType,
                   l.StartDate, l.EndDate, l.TotalDays as Days, l.Reason, l.Status, l.CreatedAt,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName,
                   d.DivisionName, de.DepartmentName
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            LEFT JOIN Divisions d ON e.DivisionId = d.DivisionId
            LEFT JOIN Departments de ON e.DepartmentId = de.DepartmentId
            {whereClause}
            ORDER BY l.CreatedAt DESC";

        return (await QueryAsync<LeaveRequestDto>(sql, parameters)).ToList();
    }

    public async Task<List<LeaveRequestDto>> GetApprovedLeavesAsync(DateTime? startDate, DateTime? endDate, bool bypassScope, int? scopeDivisionId, int? scopeDepartmentId, int? scopeUserId)
    {
        var whereClause = "WHERE l.Status = 'Approved'";
        var parameters = new DynamicParameters();

        if (scopeDepartmentId.HasValue)
        {
            whereClause += " AND e.DepartmentId = @DepartmentId";
            parameters.Add("DepartmentId", scopeDepartmentId.Value);
        }
        if (scopeDivisionId.HasValue)
        {
            whereClause += " AND e.DivisionId = @DivisionId";
            parameters.Add("DivisionId", scopeDivisionId.Value);
        }
        else if (!bypassScope && scopeUserId.HasValue)
        {
            whereClause += " AND e.UserId = @UserId";
            parameters.Add("UserId", scopeUserId.Value);
        }

        if (startDate.HasValue)
        {
            whereClause += " AND l.StartDate >= @StartDate";
            parameters.Add("StartDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            whereClause += " AND l.EndDate <= @EndDate";
            parameters.Add("EndDate", endDate.Value);
        }

        var sql = $@"
            SELECT l.LeaveRequestId as LeaveRequestId, l.EmployeeId, l.LeaveType,
                   l.StartDate, l.EndDate, l.TotalDays as Days, l.Reason, l.Status, l.CreatedAt,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName,
                   d.DivisionName, de.DepartmentName
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            LEFT JOIN Divisions d ON e.DivisionId = d.DivisionId
            LEFT JOIN Departments de ON e.DepartmentId = de.DepartmentId
            {whereClause}
            ORDER BY l.CreatedAt DESC";

        return (await QueryAsync<LeaveRequestDto>(sql, parameters)).ToList();
    }

    public async Task<bool> IsLeaveLockedAsync(int leaveRequestId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM PayrollLockedLeaves
            WHERE LeaveRequestId = @LeaveRequestId";
        var count = await ExecuteScalarAsync<int>(sql, new { LeaveRequestId = leaveRequestId });
        return count > 0;
    }

    public async Task LockLeavesAsync(int month, int year, List<int> leaveRequestIds)
    {
        if (!leaveRequestIds.Any()) return;

        var sql = @"
            INSERT INTO PayrollLockedLeaves (PayrollMonth, PayrollYear, LeaveRequestId, LockedAt)
            VALUES (@PayrollMonth, @PayrollYear, @LeaveRequestId, @LockedAt)";

        foreach (var leaveId in leaveRequestIds)
        {
            await ExecuteAsync(sql, new
            {
                PayrollMonth = month,
                PayrollYear = year,
                LeaveRequestId = leaveId,
                LockedAt = DateTime.UtcNow
            });
        }
    }

    public async Task UnlockLeavesAsync(int month, int year)
    {
        var sql = "DELETE FROM PayrollLockedLeaves WHERE PayrollMonth = @Month AND PayrollYear = @Year";
        await ExecuteAsync(sql, new { Month = month, Year = year });
    }

    public async Task<List<int>> GetLockedLeaveIdsAsync(int month, int year)
    {
        var sql = "SELECT LeaveRequestId FROM PayrollLockedLeaves WHERE PayrollMonth = @Month AND PayrollYear = @Year";
        var results = await QueryAsync<int>(sql, new { Month = month, Year = year });
        return results.ToList();
    }

    public async Task<bool> IsPayrollProcessedAsync(int month, int year)
    {
        var sql = "SELECT COUNT(*) FROM PayrollRecords WHERE Month = @Month AND Year = @Year";
        var count = await ExecuteScalarAsync<int>(sql, new { Month = month, Year = year });
        return count > 0;
    }

    public async Task<List<LeaveRequestDto>> GetApprovedLeavesInMonthAsync(int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        var sql = @"
            SELECT l.LeaveRequestId as LeaveRequestId, l.EmployeeId, l.LeaveType,
                   l.StartDate, l.EndDate, l.TotalDays as Days, l.Reason, l.Status, l.CreatedAt,
                   (e.FirstName + ' ' + e.LastName) as EmployeeName
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            WHERE l.Status = 'Approved'
              AND l.StartDate <= @EndDate AND l.EndDate >= @StartDate
            ORDER BY l.StartDate";

        return (await QueryAsync<LeaveRequestDto>(sql, new { StartDate = startDate, EndDate = endDate })).ToList();
    }

    public async Task<LeaveCertificateDto?> GetCertificateByIdAsync(int leaveRequestId)
    {
        var leaveSql = @"
            SELECT
                l.LeaveRequestId,
                (e.FirstName + ' ' + e.LastName) as EmployeeName,
                p.PositionName,
                d.DepartmentName,
                di.DivisionName,
                l.LeaveType,
                l.StartDate,
                l.EndDate,
                l.TotalDays as Days,
                l.Reason,
                l.Status,
                l.CreatedAt
            FROM LeaveRequests l
            INNER JOIN Employees e ON l.EmployeeId = e.EmployeeId
            LEFT JOIN Positions p ON e.PositionId = p.PositionId
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN Divisions di ON e.DivisionId = di.DivisionId
            WHERE l.LeaveRequestId = @LeaveRequestId";

        var leaveDto = await QuerySingleOrDefaultAsync<LeaveCertificateDto>(leaveSql, new { LeaveRequestId = leaveRequestId });
        if (leaveDto == null) return null;

        var approversSql = @"
            SELECT
                h.StepNumber,
                h.ApproverRole,
                h.Status,
                h.ActionAt,
                h.Comment,
                (ae.FirstName + ' ' + ae.LastName) as ApproverName
            FROM LeaveApprovalHistory h
            LEFT JOIN Employees ae ON h.ApproverId = ae.EmployeeId
            WHERE h.LeaveRequestId = @LeaveRequestId
            ORDER BY h.StepNumber";

        var approvers = await QueryAsync<ApproverInfo>(approversSql, new { LeaveRequestId = leaveRequestId });
        leaveDto.Approvers = approvers.ToList();

        return leaveDto;
    }

    private class LeaveTypeSum
    {
        public string LeaveType { get; set; } = "";
        public int TotalDays { get; set; }
    }

    private LeaveRequest MapToLeaveRequest(LeaveDto dto)
    {
        var employee = new Employee
        {
            Id = dto.EmployeeId,
            UserId = dto.UserId ?? 0,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DivisionId = dto.DivisionId,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId,
            Status = Enum.TryParse<EmployeeStatus>(dto.EmpStatus, true, out var empStatus)
                ? empStatus
                : EmployeeStatus.Active,
            CreatedAt = dto.EmpCreatedAt ?? DateTime.UtcNow,
            UpdatedAt = dto.EmpUpdatedAt
        };

        return new LeaveRequest
        {
            Id = dto.LeaveRequestId,
            EmployeeId = dto.EmployeeId,
            LeaveType = Enum.TryParse<LeaveType>(dto.LeaveType, true, out var leaveType)
                ? leaveType
                : LeaveType.Annual,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Days = dto.TotalDays,
            Reason = dto.Reason,
            Status = Enum.TryParse<LeaveStatus>(dto.Status, true, out var leaveStatus)
                ? leaveStatus
                : LeaveStatus.Pending,
            ApprovedBy = dto.ApprovedBy,
            ApprovedAt = dto.ApprovedAt,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    private class LeaveDto
    {
        public int LeaveRequestId { get; set; }
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "";
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int? DivisionId { get; set; }
        public int? DepartmentId { get; set; }
        public int? PositionId { get; set; }
        public string EmpStatus { get; set; } = "";
        public DateTime? EmpCreatedAt { get; set; }
        public DateTime? EmpUpdatedAt { get; set; }
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
    }
}

