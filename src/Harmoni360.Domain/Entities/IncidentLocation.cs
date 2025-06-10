using Harmoni360.Domain.Common;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Domain.Entities;

public class IncidentLocation : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Building { get; private set; }
    public string? Floor { get; private set; }
    public string? Room { get; private set; }
    public GeoLocation? GeoLocation { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }
    public bool IsHighRisk { get; private set; } = false;

    // Navigation properties
    private readonly List<Incident> _incidents = new();
    public IReadOnlyCollection<Incident> Incidents => _incidents.AsReadOnly();

    // Audit properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private IncidentLocation() { } // EF Core

    public static IncidentLocation Create(
        string name,
        string code,
        string? description = null,
        string? building = null,
        string? floor = null,
        string? room = null,
        GeoLocation? geoLocation = null,
        int displayOrder = 0,
        bool isHighRisk = false)
    {
        return new IncidentLocation
        {
            Name = name,
            Code = code,
            Description = description,
            Building = building,
            Floor = floor,
            Room = room,
            GeoLocation = geoLocation,
            DisplayOrder = displayOrder,
            IsHighRisk = isHighRisk,
            IsActive = true
        };
    }

    public void Update(
        string name,
        string code,
        string? description = null,
        string? building = null,
        string? floor = null,
        string? room = null,
        GeoLocation? geoLocation = null,
        int displayOrder = 0,
        bool isHighRisk = false)
    {
        Name = name;
        Code = code;
        Description = description;
        Building = building;
        Floor = floor;
        Room = room;
        GeoLocation = geoLocation;
        DisplayOrder = displayOrder;
        IsHighRisk = isHighRisk;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public string GetFullLocation()
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(Building))
            parts.Add($"Building {Building}");
        
        if (!string.IsNullOrEmpty(Floor))
            parts.Add($"Floor {Floor}");
        
        if (!string.IsNullOrEmpty(Room))
            parts.Add($"Room {Room}");

        return parts.Any() ? string.Join(", ", parts) : Name;
    }
}