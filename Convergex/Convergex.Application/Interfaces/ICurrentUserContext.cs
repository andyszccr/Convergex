namespace Convergex.Application.Interfaces;

public interface ICurrentUserContext
{
    int? UserId { get; }
    string? UserName { get; }
    string? IpAddress { get; }
}
