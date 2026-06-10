namespace HR_System.Application.DTOs.Position;

public class UpdatePositionRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}