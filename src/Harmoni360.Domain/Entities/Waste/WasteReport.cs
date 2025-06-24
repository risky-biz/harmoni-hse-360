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
    
    // Additional fields for comprehensive waste reporting
    public decimal? EstimatedQuantity { get; private set; }
    public string? QuantityUnit { get; private set; }
    public string? DisposalMethod { get; private set; }
    public DateTime? DisposalDate { get; private set; }
    public string? DisposedBy { get; private set; }
    public decimal? DisposalCost { get; private set; }
    public string? ContractorName { get; private set; }
    public string? ManifestNumber { get; private set; }
    public string? Treatment { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<WasteAttachment> _attachments = new();
    public IReadOnlyCollection<WasteAttachment> Attachments => _attachments.AsReadOnly();

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected WasteReport() { }

    public static WasteReport Create(string title, string description, WasteCategory category, DateTime generatedDate, string location, int? reporterId, string createdBy,
        decimal? estimatedQuantity = null, string? quantityUnit = null, string? disposalMethod = null, DateTime? disposalDate = null,
        decimal? disposalCost = null, string? contractorName = null, string? manifestNumber = null, string? treatment = null, string? notes = null, string? disposedBy = null)
    {
        return new WasteReport
        {
            Title = title,
            Description = description,
            Category = category,
            GeneratedDate = generatedDate,
            Location = location,
            ReporterId = reporterId,
            EstimatedQuantity = estimatedQuantity,
            QuantityUnit = quantityUnit,
            DisposalMethod = disposalMethod,
            DisposalDate = disposalDate,
            DisposalCost = disposalCost,
            ContractorName = contractorName,
            ManifestNumber = manifestNumber,
            Treatment = treatment,
            Notes = notes,
            DisposedBy = disposedBy,
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
