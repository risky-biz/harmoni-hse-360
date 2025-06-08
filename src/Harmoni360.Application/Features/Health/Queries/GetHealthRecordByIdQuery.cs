using MediatR;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public record GetHealthRecordByIdQuery : IRequest<HealthRecordDetailDto?>
{
    public int Id { get; init; }
    public bool IncludeInactive { get; init; } = false;
}