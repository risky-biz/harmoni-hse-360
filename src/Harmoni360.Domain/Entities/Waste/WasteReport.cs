using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities.Waste;

public class WasteReport : BaseEntity, IAuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WasteCategory Category { get; private set; }
    public DateTime GeneratedDate { get; private set; }
    public string Location { get; private set; } = string.Empty;

    public WasteDisposalStatus DisposalStatus { get; private set; } = WasteDisposalStatus.Pending;

    public int? ReporterId { get; private set; }
    public User? Reporter { get; private set; }

    private readonly List<WasteAttachment> _attachments = new();
    public IReadOnlyCollection<WasteAttachment> Attachments => _attachments.AsReadOnly();

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected WasteReport() { }

    public static WasteReport Create(string title, string description, WasteCategory category, DateTime generatedDate, string location, int? reporterId, string createdBy)
    {
        return new WasteReport
        {
            Title = title,
            Description = description,
            Category = category,
            GeneratedDate = generatedDate,
            Location = location,
            ReporterId = reporterId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void AddAttachment(string fileName, string filePath, long fileSize, string uploadedBy)
    {
        _attachments.Add(new WasteAttachment(Id, fileName, filePath, fileSize, uploadedBy));
    }

    public void UpdateDisposalStatus(WasteDisposalStatus status)
    {
        DisposalStatus = status;
    }
}
