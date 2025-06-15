using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record UpdateUserCommand : IRequest<UserDto>
{
    public int UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public List<int> RoleIds { get; init; } = new();
}