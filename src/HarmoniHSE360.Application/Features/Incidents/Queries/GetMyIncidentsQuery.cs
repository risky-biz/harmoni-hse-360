using MediatR;
using HarmoniHSE360.Application.Features.Incidents.DTOs;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetMyIncidentsQuery : IRequest<GetIncidentsResponse>
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int UserId { get; set; }
}