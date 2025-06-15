using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Waste;

public class WasteCompliance : BaseEntity, IAuditableEntity
{
    public string RegulatoryBody { get; private set; } = string.Empty;
    public string RegulationCode { get; private set; } = string.Empty;
    public string RegulationName { get; private set; } = string.Empty;
    public ComplianceStatus Status { get; private set; } = ComplianceStatus.Compliant;
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected WasteCompliance() { }

    public static WasteCompliance Create(string body, string code, string name, string createdBy)
    {
        return new WasteCompliance
        {
            RegulatoryBody = body,
            RegulationCode = code,
            RegulationName = name,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void ChangeStatus(ComplianceStatus status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
    }
}
