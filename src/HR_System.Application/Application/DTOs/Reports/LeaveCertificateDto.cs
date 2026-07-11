namespace HR_System.Application.DTOs.Reports;

public class ApproverInfo
{
    public int StepNumber { get; set; }
    public string? ApproverName { get; set; }
    public string ApproverRole { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? ActionAt { get; set; }
    public string? Comment { get; set; }
}

public class LeaveCertificateDto
{
    public int LeaveRequestId { get; set; }
    public string EmployeeName { get; set; } = "";
    public string PositionName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string DivisionName { get; set; } = "";
    public string LeaveType { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public List<ApproverInfo> Approvers { get; set; } = new();
}
