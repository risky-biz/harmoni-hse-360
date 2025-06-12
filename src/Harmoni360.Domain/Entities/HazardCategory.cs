using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class HazardCategory : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Color { get; private set; } = "#fd7e14"; // Default warning color
    public string RiskLevel { get; private set; } = "Medium"; // Low, Medium, High, Critical
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    // Navigation properties
    private readonly List<Hazard> _hazards = new();
    public IReadOnlyCollection<Hazard> Hazards => _hazards.AsReadOnly();

    private readonly List<HazardType> _hazardTypes = new();
    public IReadOnlyCollection<HazardType> HazardTypes => _hazardTypes.AsReadOnly();

    // Audit properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private HazardCategory() { } // EF Core

    public static HazardCategory Create(
        string name,
        string code,
        string? description = null,
        string color = "#fd7e14",
        string riskLevel = "Medium",
        int displayOrder = 0)
    {
        return new HazardCategory
        {
            Name = name,
            Code = code,
            Description = description,
            Color = color,
            RiskLevel = riskLevel,
            DisplayOrder = displayOrder,
            IsActive = true
        };
    }

    public void Update(
        string name,
        string code,
        string? description = null,
        string color = "#fd7e14",
        string riskLevel = "Medium",
        int displayOrder = 0)
    {
        Name = name;
        Code = code;
        Description = description;
        Color = color;
        RiskLevel = riskLevel;
        DisplayOrder = displayOrder;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}