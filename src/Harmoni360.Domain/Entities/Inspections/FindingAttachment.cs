using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities.Inspections;

public class FindingAttachment : BaseEntity, IAuditableEntity
{
    public int FindingId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsPhoto { get; private set; }
    public string? ThumbnailPath { get; private set; }

    // Navigation Properties
    public virtual InspectionFinding Finding { get; private set; } = null!;

    // IAuditableEntity
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private FindingAttachment() { }

    public static FindingAttachment Create(
        int findingId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string filePath,
        string? description = null)
    {
        var isPhoto = IsImageContentType(contentType);
        
        return new FindingAttachment
        {
            FindingId = findingId,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            Description = description,
            IsPhoto = isPhoto,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDescription(string description)
    {
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetThumbnailPath(string thumbnailPath)
    {
        if (!IsPhoto)
            throw new InvalidOperationException("Thumbnails can only be set for photos");

        ThumbnailPath = thumbnailPath;
        LastModifiedAt = DateTime.UtcNow;
    }

    private static bool IsImageContentType(string contentType)
    {
        var imageTypes = new[] 
        { 
            "image/jpeg", 
            "image/jpg", 
            "image/png", 
            "image/gif", 
            "image/bmp", 
            "image/webp" 
        };
        
        return imageTypes.Contains(contentType.ToLowerInvariant());
    }

    public string GetFileSizeFormatted()
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = FileSize;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }

    public bool IsDocument => !IsPhoto;
    public string FileExtension => Path.GetExtension(OriginalFileName);
}