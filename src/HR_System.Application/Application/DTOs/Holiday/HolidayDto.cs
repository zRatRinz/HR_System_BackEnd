namespace HR_System.Application.DTOs.Holiday;

public class HolidayDto
{
    public int HolidayId { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateOnly HolidayDate { get; set; }
    public int HolidayYear { get; set; }
    public string HolidayType { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public string? HolidayDescription { get; set; }
}