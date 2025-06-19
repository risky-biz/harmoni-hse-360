using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Domain.Events.WorkPermitSettings;

/// <summary>
/// Domain event raised when Work Permit Settings are activated
/// </summary>
public record WorkPermitSettingsActivatedEvent(Entities.WorkPermitSettings WorkPermitSettings) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}