using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record UnlockUserAccountCommand : IRequest<UserDto>
{
    public int UserId { get; init; }
    public string? Reason { get; init; }
}