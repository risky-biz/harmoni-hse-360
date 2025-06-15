using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Waste;

public class WasteType : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public WasteClassification Classification { get; private set; }
    public bool IsRecyclable { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected WasteType() { }

    public static WasteType Create(string name, string code, WasteClassification classification, bool isRecyclable, string createdBy)
    {
        return new WasteType
        {
            Name = name,
            Code = code,
            Classification = classification,
            IsRecyclable = isRecyclable,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Update(string name, WasteClassification classification, bool isRecyclable)
    {
        Name = name;
        Classification = classification;
        IsRecyclable = isRecyclable;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }
}
