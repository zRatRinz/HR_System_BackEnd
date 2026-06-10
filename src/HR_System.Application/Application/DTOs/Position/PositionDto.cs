namespace HR_System.Application.DTOs.Position;

public class PositionDto
{
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
}