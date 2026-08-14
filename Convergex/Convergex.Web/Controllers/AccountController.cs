using Convergex.Application.DTOs.Audit;
using Convergex.Application.DTOs.Auth;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Convergex.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public AccountController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = model.Email,
            Password = model.Password
        }, cancellationToken);

        if (user is null)
        {
            await _auditService.LogAsync(new AuditLogEntryDto
            {
                Action = AuditAction.LoginFailed,
                EntityName = "Auth",
                Detail = $"Intento de inicio de sesión fallido: {model.Email}",
                Status = AuditStatus.Failed,
                UserName = model.Email
            }, cancellationToken);

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            Action = AuditAction.Login,
            EntityName = "Auth",
            EntityId = user.Id.ToString(),
            Detail = $"Inicio de sesión exitoso: {user.Email}",
            Status = AuditStatus.Success,
            UserId = user.Id,
            UserName = user.FullName
        }, cancellationToken);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userName = User.Identity?.Name ?? "Usuario";

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            Action = AuditAction.Logout,
            EntityName = "Auth",
            EntityId = userId.ToString(),
            Detail = $"Cierre de sesión: {userName}",
            Status = AuditStatus.Success,
            UserId = userId,
            UserName = userName
        }, cancellationToken);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RequestPasswordResetAsync(model.Email, cancellationToken);
        TempData["Info"] = result.Message;
        return RedirectToAction(nameof(ResetPassword), new { email = model.Email });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email = null)
        => View(new ResetPasswordViewModel { Email = email ?? string.Empty });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _authService.GetProfileAsync(userId, cancellationToken);
        if (profile is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ProfileViewModel
        {
            Id = profile.Id,
            FullName = profile.FullName,
            Email = profile.Email,
            RoleName = profile.RoleName
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        model.Id = userId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var update = await _authService.UpdateProfileAsync(userId, model.FullName, model.Email, cancellationToken);
        if (!update.Success)
        {
            ModelState.AddModelError(string.Empty, update.Message);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Ingresa tu contraseña actual.");
                return View(model);
            }

            var passwordResult = await _authService.ChangePasswordAsync(
                userId,
                model.CurrentPassword,
                model.NewPassword,
                cancellationToken);

            if (!passwordResult.Success)
            {
                ModelState.AddModelError(string.Empty, passwordResult.Message);
                return View(model);
            }
        }

        var refreshed = await _authService.GetProfileAsync(userId, cancellationToken);
        if (refreshed is not null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, refreshed.Id.ToString()),
                new(ClaimTypes.Name, refreshed.FullName),
                new(ClaimTypes.Email, refreshed.Email),
                new(ClaimTypes.Role, refreshed.RoleName)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        TempData["Success"] = "Perfil actualizado correctamente.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        TempData["Info"] = "El registro público estará disponible próximamente. Solicita acceso a un administrador.";
        return RedirectToAction(nameof(Login));
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
}
