namespace HR_System.Domain.Entities;

public class Holiday
{
    public int HolidayId { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public int HolidayYear { get; set; }
    public string HolidayType { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public string? HolidayDescription { get; set; }
    public DateTime CreatedAt { get; set; }
}