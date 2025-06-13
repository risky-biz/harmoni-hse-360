using MediatR;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record RemoveRoleCommand : IRequest<bool>
{
    public int UserId { get; init; }
    public int RoleId { get; init; }
}