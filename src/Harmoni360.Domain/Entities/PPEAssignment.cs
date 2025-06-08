using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class PPEAssignment : BaseEntity, IAuditableEntity
{
    public int PPEItemId { get; private set; }
    public PPEItem PPEItem { get; private set; } = null!;
    public int AssignedToId { get; private set; }
    public User AssignedTo { get; private set; } = null!;
    public DateTime AssignedDate { get; private set; }
    public DateTime? ReturnedDate { get; private set; }
    public string AssignedBy { get; private set; } = string.Empty;
    public string? ReturnedBy { get; private set; }
    public string? Purpose { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public string? ReturnNotes { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected PPEAssignment() { } // For EF Core

    public static PPEAssignment Create(
        int ppeItemId,
        int assignedToId,
        string assignedBy,
        string? purpose = null)
    {
        var assignment = new PPEAssignment
        {
            PPEItemId = ppeItemId,
            AssignedToId = assignedToId,
            AssignedDate = DateTime.UtcNow,
            AssignedBy = assignedBy,
            Purpose = purpose,
            Status = AssignmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = assignedBy
        };

        return assignment;
    }

    public void Return(string returnedBy, string? returnNotes = null)
    {
        if (Status != AssignmentStatus.Active)
        {
            throw new InvalidOperationException("Cannot return PPE assignment that is not active");
        }

        ReturnedDate = DateTime.UtcNow;
        ReturnedBy = returnedBy;
        ReturnNotes = returnNotes;
        Status = AssignmentStatus.Returned;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = returnedBy;
    }

    public void MarkAsLost(string reportedBy, string? notes = null)
    {
        if (Status != AssignmentStatus.Active)
        {
            throw new InvalidOperationException("Cannot mark PPE assignment as lost that is not active");
        }

        ReturnedDate = DateTime.UtcNow;
        ReturnedBy = reportedBy;
        ReturnNotes = $"LOST: {notes}";
        Status = AssignmentStatus.Lost;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = reportedBy;
    }

    public void MarkAsDamaged(string reportedBy, string? damageDescription = null)
    {
        if (Status != AssignmentStatus.Active)
        {
            throw new InvalidOperationException("Cannot mark PPE assignment as damaged that is not active");
        }

        ReturnedDate = DateTime.UtcNow;
        ReturnedBy = reportedBy;
        ReturnNotes = $"DAMAGED: {damageDescription}";
        Status = AssignmentStatus.Damaged;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = reportedBy;
    }

    // Computed Properties
    public TimeSpan? AssignmentDuration => ReturnedDate.HasValue 
        ? ReturnedDate.Value - AssignedDate 
        : DateTime.UtcNow - AssignedDate;

    public int DaysAssigned => (int)(AssignmentDuration?.TotalDays ?? 0);

    public bool IsActive => Status == AssignmentStatus.Active;
}

public enum AssignmentStatus
{
    Active = 1,
    Returned = 2,
    Lost = 3,
    Damaged = 4
}