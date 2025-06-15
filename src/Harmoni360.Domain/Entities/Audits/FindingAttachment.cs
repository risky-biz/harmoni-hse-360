using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Audits;

public class FindingAttachment : BaseEntity, IAuditableEntity
{
    public int AuditFindingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public AuditAttachmentType AttachmentType { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsEvidence { get; set; }

    // Navigation Properties
    public virtual AuditFinding AuditFinding { get; set; } = null!;

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static FindingAttachment Create(
        int auditFindingId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string filePath,
        string uploadedBy,
        AuditAttachmentType attachmentType,
        string description = "",
        bool isEvidence = true)
    {
        return new FindingAttachment
        {
            AuditFindingId = auditFindingId,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            AttachmentType = attachmentType,
            Description = description,
            IsEvidence = isEvidence,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDescription(string description)
    {
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
    }
}