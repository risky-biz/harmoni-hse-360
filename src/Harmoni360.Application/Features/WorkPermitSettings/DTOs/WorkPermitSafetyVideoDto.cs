namespace Harmoni360.Application.Features.WorkPermitSettings.DTOs;

public class WorkPermitSafetyVideoDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? Resolution { get; set; }
    public int? Bitrate { get; set; }
    
    // Computed Properties
    public decimal FileSizeMB => Math.Round(FileSize / 1024m / 1024m, 2);
    public string DurationFormatted => Duration.ToString(@"mm\:ss");
    public bool IsSupportedFormat
    {
        get
        {
            var supportedTypes = new[] { "video/mp4", "video/webm", "video/avi" };
            return supportedTypes.Contains(ContentType.ToLowerInvariant());
        }
    }
    
    // Audit Properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Work Permit Settings ID
    public int WorkPermitSettingsId { get; set; }
}