using Convergex.Application.DTOs.Auth;

namespace Convergex.Application.Interfaces;

public interface IAuthService
{
    Task<AuthenticatedUserDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message, AuthenticatedUserDto? User)> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUserDto?> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> UpdateProfileAsync(
        int userId,
        string fullName,
        string email,
        CancellationToken cancellationToken = default);
}