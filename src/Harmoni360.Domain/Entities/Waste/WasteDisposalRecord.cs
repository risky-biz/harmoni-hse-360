using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Waste;

public class WasteDisposalRecord : BaseEntity, IAuditableEntity
{
    public int WasteReportId { get; private set; }
    public WasteReport WasteReport { get; private set; } = null!;
    public int DisposalProviderId { get; private set; }
    public DisposalProvider DisposalProvider { get; private set; } = null!;
    public DateTime DisposalDate { get; private set; }
    public decimal ActualQuantity { get; private set; }
    public UnitOfMeasure Unit { get; private set; }
    public DisposalStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected WasteDisposalRecord() { }

    public static WasteDisposalRecord Create(int wasteReportId, int disposalProviderId, DateTime disposalDate, decimal quantity, UnitOfMeasure unit, string createdBy)
    {
        return new WasteDisposalRecord
        {
            WasteReportId = wasteReportId,
            DisposalProviderId = disposalProviderId,
            DisposalDate = disposalDate,
            ActualQuantity = quantity,
            Unit = unit,
            Status = DisposalStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void UpdateStatus(DisposalStatus status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
    }
}
