using Harmoni360.Application.Features.Incidents.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Incidents.Queries;

public class GetAvailableUsersQuery : IRequest<List<UserDto>>
{
    public string? SearchTerm { get; set; }
}