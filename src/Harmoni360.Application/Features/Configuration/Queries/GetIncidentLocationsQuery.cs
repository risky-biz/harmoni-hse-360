using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Configuration.Queries;

public class GetIncidentLocationsQuery : IRequest<IEnumerable<IncidentLocationDto>>
{
    public bool? IsActive { get; set; } = true;
    public string? Building { get; set; }
}