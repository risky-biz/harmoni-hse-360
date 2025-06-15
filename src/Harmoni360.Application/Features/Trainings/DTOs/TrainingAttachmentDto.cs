namespace Harmoni360.Application.Features.Trainings.DTOs;

public class TrainingAttachmentDto
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    // Training Material Properties
    public bool IsTrainingMaterial { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDownloadable { get; set; }
    public int DownloadCount { get; set; }
    
    // Version Control
    public int Version { get; set; }
    public bool IsLatestVersion { get; set; }
    public int? ParentAttachmentId { get; set; }
    public string? VersionNotes { get; set; }
    
    // Access Control
    public bool RequiresApproval { get; set; }
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    
    // Metadata
    public string? Tags { get; set; }
    public string? Language { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? DocumentDate { get; set; }
    
    // Computed Properties
    public string FormattedFileSize => FormatFileSize(FileSize);
    public string FileExtension => Path.GetExtension(OriginalFileName).ToLowerInvariant();
    public bool IsImage => IsImageFile(ContentType);
    public bool IsDocument => IsDocumentFile(ContentType);
    public bool IsVideo => IsVideoFile(ContentType);
    public string AttachmentTypeIcon => GetAttachmentTypeIcon(AttachmentType);
    
    private static string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 Bytes";
        string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };
        int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024));
        return Math.Round(bytes / Math.Pow(1024, i), 2) + " " + sizes[i];
    }
    
    private static bool IsImageFile(string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
    
    private static bool IsDocumentFile(string contentType)
    {
        return contentType.Contains("pdf") || 
               contentType.Contains("document") || 
               contentType.Contains("spreadsheet") || 
               contentType.Contains("presentation") ||
               contentType.Contains("text");
    }
    
    private static bool IsVideoFile(string contentType)
    {
        return contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    }
    
    private static string GetAttachmentTypeIcon(string attachmentType)
    {
        return attachmentType switch
        {
            "CourseOutline" => "fa-list-alt",
            "Presentation" => "fa-presentation",
            "Handbook" => "fa-book",
            "Video" => "fa-video",
            "Assessment" => "fa-clipboard-check",
            "Certificate" => "fa-certificate",
            "HandoutMaterial" => "fa-file-alt",
            "ReferenceDocument" => "fa-file-text",
            "SafetyDataSheet" => "fa-shield-alt",
            "OperatingProcedure" => "fa-cogs",
            "K3Regulation" => "fa-balance-scale",
            "ComplianceDocument" => "fa-stamp",
            "PhotoEvidence" => "fa-camera",
            "AttendanceSheet" => "fa-users",
            "EvaluationForm" => "fa-poll",
            _ => "fa-file"
        };
    }
}