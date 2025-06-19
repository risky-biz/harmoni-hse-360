using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Domain.Events.WorkPermitSettings;

/// <summary>
/// Domain event raised when a safety video is updated
/// </summary>
public record SafetyVideoUpdatedEvent(WorkPermitSafetyVideo SafetyVideo) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}