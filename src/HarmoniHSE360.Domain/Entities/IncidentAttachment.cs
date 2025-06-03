using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class IncidentAttachment : BaseEntity
{
    public int IncidentId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string UploadedBy { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }

    protected IncidentAttachment() { } // For EF Core

    public IncidentAttachment(int incidentId, string fileName, string filePath, long fileSize, string uploadedBy)
    {
        IncidentId = incidentId;
        FileName = fileName;
        FilePath = filePath;
        FileSize = fileSize;
        UploadedBy = uploadedBy;
        UploadedAt = DateTime.UtcNow;
    }
}