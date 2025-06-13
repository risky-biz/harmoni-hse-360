using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Security;

/// <summary>
/// Represents an attachment to a security incident
/// </summary>
public class SecurityIncidentAttachment : BaseEntity
{
    public int SecurityIncidentId { get; private set; }
    public SecurityIncident SecurityIncident { get; private set; } = null!;
    
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string FileType { get; private set; } = string.Empty;
    public SecurityAttachmentType AttachmentType { get; private set; }
    public string? Description { get; private set; }
    public bool IsConfidential { get; private set; }
    
    // Metadata for integrity and security
    public string? Hash { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string UploadedBy { get; private set; } = string.Empty;
    
    // Constructor for EF Core
    protected SecurityIncidentAttachment() { }
    
    // Factory method
    public static SecurityIncidentAttachment Create(
        int securityIncidentId,
        string fileName,
        string filePath,
        long fileSize,
        SecurityAttachmentType attachmentType,
        string uploadedBy,
        string? description = null,
        bool isConfidential = true)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));
        
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));
        
        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));
        
        if (string.IsNullOrWhiteSpace(uploadedBy))
            throw new ArgumentException("Uploaded by is required", nameof(uploadedBy));
        
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var fileType = DetermineFileType(fileExtension);
        
        return new SecurityIncidentAttachment
        {
            SecurityIncidentId = securityIncidentId,
            FileName = fileName,
            FilePath = filePath,
            FileSize = fileSize,
            FileType = fileType,
            AttachmentType = attachmentType,
            Description = description,
            IsConfidential = isConfidential,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };
    }
    
    // Business Methods
    public void SetHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty", nameof(hash));
        
        Hash = hash;
    }
    
    public void UpdateDescription(string description)
    {
        Description = description;
    }
    
    public void UpdateConfidentiality(bool isConfidential)
    {
        IsConfidential = isConfidential;
    }
    
    // Calculated Properties
    public string FileSizeFormatted => FormatFileSize(FileSize);
    
    public bool IsImage => FileType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    
    public bool IsDocument => FileType.Contains("document") || 
                             FileType.Contains("pdf") || 
                             FileType.Contains("text");
    
    // Private Methods
    private static string DetermineFileType(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".log" => "text/plain",
            ".xml" => "application/xml",
            ".json" => "application/json",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream"
        };
    }
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
}