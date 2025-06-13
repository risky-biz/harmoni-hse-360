namespace Harmoni360.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    string Email { get; }
    string Name { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}