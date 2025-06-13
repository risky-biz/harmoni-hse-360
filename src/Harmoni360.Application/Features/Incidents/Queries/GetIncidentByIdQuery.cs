using MediatR;
using Harmoni360.Application.Features.Incidents.DTOs;

namespace Harmoni360.Application.Features.Incidents.Queries;

public class GetIncidentByIdQuery : IRequest<IncidentDto?>
{
    public int Id { get; set; }
}