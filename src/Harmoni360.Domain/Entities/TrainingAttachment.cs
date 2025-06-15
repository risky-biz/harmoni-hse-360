using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class TrainingAttachment : BaseEntity, IAuditableEntity
{
    public int TrainingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public TrainingAttachmentType AttachmentType { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Access and Security
    public bool IsPublic { get; set; } = false; // Whether attachment is visible to all participants
    public bool IsInstructorOnly { get; set; } = false; // Whether only instructors can access
    public bool IsParticipantSubmission { get; set; } = false; // Whether this is submitted by a participant
    public int? SubmittedByParticipantId { get; set; } // If submitted by participant
    
    // Version Control
    public int Version { get; set; } = 1;
    public string VersionNotes { get; set; } = string.Empty;
    public int? PreviousVersionId { get; set; } // Links to previous version
    public bool IsCurrentVersion { get; set; } = true;
    
    // Indonesian Compliance Fields
    public bool IsComplianceDocument { get; set; } = false;
    public string RegulatoryReference { get; set; } = string.Empty; // Reference to Indonesian regulation
    public bool IsK3Document { get; set; } = false;
    public string K3DocumentType { get; set; } = string.Empty; // SOP, JSA, Risk Assessment, etc.
    public bool RequiresApproval { get; set; } = false;
    public bool IsApproved { get; set; } = false;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime? ApprovalDate { get; set; }
    
    // Document Properties
    public string Language { get; set; } = "id-ID"; // Default to Indonesian
    public bool IsTranslationRequired { get; set; } = false;
    public string TranslatedFrom { get; set; } = string.Empty;
    public bool HasDigitalSignature { get; set; } = false;
    public string SignatureInfo { get; set; } = string.Empty;
    
    // Usage Tracking
    public int DownloadCount { get; set; } = 0;
    public DateTime? LastAccessedAt { get; set; }
    public string LastAccessedBy { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
    public DateTime? ArchivedAt { get; set; }
    
    // Validation and Verification
    public string ChecksumMD5 { get; set; } = string.Empty;
    public string ChecksumSHA256 { get; set; } = string.Empty;
    public bool IsVirusScanned { get; set; } = false;
    public DateTime? VirusScanDate { get; set; }
    public bool IsVirusClean { get; set; } = true;
    
    // Navigation Properties
    public Training? Training { get; set; }
    public TrainingParticipant? SubmittedByParticipant { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static TrainingAttachment Create(
        int trainingId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string filePath,
        string uploadedBy,
        TrainingAttachmentType attachmentType,
        string description = "",
        bool isPublic = false,
        bool isInstructorOnly = false)
    {
        return new TrainingAttachment
        {
            TrainingId = trainingId,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            AttachmentType = attachmentType,
            Description = description,
            IsPublic = isPublic,
            IsInstructorOnly = isInstructorOnly,
            Version = 1,
            IsCurrentVersion = true,
            Language = "id-ID",
            IsVirusClean = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static TrainingAttachment CreateParticipantSubmission(
        int trainingId,
        int participantId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string filePath,
        string uploadedBy,
        string description = "")
    {
        return new TrainingAttachment
        {
            TrainingId = trainingId,
            SubmittedByParticipantId = participantId,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            AttachmentType = TrainingAttachmentType.Assessment, // Default for participant submissions
            Description = description,
            IsParticipantSubmission = true,
            IsPublic = false,
            IsInstructorOnly = true, // Only instructors can access participant submissions
            Version = 1,
            IsCurrentVersion = true,
            Language = "id-ID",
            IsVirusClean = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void SetAccessPermissions(bool isPublic, bool isInstructorOnly)
    {
        if (isPublic && isInstructorOnly)
            throw new ArgumentException("Attachment cannot be both public and instructor-only.");

        IsPublic = isPublic;
        IsInstructorOnly = isInstructorOnly;
    }

    public void SetIndonesianComplianceInfo(
        bool isComplianceDocument,
        string regulatoryReference = "",
        bool isK3Document = false,
        string k3DocumentType = "",
        bool requiresApproval = false)
    {
        IsComplianceDocument = isComplianceDocument;
        RegulatoryReference = regulatoryReference;
        IsK3Document = isK3Document;
        K3DocumentType = k3DocumentType;
        RequiresApproval = requiresApproval;

        // K3 documents typically require approval
        if (isK3Document && !requiresApproval)
        {
            RequiresApproval = true;
        }
    }

    public void SetLanguageInfo(string language, bool requiresTranslation = false, string translatedFrom = "")
    {
        Language = language;
        IsTranslationRequired = requiresTranslation;
        TranslatedFrom = translatedFrom;
    }

    public void ApproveDocument(string approvedBy)
    {
        if (!RequiresApproval)
            throw new InvalidOperationException("This document does not require approval.");

        IsApproved = true;
        ApprovedBy = approvedBy;
        ApprovalDate = DateTime.UtcNow;
    }

    public void RejectApproval(string rejectedBy, string reason)
    {
        if (!RequiresApproval)
            throw new InvalidOperationException("This document does not require approval.");

        IsApproved = false;
        ApprovedBy = string.Empty;
        ApprovalDate = null;
        Description += $"\nApproval rejected by {rejectedBy}: {reason}";
    }

    public void RecordAccess(string accessedBy)
    {
        DownloadCount++;
        LastAccessedAt = DateTime.UtcNow;
        LastAccessedBy = accessedBy;
    }

    public void CreateNewVersion(
        string fileName,
        string filePath,
        long fileSize,
        string uploadedBy,
        string versionNotes = "")
    {
        if (!IsCurrentVersion)
            throw new InvalidOperationException("Cannot create new version from non-current version.");

        // Mark current version as not current
        IsCurrentVersion = false;

        // Create new version (this would be handled by the calling service)
        VersionNotes = $"Replaced by new version uploaded by {uploadedBy}. {versionNotes}".Trim();
    }

    public void Archive(string archivedBy, string reason = "")
    {
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
        Description += $"\nArchived by {archivedBy}: {reason}";
    }

    public void RestoreFromArchive(string restoredBy)
    {
        IsArchived = false;
        ArchivedAt = null;
        Description += $"\nRestored from archive by {restoredBy}";
    }

    public void SetVirusScanResult(bool isClean, DateTime scanDate)
    {
        IsVirusScanned = true;
        IsVirusClean = isClean;
        VirusScanDate = scanDate;
    }

    public void SetFileHashes(string md5Hash, string sha256Hash)
    {
        ChecksumMD5 = md5Hash;
        ChecksumSHA256 = sha256Hash;
    }

    public void AddDigitalSignature(string signatureInfo)
    {
        HasDigitalSignature = true;
        SignatureInfo = signatureInfo;
    }

    public bool CanBeAccessedBy(string userRole, bool isInstructor, bool isParticipant)
    {
        if (IsArchived)
            return false;

        if (!IsVirusClean)
            return false;

        if (RequiresApproval && !IsApproved)
            return isInstructor; // Only instructors can access unapproved documents

        if (IsInstructorOnly)
            return isInstructor;

        if (IsPublic)
            return true;

        if (IsParticipantSubmission)
            return isInstructor; // Only instructors can see participant submissions

        // Default: accessible by both instructors and participants
        return isInstructor || isParticipant;
    }

    public string GetFileExtension()
    {
        return Path.GetExtension(OriginalFileName).ToLowerInvariant();
    }

    public bool IsImageFile()
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
        return imageExtensions.Contains(GetFileExtension());
    }

    public bool IsVideoFile()
    {
        var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv" };
        return videoExtensions.Contains(GetFileExtension());
    }

    public bool IsDocumentFile()
    {
        var documentExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf" };
        return documentExtensions.Contains(GetFileExtension());
    }

    public string GetFileSizeFormatted()
    {
        var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
        var order = 0;
        var size = (double)FileSize;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }

    public bool RequiresSpecialHandling()
    {
        return IsComplianceDocument || IsK3Document || RequiresApproval || HasDigitalSignature;
    }
}