using Harmoni360.Domain.Common;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Domain.Entities;

public class PPEItem : BaseEntity, IAuditableEntity
{
    public string ItemCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public PPECategory Category { get; private set; } = null!;
    public string Manufacturer { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int? SizeId { get; private set; }
    public PPESize? Size { get; private set; }
    public int? StorageLocationId { get; private set; }
    public PPEStorageLocation? StorageLocation { get; private set; }
    public string? Color { get; private set; }
    public PPECondition Condition { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public decimal Cost { get; private set; }
    public string Location { get; private set; } = string.Empty; // Legacy field for backward compatibility
    public int? AssignedToId { get; private set; }
    public User? AssignedTo { get; private set; }
    public DateTime? AssignedDate { get; private set; }
    public PPEStatus Status { get; private set; }
    public CertificationInfo? Certification { get; private set; }
    public MaintenanceSchedule? MaintenanceInfo { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public virtual ICollection<PPEInspection> Inspections { get; private set; } = new List<PPEInspection>();
    public virtual ICollection<PPEAssignment> AssignmentHistory { get; private set; } = new List<PPEAssignment>();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected PPEItem() { } // For EF Core

    public static PPEItem Create(
        string itemCode,
        string name,
        string description,
        int categoryId,
        string manufacturer,
        string model,
        DateTime purchaseDate,
        decimal cost,
        string createdBy,
        int? sizeId = null,
        int? storageLocationId = null,
        string? location = null,
        string? color = null,
        DateTime? expiryDate = null,
        CertificationInfo? certification = null,
        MaintenanceSchedule? maintenanceInfo = null,
        string? notes = null)
    {
        var item = new PPEItem
        {
            ItemCode = itemCode,
            Name = name,
            Description = description,
            CategoryId = categoryId,
            Manufacturer = manufacturer,
            Model = model,
            SizeId = sizeId,
            StorageLocationId = storageLocationId,
            Color = color,
            Condition = PPECondition.New,
            ExpiryDate = expiryDate,
            PurchaseDate = purchaseDate,
            Cost = cost,
            Location = location ?? string.Empty,
            Status = PPEStatus.Available,
            Certification = certification,
            MaintenanceInfo = maintenanceInfo,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        item.AddDomainEvent(new PPEItemCreatedEvent(item));

        return item;
    }

    public void UpdateDetails(
        string name,
        string description,
        string manufacturer,
        string model,
        string modifiedBy,
        int? sizeId = null,
        int? storageLocationId = null,
        string? location = null,
        string? color = null,
        DateTime? expiryDate = null,
        string? notes = null)
    {
        Name = name;
        Description = description;
        Manufacturer = manufacturer;
        Model = model;
        SizeId = sizeId;
        StorageLocationId = storageLocationId;
        Color = color;
        ExpiryDate = expiryDate;
        Location = location ?? Location; // Keep existing if not provided
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new PPEItemUpdatedEvent(this));
    }

    public void UpdateCondition(PPECondition condition, string modifiedBy)
    {
        var previousCondition = Condition;
        Condition = condition;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        // Auto-update status based on condition
        if (condition == PPECondition.Damaged || condition == PPECondition.Expired)
        {
            if (Status == PPEStatus.Assigned)
            {
                // If assigned, mark for return
                Status = PPEStatus.RequiresReturn;
            }
            else
            {
                Status = PPEStatus.OutOfService;
            }
        }
        else if (condition == PPECondition.Retired)
        {
            Status = PPEStatus.Retired;
        }

        AddDomainEvent(new PPEConditionChangedEvent(this, previousCondition, condition));
    }

    public void AssignTo(int userId, string assignedBy, string? purpose = null)
    {
        if (Status != PPEStatus.Available)
        {
            throw new InvalidOperationException($"Cannot assign PPE item with status {Status}");
        }

        if (IsExpired)
        {
            throw new InvalidOperationException("Cannot assign expired PPE item");
        }

        if (Condition == PPECondition.Damaged || Condition == PPECondition.Poor)
        {
            throw new InvalidOperationException($"Cannot assign PPE item in {Condition} condition");
        }

        AssignedToId = userId;
        AssignedDate = DateTime.UtcNow;
        Status = PPEStatus.Assigned;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = assignedBy;

        var assignment = PPEAssignment.Create(Id, userId, assignedBy, purpose);
        AssignmentHistory.Add(assignment);

        AddDomainEvent(new PPEItemAssignedEvent(this, userId, assignedBy));
    }

    public void Return(string returnedBy, PPECondition? newCondition = null, string? returnNotes = null)
    {
        if (Status != PPEStatus.Assigned && Status != PPEStatus.RequiresReturn)
        {
            throw new InvalidOperationException($"Cannot return PPE item with status {Status}");
        }

        var previousAssignedTo = AssignedToId;
        AssignedToId = null;
        AssignedDate = null;
        Status = PPEStatus.Available;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = returnedBy;

        if (newCondition.HasValue)
        {
            UpdateCondition(newCondition.Value, returnedBy);
        }

        // Update latest assignment record
        var latestAssignment = AssignmentHistory.Where(a => a.ReturnedDate == null).FirstOrDefault();
        latestAssignment?.Return(returnedBy, returnNotes);

        AddDomainEvent(new PPEItemReturnedEvent(this, previousAssignedTo, returnedBy));
    }

    public void MarkAsLost(string reportedBy, string? notes = null)
    {
        Status = PPEStatus.Lost;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = reportedBy;

        if (!string.IsNullOrEmpty(notes))
        {
            Notes = $"{Notes}\nLost: {notes} (Reported by {reportedBy} on {DateTime.UtcNow:yyyy-MM-dd})";
        }

        // Update latest assignment record if exists
        var latestAssignment = AssignmentHistory.Where(a => a.ReturnedDate == null).FirstOrDefault();
        latestAssignment?.MarkAsLost(reportedBy, notes);

        AddDomainEvent(new PPEItemLostEvent(this, reportedBy));
    }

    public void Retire(string retiredBy, string? reason = null)
    {
        if (Status == PPEStatus.Assigned)
        {
            throw new InvalidOperationException("Cannot retire assigned PPE item. Return it first.");
        }

        Status = PPEStatus.Retired;
        Condition = PPECondition.Retired;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = retiredBy;

        if (!string.IsNullOrEmpty(reason))
        {
            Notes = $"{Notes}\nRetired: {reason} (By {retiredBy} on {DateTime.UtcNow:yyyy-MM-dd})";
        }

        AddDomainEvent(new PPEItemRetiredEvent(this, retiredBy));
    }

    public void UpdateCertification(CertificationInfo? certification, string modifiedBy)
    {
        Certification = certification;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new PPECertificationUpdatedEvent(this));
    }

    public void UpdateMaintenanceSchedule(MaintenanceSchedule? maintenanceSchedule, string modifiedBy)
    {
        MaintenanceInfo = maintenanceSchedule;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void RecordMaintenance(DateTime maintenanceDate, string performedBy, string? notes = null)
    {
        if (MaintenanceInfo != null)
        {
            MaintenanceInfo = MaintenanceInfo.UpdateLastMaintenance(maintenanceDate);
        }

        Status = PPEStatus.Available;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = performedBy;

        if (!string.IsNullOrEmpty(notes))
        {
            Notes = $"{Notes}\nMaintenance: {notes} (By {performedBy} on {maintenanceDate:yyyy-MM-dd})";
        }

        AddDomainEvent(new PPEMaintenancePerformedEvent(this, performedBy, maintenanceDate));
    }

    public void AddInspection(PPEInspection inspection)
    {
        Inspections.Add(inspection);
        AddDomainEvent(new PPEInspectionRecordedEvent(this, inspection));
    }

    // Computed Properties
    public bool IsExpired => ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate.Value;

    public bool IsExpiringSoon(int daysWarning = 30) => 
        ExpiryDate.HasValue && DateTime.UtcNow.AddDays(daysWarning) > ExpiryDate.Value;

    public bool IsMaintenanceDue => MaintenanceInfo?.IsMaintenanceDue == true;

    public bool IsMaintenanceDueSoon(int daysWarning = 7) => 
        MaintenanceInfo?.IsMaintenanceDueSoon(daysWarning) == true;

    public bool IsCertificationExpired => Certification?.IsExpired == true;

    public bool IsCertificationExpiringSoon(int daysWarning = 30) => 
        Certification?.IsExpiringSoon(daysWarning) == true;

    public DateTime? LastInspectionDate => Inspections
        .OrderByDescending(i => i.InspectionDate)
        .FirstOrDefault()?.InspectionDate;

    public DateTime? NextInspectionDue => LastInspectionDate?.AddDays(Category?.InspectionIntervalDays ?? 365);

    public bool IsInspectionDue => NextInspectionDue.HasValue && DateTime.UtcNow >= NextInspectionDue.Value;
}

public enum PPECondition
{
    New = 1,
    Excellent = 2,
    Good = 3,
    Fair = 4,
    Poor = 5,
    Damaged = 6,
    Expired = 7,
    Retired = 8
}

public enum PPEStatus
{
    Available = 1,
    Assigned = 2,
    InMaintenance = 3,
    InInspection = 4,
    OutOfService = 5,
    RequiresReturn = 6,
    Lost = 7,
    Retired = 8
}