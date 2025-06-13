using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class Department : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? HeadOfDepartment { get; private set; }
    public string? Contact { get; private set; }
    public string? Location { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    // Navigation properties
    private readonly List<Incident> _incidents = new();
    public IReadOnlyCollection<Incident> Incidents => _incidents.AsReadOnly();

    private readonly List<CorrectiveAction> _correctiveActions = new();
    public IReadOnlyCollection<CorrectiveAction> CorrectiveActions => _correctiveActions.AsReadOnly();

    // Audit properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private Department() { } // EF Core

    public static Department Create(
        string name,
        string code,
        string? description = null,
        string? headOfDepartment = null,
        string? contact = null,
        string? location = null,
        int displayOrder = 0,
        bool isActive = true)
    {
        return new Department
        {
            Name = name,
            Code = code,
            Description = description,
            HeadOfDepartment = headOfDepartment,
            Contact = contact,
            Location = location,
            DisplayOrder = displayOrder,
            IsActive = isActive
        };
    }

    public void Update(
        string name,
        string code,
        string? description = null,
        string? headOfDepartment = null,
        string? contact = null,
        string? location = null,
        int displayOrder = 0,
        bool isActive = true)
    {
        Name = name;
        Code = code;
        Description = description;
        HeadOfDepartment = headOfDepartment;
        Contact = contact;
        Location = location;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}