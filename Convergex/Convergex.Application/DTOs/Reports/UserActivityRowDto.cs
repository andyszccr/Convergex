namespace Convergex.Application.DTOs.Reports;

public class UserActivityRowDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int OperationsCount { get; set; }
    public decimal TotalAmount { get; set; }
}
