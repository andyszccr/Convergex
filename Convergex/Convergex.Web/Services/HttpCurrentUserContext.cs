using System.Security.Claims;
using Convergex.Application.Interfaces;

namespace Convergex.Web.Services;

public class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true
            ? _httpContextAccessor.HttpContext.User.Identity!.Name
            : "Sistema";

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
