using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public enum ConditionType
{
    Allergy = 1,
    ChronicCondition = 2,
    MedicationDependency = 3,
    PhysicalLimitation = 4,
    MentalHealthCondition = 5,
    Dietary = 6,
    Other = 7
}

public enum ConditionSeverity
{
    Mild = 1,
    Moderate = 2,
    Severe = 3,
    LifeThreatening = 4
}

public class MedicalCondition : BaseEntity, IAuditableEntity
{
    public int HealthRecordId { get; private set; }
    public ConditionType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ConditionSeverity Severity { get; private set; }
    public string? TreatmentPlan { get; private set; }
    public DateTime? DiagnosedDate { get; private set; }
    public bool RequiresEmergencyAction { get; private set; }
    public string? EmergencyInstructions { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // Navigation properties
    public HealthRecord HealthRecord { get; private set; } = null!;

    protected MedicalCondition() { } // For EF Core

    public static MedicalCondition Create(
        int healthRecordId,
        ConditionType type,
        string name,
        string description,
        ConditionSeverity severity,
        string? treatmentPlan = null,
        DateTime? diagnosedDate = null,
        bool requiresEmergencyAction = false,
        string? emergencyInstructions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Medical condition name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Medical condition description cannot be empty", nameof(description));

        if (requiresEmergencyAction && string.IsNullOrWhiteSpace(emergencyInstructions))
            throw new ArgumentException("Emergency instructions are required for conditions requiring emergency action", nameof(emergencyInstructions));

        var condition = new MedicalCondition
        {
            HealthRecordId = healthRecordId,
            Type = type,
            Name = name.Trim(),
            Description = description.Trim(),
            Severity = severity,
            TreatmentPlan = treatmentPlan?.Trim(),
            DiagnosedDate = diagnosedDate,
            RequiresEmergencyAction = requiresEmergencyAction,
            EmergencyInstructions = emergencyInstructions?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System" // Will be set by infrastructure
        };

        return condition;
    }

    public void Update(
        ConditionType type,
        string name,
        string description,
        ConditionSeverity severity,
        string? treatmentPlan = null,
        DateTime? diagnosedDate = null,
        bool requiresEmergencyAction = false,
        string? emergencyInstructions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Medical condition name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Medical condition description cannot be empty", nameof(description));

        if (requiresEmergencyAction && string.IsNullOrWhiteSpace(emergencyInstructions))
            throw new ArgumentException("Emergency instructions are required for conditions requiring emergency action", nameof(emergencyInstructions));

        Type = type;
        Name = name.Trim();
        Description = description.Trim();
        Severity = severity;
        TreatmentPlan = treatmentPlan?.Trim();
        DiagnosedDate = diagnosedDate;
        RequiresEmergencyAction = requiresEmergencyAction;
        EmergencyInstructions = emergencyInstructions?.Trim();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsLifeThreatening()
    {
        return Severity == ConditionSeverity.LifeThreatening || RequiresEmergencyAction;
    }

    public bool IsAllergy()
    {
        return Type == ConditionType.Allergy;
    }

    public string GetDisplayName()
    {
        return $"{Name} ({Type})";
    }
}