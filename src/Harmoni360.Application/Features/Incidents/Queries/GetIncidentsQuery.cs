using MediatR;
using Harmoni360.Application.Features.Incidents.DTOs;

namespace Harmoni360.Application.Features.Incidents.Queries;

public class GetIncidentsQuery : IRequest<GetIncidentsResponse>
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetIncidentsResponse
{
    public List<IncidentDto> Incidents { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}