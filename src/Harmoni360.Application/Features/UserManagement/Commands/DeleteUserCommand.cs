using MediatR;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record DeleteUserCommand : IRequest<bool>
{
    public int UserId { get; init; }
}