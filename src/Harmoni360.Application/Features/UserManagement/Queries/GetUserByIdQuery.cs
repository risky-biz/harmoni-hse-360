using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public record GetUserByIdQuery : IRequest<UserDto?>
{
    public int UserId { get; init; }
}