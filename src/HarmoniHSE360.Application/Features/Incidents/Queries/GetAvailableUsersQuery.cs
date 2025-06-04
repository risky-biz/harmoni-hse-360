using HarmoniHSE360.Application.Features.Incidents.DTOs;
using MediatR;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetAvailableUsersQuery : IRequest<List<UserDto>>
{
    public string? SearchTerm { get; set; }
}