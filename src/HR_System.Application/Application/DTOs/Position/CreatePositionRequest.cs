namespace HR_System.Application.DTOs.Position;

public class CreatePositionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}