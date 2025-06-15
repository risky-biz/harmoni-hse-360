using MediatR;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record WithdrawParticipantCommand : IRequest
{
    public int TrainingId { get; init; }
    public int ParticipantId { get; init; }
    public string WithdrawalReason { get; init; } = string.Empty;
}