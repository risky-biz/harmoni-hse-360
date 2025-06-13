using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class HazardReassessment : BaseEntity
{
    public int HazardId { get; private set; }
    public Hazard Hazard { get; private set; } = null!;
    public DateTime ScheduledDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? CompletedById { get; private set; }
    public User? CompletedBy { get; private set; }
    public string? CompletionNotes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected HazardReassessment() { } // For EF Core

    public HazardReassessment(int hazardId, DateTime scheduledDate, string reason)
    {
        HazardId = hazardId;
        ScheduledDate = scheduledDate;
        Reason = reason;
        IsCompleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void CompleteReassessment(int completedById, string completionNotes)
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("Reassessment is already completed");
        }

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        CompletedById = completedById;
        CompletionNotes = completionNotes;
    }

    public bool IsOverdue()
    {
        return !IsCompleted && DateTime.UtcNow > ScheduledDate;
    }
}