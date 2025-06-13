using MediatR;
using Harmoni360.Application.Features.Hazards.DTOs;

namespace Harmoni360.Application.Features.Hazards.Queries;

public class GetMyHazardsQuery : IRequest<GetHazardsResponse>
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int UserId { get; set; }
}