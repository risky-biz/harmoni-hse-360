using Harmoni360.Domain.Common;
using Harmoni360.Domain.Events.WorkPermitSettings;

namespace Harmoni360.Domain.Entities;

/// <summary>
/// Work Permit Settings aggregate root for managing form configuration including safety induction videos
/// </summary>
public class WorkPermitSettings : BaseEntity, IAuditableEntity
{
    private readonly List<WorkPermitSafetyVideo> _safetyVideos = new();

    private WorkPermitSettings() { } // For EF Core

    private WorkPermitSettings(
        bool requireSafetyInduction,
        bool enableFormValidation,
        bool allowAttachments,
        int maxAttachmentSize,
        string? formInstructions,
        string createdBy)
    {
        RequireSafetyInduction = requireSafetyInduction;
        EnableFormValidation = enableFormValidation;
        AllowAttachments = allowAttachments;
        MaxAttachmentSizeMB = maxAttachmentSize;
        FormInstructions = formInstructions;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;

        AddDomainEvent(new WorkPermitSettingsCreatedEvent(this));
    }

    /// <summary>
    /// Whether safety induction video is required before form submission
    /// </summary>
    public bool RequireSafetyInduction { get; private set; }

    /// <summary>
    /// Whether form validation is enabled
    /// </summary>
    public bool EnableFormValidation { get; private set; }

    /// <summary>
    /// Whether attachments are allowed on work permit forms
    /// </summary>
    public bool AllowAttachments { get; private set; }

    /// <summary>
    /// Maximum attachment size in MB
    /// </summary>
    public int MaxAttachmentSizeMB { get; private set; }

    /// <summary>
    /// Optional instructions displayed on the form
    /// </summary>
    public string? FormInstructions { get; private set; }

    /// <summary>
    /// Whether these settings are active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Current active safety induction video
    /// </summary>
    public WorkPermitSafetyVideo? ActiveSafetyVideo => _safetyVideos.FirstOrDefault(v => v.IsActive);

    /// <summary>
    /// All safety videos (for history/versioning)
    /// </summary>
    public IReadOnlyCollection<WorkPermitSafetyVideo> SafetyVideos => _safetyVideos.AsReadOnly();

    /// <summary>
    /// Whether safety induction is properly configured
    /// </summary>
    public bool IsSafetyInductionConfigured => RequireSafetyInduction && ActiveSafetyVideo != null;

    /// <summary>
    /// Whether the configuration is complete and ready for use
    /// </summary>
    public bool IsConfigurationComplete => IsSafetyInductionConfigured || !RequireSafetyInduction;

    // Audit properties
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    /// <summary>
    /// Factory method to create new Work Permit Settings
    /// </summary>
    public static WorkPermitSettings Create(
        bool requireSafetyInduction = true,
        bool enableFormValidation = true,
        bool allowAttachments = true,
        int maxAttachmentSize = 10,
        string? formInstructions = null,
        string createdBy = "System")
    {
        if (maxAttachmentSize <= 0 || maxAttachmentSize > 100)
            throw new ArgumentException("Max attachment size must be between 1 and 100 MB", nameof(maxAttachmentSize));

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by cannot be empty", nameof(createdBy));

        return new WorkPermitSettings(
            requireSafetyInduction,
            enableFormValidation,
            allowAttachments,
            maxAttachmentSize,
            formInstructions?.Trim(),
            createdBy);
    }

    /// <summary>
    /// Update form configuration settings
    /// </summary>
    public void UpdateFormConfiguration(
        bool requireSafetyInduction,
        bool enableFormValidation,
        bool allowAttachments,
        int maxAttachmentSize,
        string? formInstructions,
        string modifiedBy)
    {
        if (maxAttachmentSize <= 0 || maxAttachmentSize > 100)
            throw new ArgumentException("Max attachment size must be between 1 and 100 MB", nameof(maxAttachmentSize));

        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new ArgumentException("Modified by cannot be empty", nameof(modifiedBy));

        var hasChanges = RequireSafetyInduction != requireSafetyInduction ||
                        EnableFormValidation != enableFormValidation ||
                        AllowAttachments != allowAttachments ||
                        MaxAttachmentSizeMB != maxAttachmentSize ||
                        FormInstructions != formInstructions?.Trim();

        if (!hasChanges) return;

        RequireSafetyInduction = requireSafetyInduction;
        EnableFormValidation = enableFormValidation;
        AllowAttachments = allowAttachments;
        MaxAttachmentSizeMB = maxAttachmentSize;
        FormInstructions = formInstructions?.Trim();
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new WorkPermitSettingsUpdatedEvent(this));
    }

    /// <summary>
    /// Add or replace safety induction video
    /// </summary>
    public void SetSafetyInductionVideo(
        string fileName,
        string filePath,
        long fileSize,
        string contentType,
        TimeSpan duration,
        string uploadedBy)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));

        if (string.IsNullOrWhiteSpace(uploadedBy))
            throw new ArgumentException("Uploaded by cannot be empty", nameof(uploadedBy));

        // Deactivate existing video
        var existingVideo = ActiveSafetyVideo;
        if (existingVideo != null)
        {
            existingVideo.Deactivate(uploadedBy);
        }

        // Add new video
        var newVideo = WorkPermitSafetyVideo.Create(
            Id, fileName, filePath, fileSize, contentType, duration, uploadedBy);

        _safetyVideos.Add(newVideo);

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = uploadedBy;

        AddDomainEvent(new SafetyInductionVideoUploadedEvent(this, newVideo));
    }

    /// <summary>
    /// Remove safety induction video
    /// </summary>
    public void RemoveSafetyInductionVideo(string removedBy)
    {
        if (string.IsNullOrWhiteSpace(removedBy))
            throw new ArgumentException("Removed by cannot be empty", nameof(removedBy));

        var activeVideo = ActiveSafetyVideo;
        if (activeVideo == null)
            throw new InvalidOperationException("No active safety video to remove");

        activeVideo.Deactivate(removedBy);

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = removedBy;

        AddDomainEvent(new SafetyInductionVideoRemovedEvent(this, activeVideo));
    }

    /// <summary>
    /// Activate these settings
    /// </summary>
    public void Activate(string activatedBy)
    {
        if (string.IsNullOrWhiteSpace(activatedBy))
            throw new ArgumentException("Activated by cannot be empty", nameof(activatedBy));

        if (IsActive) return;

        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = activatedBy;

        AddDomainEvent(new WorkPermitSettingsActivatedEvent(this));
    }

    /// <summary>
    /// Deactivate these settings
    /// </summary>
    public void Deactivate(string deactivatedBy)
    {
        if (string.IsNullOrWhiteSpace(deactivatedBy))
            throw new ArgumentException("Deactivated by cannot be empty", nameof(deactivatedBy));

        if (!IsActive) return;

        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = deactivatedBy;

        AddDomainEvent(new WorkPermitSettingsDeactivatedEvent(this));
    }
}