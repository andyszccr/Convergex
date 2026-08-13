using Convergex.Application.DTOs.Users;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Convergex.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<(bool Success, string Message, UserDto? User)> CreateAsync(
        SystemSettingDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return (false, "Nombre, correo y contraseña son obligatorios.", null);
        }

        if (dto.Password.Length < 6)
        {
            return (false, "La contraseña debe tener al menos 6 caracteres.", null);
        }

        if (await _userRepository.EmailExistsAsync(dto.Email.Trim(), null, cancellationToken))
        {
            return (false, "Ya existe un usuario con ese correo.", null);
        }

        var role = await _userRepository.GetRoleByIdAsync(dto.RoleId, cancellationToken);
        if (role is null)
        {
            return (false, "El rol seleccionado no existe.", null);
        }

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            RoleId = dto.RoleId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var created = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
        return (true, "Usuario creado correctamente.", created is null ? null : Map(created));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(
        UpdateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        if (await _userRepository.EmailExistsAsync(dto.Email.Trim(), dto.Id, cancellationToken))
        {
            return (false, "Ya existe un usuario con ese correo.");
        }

        var role = await _userRepository.GetRoleByIdAsync(dto.RoleId, cancellationToken);
        if (role is null)
        {
            return (false, "El rol seleccionado no existe.");
        }

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLowerInvariant();
        user.RoleId = dto.RoleId;
        user.IsActive = dto.IsActive;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (dto.NewPassword.Length < 6)
            {
                return (false, "La nueva contraseña debe tener al menos 6 caracteres.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return (true, "Usuario actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        if (string.Equals(user.Role.Name, "Administrador", StringComparison.OrdinalIgnoreCase)
            && (await _userRepository.GetAllAsync(cancellationToken))
                .Count(u => u.Role.Name == "Administrador" && u.IsActive) <= 1)
        {
            return (false, "No se puede eliminar el único administrador activo.");
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return (true, "Usuario eliminado correctamente.");
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _userRepository.GetRolesAsync(cancellationToken);
        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description
        }).ToList();
    }

    private static UserDto Map(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        RoleId = user.RoleId,
        RoleName = user.Role.Name,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt
    };
}
