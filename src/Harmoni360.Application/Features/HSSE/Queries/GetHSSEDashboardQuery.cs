using MediatR;
using Harmoni360.Application.Features.HSSE.DTOs;

namespace Harmoni360.Application.Features.HSSE.Queries;

public record GetHSSEDashboardQuery : IRequest<HSSEDashboardDto>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Department { get; init; }
    public string? Location { get; init; }
    public bool IncludeTrends { get; init; } = true;
    public bool IncludeComparisons { get; init; } = true;
}