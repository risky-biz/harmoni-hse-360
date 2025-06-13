using MediatR;

namespace Harmoni360.Application.Features.Health.Commands;

public record RemoveMedicalConditionCommand : IRequest<bool>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
}