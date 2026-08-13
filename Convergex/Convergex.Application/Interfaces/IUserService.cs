using Convergex.Application.DTOs.Users;

namespace Convergex.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, UserDto? User)> CreateAsync(SystemSettingDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> UpdateAsync(UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
}
