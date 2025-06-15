using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public record GetRolesQuery : IRequest<List<RoleDto>>
{
    public bool IncludeInactive { get; init; } = false;
}