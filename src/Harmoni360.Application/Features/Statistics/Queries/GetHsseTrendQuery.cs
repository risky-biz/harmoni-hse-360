using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.Statistics.Queries;

public record GetHsseTrendQuery : IRequest<List<HsseTrendPointDto>>
{
    public ModuleType? Module { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
