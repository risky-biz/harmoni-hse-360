using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class WorkPermitPrecaution : BaseEntity, IAuditableEntity
{
    public int WorkPermitId { get; set; }
    public string PrecautionDescription { get; set; } = string.Empty;
    public PrecautionCategory Category { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public string CompletionNotes { get; set; } = string.Empty;
    public int Priority { get; set; } = 1; // 1-5 priority scale
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string VerificationMethod { get; set; } = string.Empty;
    public bool RequiresVerification { get; set; } = true;
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;

    // Indonesian K3 Compliance
    public bool IsK3Requirement { get; set; } = false;
    public string K3StandardReference { get; set; } = string.Empty; // Reference to Indonesian K3 standards
    public bool IsMandatoryByLaw { get; set; } = false;

    // Navigation Properties
    public WorkPermit? WorkPermit { get; set; }

    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static WorkPermitPrecaution Create(
        int workPermitId,
        string precautionDescription,
        PrecautionCategory category,
        bool isRequired = true,
        int priority = 1,
        string responsiblePerson = "",
        string verificationMethod = "")
    {
        return new WorkPermitPrecaution
        {
            WorkPermitId = workPermitId,
            PrecautionDescription = precautionDescription,
            Category = category,
            IsRequired = isRequired,
            Priority = priority,
            ResponsiblePerson = responsiblePerson,
            VerificationMethod = verificationMethod,
            RequiresVerification = !string.IsNullOrEmpty(verificationMethod),
            IsCompleted = false,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkCompleted(string completedBy, string notes = "")
    {
        if (IsCompleted)
            throw new InvalidOperationException("Precaution is already completed.");

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = completedBy;
        CompletionNotes = notes;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = completedBy;
    }

    public void Verify(string verifiedBy, string notes = "")
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Precaution must be completed before verification.");

        if (!RequiresVerification)
            throw new InvalidOperationException("This precaution does not require verification.");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
        if (!string.IsNullOrEmpty(notes))
        {
            CompletionNotes += $" | Verification: {notes}";
        }
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = verifiedBy;
    }

    public void SetK3Compliance(bool isK3Requirement, string k3StandardReference = "", bool isMandatoryByLaw = false)
    {
        IsK3Requirement = isK3Requirement;
        K3StandardReference = k3StandardReference;
        IsMandatoryByLaw = isMandatoryByLaw;
    }

    public void UpdatePriority(int newPriority)
    {
        if (newPriority < 1 || newPriority > 5)
            throw new ArgumentException("Priority must be between 1 and 5.");

        Priority = newPriority;
        LastModifiedAt = DateTime.UtcNow;
    }
}