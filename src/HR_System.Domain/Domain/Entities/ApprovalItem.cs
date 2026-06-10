using HR_System.Domain.Enums;

namespace HR_System.Domain.Entities;

public class ApprovalItem
{
    public Guid Id { get; set; }
    public int EmployeeId { get; set; }
    public ApprovalType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}