using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Events;

// Training Lifecycle Events
public record TrainingCreatedEvent(int TrainingId, string Title, TrainingType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingUpdatedEvent(int TrainingId, string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingScheduledEvent(int TrainingId, string Title, DateTime ScheduledStartDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingStartedEvent(int TrainingId, string Title, string InstructorName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingCompletedEvent(int TrainingId, string Title, int ParticipantCount, string InstructorName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingCancelledEvent(int TrainingId, string Title, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingParticipantEnrolledEvent(int TrainingId, int ParticipantUserId, string ParticipantName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingParticipantRemovedEvent(int TrainingId, int ParticipantUserId, string ParticipantName, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingRatedEvent(int TrainingId, string ParticipantName, decimal Rating, string? Feedback) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingCertificationIssuedEvent(int TrainingId, int CertificationId, string ParticipantName, string CertificateNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Training Assessment Events
public record TrainingAssessmentCompletedEvent(int TrainingId, int ParticipantUserId, string ParticipantName, decimal Score, bool Passed) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingAttendanceMarkedEvent(int TrainingId, int ParticipantUserId, string ParticipantName, bool IsPresent) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Training Content Events
public record TrainingAttachmentUploadedEvent(int TrainingId, string FileName, string UploadedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingCommentAddedEvent(int TrainingId, string CommentContent, string AuthorName, TrainingCommentType CommentType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TrainingRequirementAddedEvent(int TrainingId, string RequirementDescription, bool IsMandatory) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}