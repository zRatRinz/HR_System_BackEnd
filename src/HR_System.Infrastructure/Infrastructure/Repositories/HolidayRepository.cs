using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace HR_System.Infrastructure.Repositories;

public class HolidayRepository : BaseRepository, IHolidayRepository
{
    public HolidayRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<List<Holiday>> GetByYearAsync(int year)
    {
        var sql = @"
            SELECT HolidayId, HolidayName, HolidayDate, HolidayYear, HolidayType,
                   IsRecurring, HolidayDescription, CreatedAt
            FROM Holidays
            WHERE HolidayYear = @Year OR (IsRecurring = 1 AND MONTH(HolidayDate) = MONTH(DATEFROMPARTS(@Year, 1, 1)))
            ORDER BY HolidayDate";

        var results = await QueryAsync<Holiday>(sql, new { Year = year });
        return results.ToList();
    }

    public async Task<List<Holiday>> GetAllAsync()
    {
        var sql = @"
            SELECT HolidayId, HolidayName, HolidayDate, HolidayYear, HolidayType,
                   IsRecurring, HolidayDescription, CreatedAt
            FROM Holidays
            ORDER BY HolidayDate";

        var results = await QueryAsync<Holiday>(sql);
        return results.ToList();
    }

    public async Task<int> CountHolidaysInRangeAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM Holidays
            WHERE (HolidayDate BETWEEN @StartDate AND @EndDate)
               OR (IsRecurring = 1 
                   AND MONTH(HolidayDate) BETWEEN MONTH(@StartDate) AND MONTH(@EndDate)
                   AND DAY(HolidayDate) BETWEEN DAY(@StartDate) AND DAY(@EndDate))";

        return await ExecuteScalarAsync<int>(sql, new { StartDate = startDate, EndDate = endDate });
    }
}