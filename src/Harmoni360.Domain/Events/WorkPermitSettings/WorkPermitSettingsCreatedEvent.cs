using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Domain.Events.WorkPermitSettings;

/// <summary>
/// Domain event raised when Work Permit Settings are created
/// </summary>
public record WorkPermitSettingsCreatedEvent(Entities.WorkPermitSettings WorkPermitSettings) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}