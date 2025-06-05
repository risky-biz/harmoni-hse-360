using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class PPECategory : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public PPEType Type { get; private set; }
    public bool RequiresCertification { get; private set; }
    public bool RequiresInspection { get; private set; }
    public int? InspectionIntervalDays { get; private set; }
    public bool RequiresExpiry { get; private set; }
    public int? DefaultExpiryDays { get; private set; }
    public string? ComplianceStandard { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public virtual ICollection<PPEItem> PPEItems { get; private set; } = new List<PPEItem>();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected PPECategory() { } // For EF Core

    public static PPECategory Create(
        string name,
        string code,
        string description,
        PPEType type,
        string createdBy,
        bool requiresCertification = false,
        bool requiresInspection = false,
        int? inspectionIntervalDays = null,
        bool requiresExpiry = false,
        int? defaultExpiryDays = null,
        string? complianceStandard = null)
    {
        var category = new PPECategory
        {
            Name = name,
            Code = code,
            Description = description,
            Type = type,
            RequiresCertification = requiresCertification,
            RequiresInspection = requiresInspection,
            InspectionIntervalDays = inspectionIntervalDays,
            RequiresExpiry = requiresExpiry,
            DefaultExpiryDays = defaultExpiryDays,
            ComplianceStandard = complianceStandard,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        return category;
    }

    public void UpdateDetails(
        string name,
        string code,
        string description,
        PPEType type,
        string modifiedBy,
        bool requiresCertification,
        bool requiresInspection,
        int? inspectionIntervalDays,
        bool requiresExpiry,
        int? defaultExpiryDays,
        string? complianceStandard)
    {
        Name = name;
        Code = code;
        Description = description;
        Type = type;
        RequiresCertification = requiresCertification;
        RequiresInspection = requiresInspection;
        InspectionIntervalDays = inspectionIntervalDays;
        RequiresExpiry = requiresExpiry;
        DefaultExpiryDays = defaultExpiryDays;
        ComplianceStandard = complianceStandard;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Deactivate(string modifiedBy)
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Activate(string modifiedBy)
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}

public enum PPEType
{
    HeadProtection = 1,
    EyeProtection = 2,
    HearingProtection = 3,
    RespiratoryProtection = 4,
    HandProtection = 5,
    FootProtection = 6,
    BodyProtection = 7,
    FallProtection = 8,
    HighVisibility = 9,
    EmergencyEquipment = 10
}