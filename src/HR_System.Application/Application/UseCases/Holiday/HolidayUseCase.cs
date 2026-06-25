using HR_System.Application.DTOs.Holiday;
using HR_System.Application.Interfaces;

namespace HR_System.Application.UseCases.Holiday;

public class HolidayUseCase
{
    private readonly IHolidayRepository _holidayRepository;

    public HolidayUseCase(IHolidayRepository holidayRepository)
    {
        _holidayRepository = holidayRepository;
    }

    public async Task<HolidayListDto> GetByYearAsync(int year)
    {
        var holidays = await _holidayRepository.GetByYearAsync(year);

        return new HolidayListDto
        {
            Holidays = holidays.Select(h => new HolidayDto
            {
                HolidayId = h.HolidayId,
                HolidayName = h.HolidayName,
                HolidayDate = DateOnly.FromDateTime(h.HolidayDate),
                HolidayYear = h.HolidayYear,
                HolidayType = h.HolidayType,
                IsRecurring = h.IsRecurring,
                HolidayDescription = h.HolidayDescription
            }).ToList(),
            Total = holidays.Count
        };
    }

    public async Task<HolidayListDto> GetAllAsync()
    {
        var holidays = await _holidayRepository.GetAllAsync();

        return new HolidayListDto
        {
            Holidays = holidays.Select(h => new HolidayDto
            {
                HolidayId = h.HolidayId,
                HolidayName = h.HolidayName,
                HolidayDate = DateOnly.FromDateTime(h.HolidayDate),
                HolidayYear = h.HolidayYear,
                HolidayType = h.HolidayType,
                IsRecurring = h.IsRecurring,
                HolidayDescription = h.HolidayDescription
            }).ToList(),
            Total = holidays.Count
        };
    }
}