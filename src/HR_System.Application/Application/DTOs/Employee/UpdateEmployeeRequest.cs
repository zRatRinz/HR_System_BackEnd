namespace HR_System.Application.DTOs.Employee;

public class UpdateEmployeeRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int? DivisionId { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public string? Status { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal? Salary { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}