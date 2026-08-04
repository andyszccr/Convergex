using Convergex.Application.DTOs.Auth;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Convergex.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthenticatedUserDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    public async Task<(bool Success, string Message)> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email.Trim(), cancellationToken);
        if (user is null || !user.IsActive)
        {
            return (true, "Si el correo existe, enviaremos instrucciones para restablecer la contraseña.");
        }

        user.ResetToken = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        user.ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return (true, $"Token de recuperación generado (demo): {user.ResetToken}. Úsalo para restablecer tu contraseña.");
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email.Trim(), cancellationToken);
        if (user is null
            || string.IsNullOrWhiteSpace(user.ResetToken)
            || !string.Equals(user.ResetToken, token.Trim(), StringComparison.OrdinalIgnoreCase)
            || user.ResetTokenExpiresAt is null
            || user.ResetTokenExpiresAt < DateTime.UtcNow)
        {
            return (false, "El token de recuperación no es válido o ha expirado.");
        }

        if (newPassword.Length < 6)
        {
            return (false, "La nueva contraseña debe tener al menos 6 caracteres.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiresAt = null;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return (true, "Contraseña actualizada correctamente. Ya puedes iniciar sesión.");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return (false, "La contraseña actual es incorrecta.");
        }

        if (newPassword.Length < 6)
        {
            return (false, "La nueva contraseña debe tener al menos 6 caracteres.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return (true, "Contraseña actualizada correctamente.");
    }

    public async Task<AuthenticatedUserDto?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(
        int userId,
        string fullName,
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        if (await _userRepository.EmailExistsAsync(email.Trim(), userId, cancellationToken))
        {
            return (false, "El correo electrónico ya está en uso.");
        }

        user.FullName = fullName.Trim();
        user.Email = email.Trim().ToLowerInvariant();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return (true, "Perfil actualizado correctamente.");
    }

    private static AuthenticatedUserDto Map(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        RoleName = user.Role.Name
    };
}
