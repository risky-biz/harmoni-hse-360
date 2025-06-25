using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Domain.Events.WorkPermitSettings;

/// <summary>
/// Domain event raised when a safety video is deactivated
/// </summary>
public record SafetyVideoDeactivatedEvent(WorkPermitSafetyVideo SafetyVideo) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}