using Convergex.Application.DTOs.Auth;
using Convergex.Application.Interfaces;
using Convergex.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Convergex.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _authService.LoginAsync(
            new LoginRequestDto
            {
                Email = model.Email,
                Password = model.Password
            },
            cancellationToken);

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Correo o contraseña incorrectos.");

            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.RoleName)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,

            ExpiresUtc = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
            && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction(
            "Index",
            "Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(
            new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result =
            await _authService.RequestPasswordResetAsync(
                model.Email,
                cancellationToken);

        TempData["Info"] = result.Message;

        return RedirectToAction(
            nameof(ResetPassword),
            new
            {
                email = model.Email
            });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(
        string? email = null)
    {
        return View(
            new ResetPasswordViewModel
            {
                Email = email ?? string.Empty
            });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result =
            await _authService.ResetPasswordAsync(
                model.Email,
                model.Token,
                model.NewPassword,
                cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        TempData["Success"] =
            result.Message;

        return RedirectToAction(
            nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var profile =
            await _authService.GetProfileAsync(
                userId,
                cancellationToken);

        if (profile is null)
        {
            return RedirectToAction(
                nameof(Login));
        }

        return View(
            new ProfileViewModel
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
    public async Task<IActionResult> Profile(
        ProfileViewModel model,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        model.Id = userId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var update =
            await _authService.UpdateProfileAsync(
                userId,
                model.FullName,
                model.Email,
                cancellationToken);

        if (!update.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                update.Message);

            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(
                model.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(
                    model.CurrentPassword))
            {
                ModelState.AddModelError(
                    nameof(model.CurrentPassword),
                    "Ingresa tu contraseña actual.");

                return View(model);
            }

            var passwordResult =
                await _authService.ChangePasswordAsync(
                    userId,
                    model.CurrentPassword,
                    model.NewPassword,
                    cancellationToken);

            if (!passwordResult.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    passwordResult.Message);

                return View(model);
            }
        }

        var refreshed =
            await _authService.GetProfileAsync(
                userId,
                cancellationToken);

        if (refreshed is not null)
        {
            var claims =
                new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        refreshed.Id.ToString()),

                    new Claim(
                        ClaimTypes.Name,
                        refreshed.FullName),

                    new Claim(
                        ClaimTypes.Email,
                        refreshed.Email),

                    new Claim(
                        ClaimTypes.Role,
                        refreshed.RoleName)
                };

            var identity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        TempData["Success"] =
            "Perfil actualizado correctamente.";

        return RedirectToAction(
            nameof(Profile));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        TempData["Info"] =
            "El registro público estará disponible próximamente. Solicita acceso a un administrador.";

        return RedirectToAction(
            nameof(Login));
    }

    private int GetUserId()
    {
        return int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? "0");
    }
}