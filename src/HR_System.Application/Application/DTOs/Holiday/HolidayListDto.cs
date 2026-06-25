namespace HR_System.Application.DTOs.Holiday;

public class HolidayListDto
{
    public List<HolidayDto> Holidays { get; set; } = new();
    public int Total { get; set; }
}