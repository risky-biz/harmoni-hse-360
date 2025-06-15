using FluentAssertions;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Events;
using Xunit;

namespace Harmoni360.Domain.Tests.Entities;

public class TrainingParticipantTests
{
    [Fact]
    public void Create_ShouldCreateParticipantWithValidData()
    {
        // Arrange
        var trainingId = 1;
        var participantId = 2;
        var enrolledBy = 3;
        var isWaitlisted = false;

        // Act
        var participant = TrainingParticipant.Create(
            trainingId: trainingId,
            participantId: participantId,
            enrolledBy: enrolledBy,
            isWaitlisted: isWaitlisted
        );

        // Assert
        participant.Should().NotBeNull();
        participant.TrainingId.Should().Be(trainingId);
        participant.ParticipantId.Should().Be(participantId);
        participant.EnrolledBy.Should().Be(enrolledBy);
        participant.IsWaitlisted.Should().Be(isWaitlisted);
        participant.Status.Should().Be(ParticipantStatus.Enrolled);
        participant.EnrolledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        participant.IsPresent.Should().BeNull();
        participant.Score.Should().BeNull();
        participant.Passed.Should().BeNull();
        participant.CompletedAt.Should().BeNull();
        participant.AttendanceMarkedAt.Should().BeNull();

        // Domain event should be raised
        participant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ParticipantEnrolledEvent>();
        
        var domainEvent = (ParticipantEnrolledEvent)participant.DomainEvents.First();
        domainEvent.TrainingId.Should().Be(trainingId);
        domainEvent.ParticipantId.Should().Be(participantId);
    }

    [Fact]
    public void MarkAttendance_ShouldUpdateAttendanceStatus()
    {
        // Arrange
        var participant = CreateValidParticipant();
        var notes = "Present and engaged";

        // Act
        participant.MarkAttendance(true, notes);

        // Assert
        participant.IsPresent.Should().BeTrue();
        participant.AttendanceNotes.Should().Be(notes);
        participant.AttendanceMarkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        participant.Status.Should().Be(ParticipantStatus.Attended);
        participant.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAttendance_WhenAbsent_ShouldUpdateStatusToAbsent()
    {
        // Arrange
        var participant = CreateValidParticipant();

        // Act
        participant.MarkAttendance(false, "No show");

        // Assert
        participant.IsPresent.Should().BeFalse();
        participant.AttendanceNotes.Should().Be("No show");
        participant.Status.Should().Be(ParticipantStatus.Absent);
    }

    [Fact]
    public void RecordResults_ShouldUpdateResultsAndStatus()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.MarkAttendance(true, "Present");
        var score = 85m;
        var feedback = "Excellent performance";

        // Act
        participant.RecordResults(score, true, feedback);

        // Assert
        participant.Score.Should().Be(score);
        participant.Passed.Should().BeTrue();
        participant.Feedback.Should().Be(feedback);
        participant.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        participant.Status.Should().Be(ParticipantStatus.Completed);
        participant.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordResults_WhenFailed_ShouldUpdateStatusToFailed()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.MarkAttendance(true, "Present");

        // Act
        participant.RecordResults(45m, false, "Did not meet minimum requirements");

        // Assert
        participant.Score.Should().Be(45m);
        participant.Passed.Should().BeFalse();
        participant.Status.Should().Be(ParticipantStatus.Failed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void RecordResults_WithInvalidScore_ShouldThrowArgumentException(decimal invalidScore)
    {
        // Arrange
        var participant = CreateValidParticipant();

        // Act & Assert
        var act = () => participant.RecordResults(invalidScore, true);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*0 and 100*");
    }

    [Fact]
    public void Cancel_ShouldUpdateStatusToCancelled()
    {
        // Arrange
        var participant = CreateValidParticipant();
        var reason = "Medical emergency";

        // Act
        participant.Cancel(reason);

        // Assert
        participant.Status.Should().Be(ParticipantStatus.Cancelled);
        participant.CancellationReason.Should().Be(reason);
        participant.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        participant.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MoveFromWaitlist_ShouldUpdateWaitlistStatus()
    {
        // Arrange
        var participant = TrainingParticipant.Create(
            trainingId: 1,
            participantId: 2,
            enrolledBy: 3,
            isWaitlisted: true
        );

        // Act
        participant.MoveFromWaitlist();

        // Assert
        participant.IsWaitlisted.Should().BeFalse();
        participant.Status.Should().Be(ParticipantStatus.Enrolled);
        participant.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MoveFromWaitlist_WhenNotWaitlisted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var participant = CreateValidParticipant(); // Not waitlisted

        // Act & Assert
        var act = () => participant.MoveFromWaitlist();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*waitlisted*");
    }

    [Fact]
    public void StartInProgress_ShouldUpdateStatusToInProgress()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.MarkAttendance(true, "Present");

        // Act
        participant.StartInProgress();

        // Assert
        participant.Status.Should().Be(ParticipantStatus.InProgress);
        participant.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void StartInProgress_WhenNotAttended_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var participant = CreateValidParticipant();

        // Act & Assert
        var act = () => participant.StartInProgress();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*attended*");
    }

    [Fact]
    public void UpdateEnrollmentDetails_ShouldUpdateDetails()
    {
        // Arrange
        var participant = CreateValidParticipant();
        var specialRequirements = "Wheelchair access needed";

        // Act
        participant.UpdateEnrollmentDetails(specialRequirements);

        // Assert
        participant.SpecialRequirements.Should().Be(specialRequirements);
        participant.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CanMarkAttendance_WhenEnrolled_ShouldReturnTrue()
    {
        // Arrange
        var participant = CreateValidParticipant();

        // Act & Assert
        participant.CanMarkAttendance().Should().BeTrue();
    }

    [Fact]
    public void CanMarkAttendance_WhenCancelled_ShouldReturnFalse()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.Cancel("Reason");

        // Act & Assert
        participant.CanMarkAttendance().Should().BeFalse();
    }

    [Fact]
    public void CanMarkAttendance_WhenWaitlisted_ShouldReturnFalse()
    {
        // Arrange
        var participant = TrainingParticipant.Create(
            trainingId: 1,
            participantId: 2,
            enrolledBy: 3,
            isWaitlisted: true
        );

        // Act & Assert
        participant.CanMarkAttendance().Should().BeFalse();
    }

    [Fact]
    public void CanRecordResults_WhenAttended_ShouldReturnTrue()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.MarkAttendance(true, "Present");

        // Act & Assert
        participant.CanRecordResults().Should().BeTrue();
    }

    [Fact]
    public void CanRecordResults_WhenAbsent_ShouldReturnFalse()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.MarkAttendance(false, "Absent");

        // Act & Assert
        participant.CanRecordResults().Should().BeFalse();
    }

    [Fact]
    public void CanRecordResults_WhenNotAttended_ShouldReturnFalse()
    {
        // Arrange
        var participant = CreateValidParticipant();

        // Act & Assert
        participant.CanRecordResults().Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForCertification_WhenPassedAndRequiredAttendance_ShouldReturnTrue()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.MarkAttendance(true, "Present");
        participant.RecordResults(85m, true, "Good work");

        // Act & Assert
        participant.IsEligibleForCertification().Should().BeTrue();
    }

    [Fact]
    public void IsEligibleForCertification_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.MarkAttendance(true, "Present");
        participant.RecordResults(45m, false, "Failed");

        // Act & Assert
        participant.IsEligibleForCertification().Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForCertification_WhenAbsent_ShouldReturnFalse()
    {
        // Arrange
        var participant = CreateValidParticipant();
        participant.MarkAttendance(false, "Absent");

        // Act & Assert
        participant.IsEligibleForCertification().Should().BeFalse();
    }

    private static TrainingParticipant CreateValidParticipant()
    {
        return TrainingParticipant.Create(
            trainingId: 1,
            participantId: 2,
            enrolledBy: 3,
            isWaitlisted: false
        );
    }
}