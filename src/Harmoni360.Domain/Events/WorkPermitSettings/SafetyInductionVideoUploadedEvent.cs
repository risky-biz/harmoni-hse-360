using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Domain.Events.WorkPermitSettings;

/// <summary>
/// Domain event raised when a safety induction video is uploaded
/// </summary>
public record SafetyInductionVideoUploadedEvent(
    Entities.WorkPermitSettings WorkPermitSettings,
    WorkPermitSafetyVideo SafetyVideo) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}