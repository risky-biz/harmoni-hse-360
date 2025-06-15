using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class LicenseAttachment : BaseEntity, IAuditableEntity
{
    public int LicenseId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public LicenseAttachmentType AttachmentType { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public DateTime? ValidUntil { get; set; }
    
    // Navigation Properties
    public License? License { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Computed Properties
    public bool IsExpired => ValidUntil.HasValue && DateTime.UtcNow > ValidUntil.Value;

    public static LicenseAttachment Create(
        int licenseId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string filePath,
        string uploadedBy,
        LicenseAttachmentType attachmentType,
        string description = "",
        bool isRequired = false,
        DateTime? validUntil = null)
    {
        return new LicenseAttachment
        {
            LicenseId = licenseId,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            AttachmentType = attachmentType,
            Description = description,
            IsRequired = isRequired,
            ValidUntil = validUntil,
            CreatedAt = DateTime.UtcNow
        };
    }
}