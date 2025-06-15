namespace Harmoni360.Application.Features.Inspections.DTOs;

public class InspectionAttachmentDto
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsPhoto { get; set; }
    public string? ThumbnailPath { get; set; }
    public bool IsDocument { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }
}