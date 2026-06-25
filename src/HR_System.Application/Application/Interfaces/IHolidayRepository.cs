using HR_System.Domain.Entities;

namespace HR_System.Application.Interfaces;

public interface IHolidayRepository
{
    Task<List<Holiday>> GetByYearAsync(int year);
    Task<List<Holiday>> GetAllAsync();
}