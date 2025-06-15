using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class IncidentCategory : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Color { get; private set; } = "#007bff"; // Default blue color
    public string Icon { get; private set; } = "fa-exclamation-triangle"; // Default icon
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }
    public bool RequiresImmediateAction { get; private set; } = false;

    // Navigation properties
    private readonly List<Incident> _incidents = new();
    public IReadOnlyCollection<Incident> Incidents => _incidents.AsReadOnly();

    // Audit properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private IncidentCategory() { } // EF Core

    public static IncidentCategory Create(
        string name,
        string code,
        string? description = null,
        string color = "#007bff",
        string icon = "fa-exclamation-triangle",
        int displayOrder = 0,
        bool requiresImmediateAction = false,
        bool isActive = true)
    {
        return new IncidentCategory
        {
            Name = name,
            Code = code,
            Description = description,
            Color = color,
            Icon = icon,
            DisplayOrder = displayOrder,
            RequiresImmediateAction = requiresImmediateAction,
            IsActive = isActive
        };
    }

    public void Update(
        string name,
        string code,
        string? description = null,
        string color = "#007bff",
        string icon = "fa-exclamation-triangle",
        int displayOrder = 0,
        bool requiresImmediateAction = false,
        bool isActive = true)
    {
        Name = name;
        Code = code;
        Description = description;
        Color = color;
        Icon = icon;
        DisplayOrder = displayOrder;
        RequiresImmediateAction = requiresImmediateAction;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}