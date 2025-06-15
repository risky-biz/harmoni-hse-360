using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities.Waste;

public class WasteAttachment : BaseEntity
{
    public int WasteReportId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string UploadedBy { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }

    protected WasteAttachment() { }

    public WasteAttachment(int wasteReportId, string fileName, string filePath, long fileSize, string uploadedBy)
    {
        WasteReportId = wasteReportId;
        FileName = fileName;
        FilePath = filePath;
        FileSize = fileSize;
        UploadedBy = uploadedBy;
        UploadedAt = DateTime.UtcNow;
    }
}
