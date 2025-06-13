using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class HazardType : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int? CategoryId { get; private set; }
    public decimal RiskMultiplier { get; private set; } = 1.0m;
    public bool RequiresPermit { get; private set; } = false;
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public HazardCategory? Category { get; private set; }

    private readonly List<Hazard> _hazards = new();
    public IReadOnlyCollection<Hazard> Hazards => _hazards.AsReadOnly();

    // Audit properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private HazardType() { } // EF Core

    public static HazardType Create(
        string name,
        string code,
        string? description = null,
        int? categoryId = null,
        decimal riskMultiplier = 1.0m,
        bool requiresPermit = false,
        int displayOrder = 0)
    {
        return new HazardType
        {
            Name = name,
            Code = code,
            Description = description,
            CategoryId = categoryId,
            RiskMultiplier = riskMultiplier,
            RequiresPermit = requiresPermit,
            DisplayOrder = displayOrder,
            IsActive = true
        };
    }

    public void Update(
        string name,
        string code,
        string? description = null,
        int? categoryId = null,
        decimal riskMultiplier = 1.0m,
        bool requiresPermit = false,
        int displayOrder = 0)
    {
        Name = name;
        Code = code;
        Description = description;
        CategoryId = categoryId;
        RiskMultiplier = riskMultiplier;
        RequiresPermit = requiresPermit;
        DisplayOrder = displayOrder;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}