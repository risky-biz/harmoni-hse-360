using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class LicenseCondition : BaseEntity, IAuditableEntity
{
    public int LicenseId { get; set; }
    public string ConditionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public DateTime? DueDate { get; set; }
    public LicenseConditionStatus Status { get; set; }
    public string ComplianceEvidence { get; set; } = string.Empty;
    public DateTime? ComplianceDate { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    // Navigation Properties
    public License? License { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Computed Properties
    public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value && Status != LicenseConditionStatus.Completed;

    public static LicenseCondition Create(
        int licenseId,
        string conditionType,
        string description,
        bool isMandatory,
        DateTime? dueDate = null,
        string responsiblePerson = "")
    {
        return new LicenseCondition
        {
            LicenseId = licenseId,
            ConditionType = conditionType,
            Description = description,
            IsMandatory = isMandatory,
            DueDate = dueDate,
            Status = LicenseConditionStatus.Pending,
            ResponsiblePerson = responsiblePerson,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void StartProgress(string startedBy, string notes = "")
    {
        if (Status != LicenseConditionStatus.Pending)
            throw new InvalidOperationException("Only pending conditions can be started.");
            
        Status = LicenseConditionStatus.InProgress;
        Notes = notes;
    }

    public void Complete(string completedBy, string complianceEvidence, string notes = "")
    {
        if (Status == LicenseConditionStatus.Completed)
            throw new InvalidOperationException("Condition is already completed.");
            
        Status = LicenseConditionStatus.Completed;
        ComplianceDate = DateTime.UtcNow;
        ComplianceEvidence = complianceEvidence;
        VerifiedBy = completedBy;
        Notes = notes;
    }

    public void Waive(string waivedBy, string waiverReason)
    {
        if (Status == LicenseConditionStatus.Completed)
            throw new InvalidOperationException("Completed conditions cannot be waived.");
            
        Status = LicenseConditionStatus.Waived;
        Notes = $"Waived by {waivedBy}: {waiverReason}";
    }

    public void UpdateStatus(LicenseConditionStatus newStatus, string updatedBy, string notes = "")
    {
        Status = newStatus;
        Notes = notes;
        
        if (newStatus == LicenseConditionStatus.Overdue && DueDate.HasValue)
        {
            Notes = $"Overdue since {DueDate.Value:yyyy-MM-dd}. {notes}";
        }
    }
}