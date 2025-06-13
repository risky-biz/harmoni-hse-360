using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class WorkPermitAttachment : BaseEntity, IAuditableEntity
{
    public int WorkPermitId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public WorkPermitAttachmentType AttachmentType { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation Properties
    public WorkPermit? WorkPermit { get; set; }

    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static WorkPermitAttachment Create(
        int workPermitId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string filePath,
        string uploadedBy,
        WorkPermitAttachmentType attachmentType,
        string description = "")
    {
        return new WorkPermitAttachment
        {
            WorkPermitId = workPermitId,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            AttachmentType = attachmentType,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }
}