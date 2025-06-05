using MediatR;
using HarmoniHSE360.Application.Features.Incidents.DTOs;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetIncidentByIdQuery : IRequest<IncidentDto?>
{
    public int Id { get; set; }
}