using MediatR;
using Harmoni360.Domain.Enums;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record ChangeUserStatusCommand : IRequest<UserDto>
{
    public int UserId { get; init; }
    public UserStatus Status { get; init; }
    public string? Reason { get; init; }
}