using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record CreateUserCommand : IRequest<UserDto>
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string EmployeeId { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public List<int> RoleIds { get; init; } = new();
}