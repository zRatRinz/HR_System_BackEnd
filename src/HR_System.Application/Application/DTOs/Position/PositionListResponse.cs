namespace HR_System.Application.DTOs.Position;

public class PositionListResponse
{
    public List<PositionDto> Data { get; set; } = new();
    public int Total { get; set; }
}