using Harmoni360.Domain.Common;
using Harmoni360.Domain.Events.WorkPermitSettings;

namespace Harmoni360.Domain.Entities;

/// <summary>
/// Represents a safety induction video for work permit forms
/// </summary>
public class WorkPermitSafetyVideo : BaseEntity, IAuditableEntity
{
    private WorkPermitSafetyVideo() { } // For EF Core

    private WorkPermitSafetyVideo(
        int workPermitSettingsId,
        string fileName,
        string filePath,
        long fileSize,
        string contentType,
        TimeSpan duration,
        string uploadedBy)
    {
        WorkPermitSettingsId = workPermitSettingsId;
        FileName = fileName;
        OriginalFileName = fileName;
        FilePath = filePath;
        FileSize = fileSize;
        ContentType = contentType;
        Duration = duration;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = uploadedBy;

        AddDomainEvent(new SafetyVideoCreatedEvent(this));
    }

    /// <summary>
    /// Current file name (may be different from original if renamed)
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// Original file name when uploaded
    /// </summary>
    public string OriginalFileName { get; private set; } = string.Empty;

    /// <summary>
    /// File path in storage system
    /// </summary>
    public string FilePath { get; private set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// MIME content type (e.g., video/mp4, video/webm)
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// Video duration
    /// </summary>
    public TimeSpan Duration { get; private set; }

    /// <summary>
    /// Whether this video is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Optional description or notes about the video
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Video thumbnail path (if generated)
    /// </summary>
    public string? ThumbnailPath { get; private set; }

    /// <summary>
    /// Video resolution (e.g., "1920x1080")
    /// </summary>
    public string? Resolution { get; private set; }

    /// <summary>
    /// Video bitrate in kbps
    /// </summary>
    public int? Bitrate { get; private set; }

    /// <summary>
    /// Foreign key to WorkPermitSettings
    /// </summary>
    public int WorkPermitSettingsId { get; private set; }

    // Audit properties
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    /// <summary>
    /// File size in MB (computed property)
    /// </summary>
    public double FileSizeMB => Math.Round(FileSize / (1024.0 * 1024.0), 2);

    /// <summary>
    /// Duration in human-readable format
    /// </summary>
    public string DurationFormatted => Duration.ToString(@"hh\:mm\:ss");

    /// <summary>
    /// Whether the video is a supported format
    /// </summary>
    public bool IsSupportedFormat => SupportedFormats.Contains(ContentType.ToLowerInvariant());

    /// <summary>
    /// Supported video formats
    /// </summary>
    public static readonly HashSet<string> SupportedFormats = new()
    {
        "video/mp4",
        "video/webm",
        "video/avi",
        "video/quicktime", // .mov files
        "video/x-msvideo"  // alternative MIME type for .avi
    };

    /// <summary>
    /// Maximum allowed file size in bytes (100MB)
    /// </summary>
    public static readonly long MaxFileSizeBytes = 100 * 1024 * 1024;

    /// <summary>
    /// Factory method to create a new safety video
    /// </summary>
    public static WorkPermitSafetyVideo Create(
        int workPermitSettingsId,
        string fileName,
        string filePath,
        long fileSize,
        string contentType,
        TimeSpan duration,
        string uploadedBy)
    {
        if (workPermitSettingsId <= 0)
            throw new ArgumentException("Work permit settings ID must be greater than zero", nameof(workPermitSettingsId));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));

        if (fileSize > MaxFileSizeBytes)
            throw new ArgumentException($"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB", nameof(fileSize));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type cannot be empty", nameof(contentType));

        if (!SupportedFormats.Contains(contentType.ToLowerInvariant()))
            throw new ArgumentException($"Unsupported content type: {contentType}", nameof(contentType));

        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be greater than zero", nameof(duration));

        if (string.IsNullOrWhiteSpace(uploadedBy))
            throw new ArgumentException("Uploaded by cannot be empty", nameof(uploadedBy));

        return new WorkPermitSafetyVideo(workPermitSettingsId, fileName, filePath, fileSize, contentType, duration, uploadedBy);
    }

    /// <summary>
    /// Update video metadata
    /// </summary>
    public void UpdateMetadata(
        string? description = null,
        string? thumbnailPath = null,
        string? resolution = null,
        int? bitrate = null,
        string modifiedBy = "System")
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new ArgumentException("Modified by cannot be empty", nameof(modifiedBy));

        var hasChanges = Description != description ||
                        ThumbnailPath != thumbnailPath ||
                        Resolution != resolution ||
                        Bitrate != bitrate;

        if (!hasChanges) return;

        Description = description?.Trim();
        ThumbnailPath = thumbnailPath?.Trim();
        Resolution = resolution?.Trim();
        Bitrate = bitrate;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new SafetyVideoUpdatedEvent(this));
    }

    /// <summary>
    /// Rename the video file
    /// </summary>
    public void Rename(string newFileName, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(newFileName))
            throw new ArgumentException("New file name cannot be empty", nameof(newFileName));

        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new ArgumentException("Modified by cannot be empty", nameof(modifiedBy));

        if (FileName == newFileName.Trim()) return;

        FileName = newFileName.Trim();
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new SafetyVideoUpdatedEvent(this));
    }

    /// <summary>
    /// Deactivate this video
    /// </summary>
    public void Deactivate(string deactivatedBy)
    {
        if (string.IsNullOrWhiteSpace(deactivatedBy))
            throw new ArgumentException("Deactivated by cannot be empty", nameof(deactivatedBy));

        if (!IsActive) return;

        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = deactivatedBy;

        AddDomainEvent(new SafetyVideoDeactivatedEvent(this));
    }

    /// <summary>
    /// Activate this video
    /// </summary>
    public void Activate(string activatedBy)
    {
        if (string.IsNullOrWhiteSpace(activatedBy))
            throw new ArgumentException("Activated by cannot be empty", nameof(activatedBy));

        if (IsActive) return;

        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = activatedBy;

        AddDomainEvent(new SafetyVideoActivatedEvent(this));
    }

    /// <summary>
    /// Validate file extension matches content type
    /// </summary>
    public static bool IsValidFileExtension(string fileName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(contentType))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var lowerContentType = contentType.ToLowerInvariant();

        return lowerContentType switch
        {
            "video/mp4" => extension == ".mp4",
            "video/webm" => extension == ".webm",
            "video/avi" or "video/x-msvideo" => extension == ".avi",
            "video/quicktime" => extension == ".mov",
            _ => false
        };
    }
}