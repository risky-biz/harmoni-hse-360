using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class PPESize : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public virtual ICollection<PPEItem> PPEItems { get; private set; } = new List<PPEItem>();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected PPESize() { } // For EF Core

    public static PPESize Create(
        string name,
        string code,
        string createdBy,
        string? description = null,
        int sortOrder = 0)
    {
        return new PPESize
        {
            Name = name,
            Code = code,
            Description = description,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string name,
        string code,
        string modifiedBy,
        string? description = null,
        int sortOrder = 0)
    {
        Name = name;
        Code = code;
        Description = description;
        SortOrder = sortOrder;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Activate(string modifiedBy)
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Deactivate(string modifiedBy)
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}