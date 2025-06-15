using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Audits;

public class AuditAttachment : BaseEntity, IAuditableEntity
{
    public int AuditId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public AuditAttachmentType AttachmentType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsEvidence { get; set; }
    public int? AuditItemId { get; set; }

    // Navigation Properties
    public virtual Audit Audit { get; set; } = null!;
    public virtual AuditItem? AuditItem { get; set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static AuditAttachment Create(
        int auditId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string filePath,
        string uploadedBy,
        AuditAttachmentType attachmentType,
        string description = "",
        string? category = null,
        bool isEvidence = false,
        int? auditItemId = null)
    {
        return new AuditAttachment
        {
            AuditId = auditId,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            AttachmentType = attachmentType,
            Description = description,
            Category = category,
            IsEvidence = isEvidence,
            AuditItemId = auditItemId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDescription(string description, string? category = null)
    {
        Description = description;
        Category = category;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsEvidence(bool isEvidence = true)
    {
        IsEvidence = isEvidence;
        LastModifiedAt = DateTime.UtcNow;
    }
}