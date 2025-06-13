using Harmoni360.Application.Features.Incidents.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Incidents.Queries;

public class GetIncidentDetailQuery : IRequest<IncidentDetailDto?>
{
    public int Id { get; set; }
}