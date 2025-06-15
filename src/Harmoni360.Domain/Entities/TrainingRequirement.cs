using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class TrainingRequirement : BaseEntity, IAuditableEntity
{
    public int TrainingId { get; private set; }
    public string RequirementDescription { get; private set; } = string.Empty;
    public RequirementStatus Status { get; private set; }
    public bool IsMandatory { get; private set; }
    public int Priority { get; private set; } = 1; // 1 = highest priority
    
    // Timing Information
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public bool IsOverdue { get; private set; }
    
    // Assignment Information
    public string AssignedTo { get; private set; } = string.Empty; // Role or specific person responsible
    public string AssignedBy { get; private set; } = string.Empty;
    public DateTime AssignedDate { get; private set; }
    
    // Completion Information
    public string CompletedBy { get; private set; } = string.Empty;
    public string CompletionNotes { get; private set; } = string.Empty;
    public string VerificationMethod { get; private set; } = string.Empty;
    public bool RequiresVerification { get; private set; } = true;
    public bool IsVerified { get; private set; }
    public string VerifiedBy { get; private set; } = string.Empty;
    public DateTime? VerificationDate { get; private set; }
    
    // Indonesian Compliance Fields
    public bool IsK3Requirement { get; private set; }
    public string K3RegulationReference { get; private set; } = string.Empty;
    public bool IsGovernmentMandated { get; private set; }
    public string RegulatoryReference { get; private set; } = string.Empty; // Ministry regulation reference
    public bool IsBPJSRelated { get; private set; }
    
    // Documentation
    public string DocumentationRequired { get; private set; } = string.Empty;
    public string EvidenceProvided { get; private set; } = string.Empty;
    public string AttachmentPath { get; private set; } = string.Empty;
    
    // Risk and Compliance
    public RiskLevel RiskLevelIfNotCompleted { get; private set; } = RiskLevel.Medium;
    public string ComplianceNotes { get; private set; } = string.Empty;
    public decimal? ComplianceCost { get; private set; }
    
    // Navigation Properties
    public Training? Training { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private TrainingRequirement() { } // For EF Core

    public static TrainingRequirement Create(
        int trainingId,
        string requirementDescription,
        bool isMandatory,
        DateTime? dueDate = null,
        string assignedTo = "",
        string assignedBy = "",
        int priority = 1)
    {
        var requirement = new TrainingRequirement
        {
            TrainingId = trainingId,
            RequirementDescription = requirementDescription,
            Status = RequirementStatus.Pending,
            IsMandatory = isMandatory,
            Priority = priority,
            DueDate = dueDate,
            AssignedTo = assignedTo,
            AssignedBy = assignedBy,
            AssignedDate = DateTime.UtcNow,
            RequiresVerification = isMandatory, // Mandatory requirements need verification
            IsVerified = false,
            IsOverdue = false,
            RiskLevelIfNotCompleted = isMandatory ? RiskLevel.High : RiskLevel.Medium,
            CreatedAt = DateTime.UtcNow
        };

        return requirement;
    }

    public void UpdateDetails(
        string requirementDescription,
        bool isMandatory,
        DateTime? dueDate = null,
        int priority = 1)
    {
        if (Status == RequirementStatus.Completed)
            throw new InvalidOperationException("Cannot update completed requirements.");

        RequirementDescription = requirementDescription;
        IsMandatory = isMandatory;
        DueDate = dueDate;
        Priority = priority;
        
        // Update verification requirement based on mandatory status
        RequiresVerification = isMandatory;
        RiskLevelIfNotCompleted = isMandatory ? RiskLevel.High : RiskLevel.Medium;
        
        UpdateOverdueStatus();
    }

    public void AssignTo(string assignedTo, string assignedBy)
    {
        if (Status == RequirementStatus.Completed)
            throw new InvalidOperationException("Cannot reassign completed requirements.");

        AssignedTo = assignedTo;
        AssignedBy = assignedBy;
        AssignedDate = DateTime.UtcNow;

        if (Status == RequirementStatus.Pending)
        {
            Status = RequirementStatus.InProgress;
        }
    }

    public void MarkAsInProgress(string startedBy, string notes = "")
    {
        if (Status != RequirementStatus.Pending)
            throw new InvalidOperationException("Only pending requirements can be marked as in progress.");

        Status = RequirementStatus.InProgress;
        CompletionNotes = $"Started by {startedBy}. {notes}".Trim();
    }

    public void MarkAsCompleted(
        string completedBy,
        string completionNotes = "",
        string evidenceProvided = "",
        string attachmentPath = "")
    {
        if (Status == RequirementStatus.Completed)
            throw new InvalidOperationException("Requirement is already completed.");

        Status = RequirementStatus.Completed;
        CompletedBy = completedBy;
        CompletedDate = DateTime.UtcNow;
        CompletionNotes = completionNotes;
        EvidenceProvided = evidenceProvided;
        AttachmentPath = attachmentPath;
        
        // If verification is not required, auto-verify
        if (!RequiresVerification)
        {
            IsVerified = true;
            VerifiedBy = completedBy;
            VerificationDate = DateTime.UtcNow;
        }
        
        UpdateOverdueStatus();
    }

    public void VerifyCompletion(string verifiedBy, string verificationNotes = "")
    {
        if (Status != RequirementStatus.Completed)
            throw new InvalidOperationException("Only completed requirements can be verified.");

        if (!RequiresVerification)
            throw new InvalidOperationException("This requirement does not require verification.");

        IsVerified = true;
        VerifiedBy = verifiedBy;
        VerificationDate = DateTime.UtcNow;
        ComplianceNotes = verificationNotes;
    }

    public void RejectVerification(string rejectedBy, string rejectionReason)
    {
        if (Status != RequirementStatus.Completed)
            throw new InvalidOperationException("Only completed requirements can have verification rejected.");

        IsVerified = false;
        Status = RequirementStatus.InProgress;
        ComplianceNotes = $"Verification rejected by {rejectedBy}: {rejectionReason}";
        VerificationDate = null;
        VerifiedBy = string.Empty;
    }

    public void MarkAsOverdue()
    {
        if (Status == RequirementStatus.Completed)
            return; // Completed requirements cannot be overdue

        if (DueDate.HasValue && DateTime.UtcNow > DueDate.Value)
        {
            Status = RequirementStatus.Overdue;
            IsOverdue = true;
        }
    }

    public void WaiveRequirement(string waivedBy, string waiverReason)
    {
        if (Status == RequirementStatus.Completed)
            throw new InvalidOperationException("Cannot waive completed requirements.");

        Status = RequirementStatus.Waived;
        CompletedBy = waivedBy;
        CompletedDate = DateTime.UtcNow;
        CompletionNotes = $"Requirement waived: {waiverReason}";
        IsVerified = true; // Waived requirements are considered verified
        VerifiedBy = waivedBy;
        VerificationDate = DateTime.UtcNow;
    }

    public void SetAsNotApplicable(string determinedBy, string reason)
    {
        Status = RequirementStatus.NotApplicable;
        CompletionNotes = $"Marked as not applicable by {determinedBy}: {reason}";
        IsVerified = true; // Not applicable requirements don't need verification
        VerifiedBy = determinedBy;
        VerificationDate = DateTime.UtcNow;
    }

    public void SetIndonesianComplianceInfo(
        bool isK3Requirement,
        string k3RegulationReference = "",
        bool isGovernmentMandated = false,
        string regulatoryReference = "",
        bool isBPJSRelated = false)
    {
        IsK3Requirement = isK3Requirement;
        K3RegulationReference = k3RegulationReference;
        IsGovernmentMandated = isGovernmentMandated;
        RegulatoryReference = regulatoryReference;
        IsBPJSRelated = isBPJSRelated;

        // Government mandated requirements are automatically mandatory and high risk
        if (isGovernmentMandated)
        {
            IsMandatory = true;
            RequiresVerification = true;
            RiskLevelIfNotCompleted = RiskLevel.Critical;
        }

        // K3 requirements are typically high priority
        if (isK3Requirement)
        {
            Priority = 1; // Highest priority
            RequiresVerification = true;
            if (RiskLevelIfNotCompleted == RiskLevel.Medium)
            {
                RiskLevelIfNotCompleted = RiskLevel.High;
            }
        }
    }

    public void SetVerificationMethod(string verificationMethod)
    {
        VerificationMethod = verificationMethod;
        RequiresVerification = !string.IsNullOrEmpty(verificationMethod);
    }

    public void SetDocumentationRequirements(string documentationRequired, decimal? complianceCost = null)
    {
        DocumentationRequired = documentationRequired;
        ComplianceCost = complianceCost;
    }

    public void ExtendDueDate(DateTime newDueDate, string extendedBy, string reason)
    {
        if (Status == RequirementStatus.Completed)
            throw new InvalidOperationException("Cannot extend due date for completed requirements.");

        if (newDueDate <= DateTime.UtcNow)
            throw new ArgumentException("New due date must be in the future.");

        DueDate = newDueDate;
        ComplianceNotes += $"\nDue date extended by {extendedBy} to {newDueDate:yyyy-MM-dd}. Reason: {reason}";
        
        // Reset overdue status if it was overdue
        if (Status == RequirementStatus.Overdue)
        {
            Status = RequirementStatus.InProgress;
            IsOverdue = false;
        }
    }

    private void UpdateOverdueStatus()
    {
        if (DueDate.HasValue && DateTime.UtcNow > DueDate.Value && Status != RequirementStatus.Completed)
        {
            if (Status != RequirementStatus.Overdue)
            {
                Status = RequirementStatus.Overdue;
            }
            IsOverdue = true;
        }
        else
        {
            IsOverdue = false;
        }
    }

    public int GetDaysUntilDue()
    {
        if (!DueDate.HasValue)
            return int.MaxValue;

        return (int)(DueDate.Value - DateTime.UtcNow).TotalDays;
    }

    public bool IsHighPriority()
    {
        return Priority <= 2 || IsMandatory || IsGovernmentMandated || IsK3Requirement;
    }

    public bool IsComplianceRelated()
    {
        return IsGovernmentMandated || IsK3Requirement || IsBPJSRelated || !string.IsNullOrEmpty(RegulatoryReference);
    }

    public string GetStatusDescription()
    {
        return Status switch
        {
            RequirementStatus.Pending => "Waiting to be started",
            RequirementStatus.InProgress => $"In progress{(string.IsNullOrEmpty(AssignedTo) ? "" : $" (Assigned to: {AssignedTo})")}",
            RequirementStatus.Completed => IsVerified ? "Completed and verified" : "Completed (pending verification)",
            RequirementStatus.Overdue => $"Overdue{(DueDate.HasValue ? $" (Due: {DueDate.Value:yyyy-MM-dd})" : "")}",
            RequirementStatus.Waived => "Waived",
            RequirementStatus.NotApplicable => "Not applicable",
            _ => Status.ToString()
        };
    }
}