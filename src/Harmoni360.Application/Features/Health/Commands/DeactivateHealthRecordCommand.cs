using MediatR;

namespace Harmoni360.Application.Features.Health.Commands;

public record DeactivateHealthRecordCommand : IRequest<bool>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
}