using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Events;

public class LicenseConditionRemovedEvent : IDomainEvent
{
    public int LicenseId { get; }
    public int ConditionId { get; }
    public string ConditionType { get; }
    public string Description { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public LicenseConditionRemovedEvent(int licenseId, int conditionId, string conditionType, string description)
    {
        LicenseId = licenseId;
        ConditionId = conditionId;
        ConditionType = conditionType;
        Description = description;
    }
}