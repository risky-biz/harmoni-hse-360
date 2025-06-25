using MediatR;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record ResetPasswordCommand : IRequest<bool>
{
    public int UserId { get; init; }
    public string NewPassword { get; init; } = string.Empty;
    public bool RequirePasswordChange { get; init; } = true;
}