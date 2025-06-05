using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class HazardAttachment : BaseEntity
{
    public int HazardId { get; private set; }
    public Hazard Hazard { get; private set; } = null!;
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public string UploadedBy { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    protected HazardAttachment() { } // For EF Core

    public HazardAttachment(int hazardId, string fileName, string filePath, long fileSize, string uploadedBy, string? description = null)
    {
        HazardId = hazardId;
        FileName = fileName;
        FilePath = filePath;
        FileSize = fileSize;
        ContentType = GetContentTypeFromFileName(fileName);
        UploadedAt = DateTime.UtcNow;
        UploadedBy = uploadedBy;
        Description = description;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    private static string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".mp4" => "video/mp4",
            ".avi" => "video/avi",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream"
        };
    }
}