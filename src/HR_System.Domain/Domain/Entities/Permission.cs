namespace HR_System.Domain.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
