namespace HR_System.Domain.Entities;

public class Position
{
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}