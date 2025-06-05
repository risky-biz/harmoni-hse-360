using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class PPEComplianceRequirement : BaseEntity, IAuditableEntity
{
    public int RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public int CategoryId { get; private set; }
    public PPECategory Category { get; private set; } = null!;
    public bool IsMandatory { get; private set; }
    public string? RiskAssessmentReference { get; private set; }
    public string? ComplianceNote { get; private set; }
    public int? MinimumQuantity { get; private set; }
    public int? ReplacementIntervalDays { get; private set; }
    public bool RequiresTraining { get; private set; }
    public string? TrainingRequirements { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected PPEComplianceRequirement() { } // For EF Core

    public static PPEComplianceRequirement Create(
        int roleId,
        int categoryId,
        bool isMandatory,
        string createdBy,
        string? riskAssessmentReference = null,
        string? complianceNote = null,
        int? minimumQuantity = null,
        int? replacementIntervalDays = null,
        bool requiresTraining = false,
        string? trainingRequirements = null)
    {
        var requirement = new PPEComplianceRequirement
        {
            RoleId = roleId,
            CategoryId = categoryId,
            IsMandatory = isMandatory,
            RiskAssessmentReference = riskAssessmentReference,
            ComplianceNote = complianceNote,
            MinimumQuantity = minimumQuantity,
            ReplacementIntervalDays = replacementIntervalDays,
            RequiresTraining = requiresTraining,
            TrainingRequirements = trainingRequirements,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        return requirement;
    }

    public void UpdateRequirement(
        bool isMandatory,
        string? riskAssessmentReference,
        string? complianceNote,
        int? minimumQuantity,
        int? replacementIntervalDays,
        bool requiresTraining,
        string? trainingRequirements,
        string modifiedBy)
    {
        IsMandatory = isMandatory;
        RiskAssessmentReference = riskAssessmentReference;
        ComplianceNote = complianceNote;
        MinimumQuantity = minimumQuantity;
        ReplacementIntervalDays = replacementIntervalDays;
        RequiresTraining = requiresTraining;
        TrainingRequirements = trainingRequirements;
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

    // Computed Properties
    public bool HasQuantityRequirement => MinimumQuantity.HasValue && MinimumQuantity > 0;
    public bool HasReplacementSchedule => ReplacementIntervalDays.HasValue && ReplacementIntervalDays > 0;
}