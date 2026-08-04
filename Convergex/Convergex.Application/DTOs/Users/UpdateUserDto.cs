namespace Convergex.Application.DTOs.Users;

public class UpdateUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
    public string? NewPassword { get; set; }
}
