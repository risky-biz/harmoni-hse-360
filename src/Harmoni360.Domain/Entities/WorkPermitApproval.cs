using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class WorkPermitApproval : BaseEntity, IAuditableEntity
{
    public int WorkPermitId { get; set; }
    public int ApprovedById { get; set; }
    public string ApprovedByName { get; set; } = string.Empty;
    public string ApprovalLevel { get; set; } = string.Empty; // SafetyOfficer, DepartmentHead, HSEManager, etc.
    public DateTime ApprovedAt { get; set; }
    public bool IsApproved { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty; // Digital signature or path to signature image
    public int ApprovalOrder { get; set; } // Order of approval in the workflow
    
    // Indonesian Compliance Fields
    public string K3CertificateNumber { get; set; } = string.Empty; // K3 Certificate number of approver
    public bool HasAuthorityToApprove { get; set; } = true;
    public string AuthorityLevel { get; set; } = string.Empty; // Level of authority (Pengawas K3, Ahli K3, etc.)
    
    // Navigation Properties
    public WorkPermit? WorkPermit { get; set; }

    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static WorkPermitApproval Create(
        int workPermitId,
        int approvedById,
        string approvedByName,
        string approvalLevel,
        bool isApproved,
        string comments = "",
        int approvalOrder = 0)
    {
        return new WorkPermitApproval
        {
            WorkPermitId = workPermitId,
            ApprovedById = approvedById,
            ApprovedByName = approvedByName,
            ApprovalLevel = approvalLevel,
            ApprovedAt = DateTime.UtcNow,
            IsApproved = isApproved,
            Comments = comments,
            ApprovalOrder = approvalOrder,
            HasAuthorityToApprove = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetIndonesianComplianceInfo(string k3CertificateNumber, string authorityLevel)
    {
        K3CertificateNumber = k3CertificateNumber;
        AuthorityLevel = authorityLevel;
    }

    public void AddSignature(string signaturePath)
    {
        Signature = signaturePath;
    }
}