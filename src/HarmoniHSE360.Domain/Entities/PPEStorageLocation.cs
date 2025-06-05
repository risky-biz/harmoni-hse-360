using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class PPEStorageLocation : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Capacity { get; private set; }
    public int CurrentStock { get; private set; }

    // Navigation properties
    public virtual ICollection<PPEItem> PPEItems { get; private set; } = new List<PPEItem>();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected PPEStorageLocation() { } // For EF Core

    public static PPEStorageLocation Create(
        string name,
        string code,
        string createdBy,
        string? description = null,
        string? address = null,
        string? contactPerson = null,
        string? contactPhone = null,
        int capacity = 1000)
    {
        return new PPEStorageLocation
        {
            Name = name,
            Code = code,
            Description = description,
            Address = address,
            ContactPerson = contactPerson,
            ContactPhone = contactPhone,
            Capacity = capacity,
            CurrentStock = 0,
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
        string? address = null,
        string? contactPerson = null,
        string? contactPhone = null,
        int capacity = 1000)
    {
        Name = name;
        Code = code;
        Description = description;
        Address = address;
        ContactPerson = contactPerson;
        ContactPhone = contactPhone;
        Capacity = capacity;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void UpdateStock(int quantity, string modifiedBy)
    {
        CurrentStock = Math.Max(0, quantity);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void AddStock(int quantity, string modifiedBy)
    {
        CurrentStock += quantity;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void RemoveStock(int quantity, string modifiedBy)
    {
        CurrentStock = Math.Max(0, CurrentStock - quantity);
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

    public bool HasCapacity(int quantity = 1)
    {
        return CurrentStock + quantity <= Capacity;
    }

    public decimal UtilizationPercentage => Capacity > 0 ? ((decimal)CurrentStock / Capacity) * 100 : 0;
}