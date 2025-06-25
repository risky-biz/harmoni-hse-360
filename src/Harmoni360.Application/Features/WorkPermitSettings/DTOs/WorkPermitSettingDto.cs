using Harmoni360.Application.Features.WorkPermitSettings.DTOs;

namespace Harmoni360.Application.Features.WorkPermitSettings.DTOs;

public class WorkPermitSettingDto
{
    public int Id { get; set; }
    public bool RequireSafetyInduction { get; set; }
    public bool EnableFormValidation { get; set; }
    public bool AllowAttachments { get; set; }
    public int MaxAttachmentSizeMB { get; set; }
    public string? FormInstructions { get; set; }
    public bool IsActive { get; set; }

    // Related Safety Videos
    public List<WorkPermitSafetyVideoDto> SafetyVideos { get; set; } = new();

    // Computed Properties
    public WorkPermitSafetyVideoDto? ActiveSafetyVideo => SafetyVideos.FirstOrDefault(v => v.IsActive);
    public bool IsSafetyInductionConfigured => RequireSafetyInduction && ActiveSafetyVideo != null;
    public bool IsConfigurationComplete => !RequireSafetyInduction || IsSafetyInductionConfigured;

    // Audit Properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

