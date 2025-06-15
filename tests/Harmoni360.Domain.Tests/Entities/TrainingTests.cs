using FluentAssertions;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Events;
using Xunit;

namespace Harmoni360.Domain.Tests.Entities;

public class TrainingTests
{
    [Fact]
    public void Create_ShouldCreateTrainingWithValidData()
    {
        // Arrange
        var title = "Safety Induction Training";
        var description = "Comprehensive safety training for new employees";
        var type = TrainingType.SafetyOrientation;
        var category = TrainingCategory.Mandatory;
        var priority = TrainingPriority.High;
        var deliveryMethod = TrainingDeliveryMethod.Classroom;
        var trainerId = 1;
        var scheduledDate = DateTime.UtcNow.AddDays(7);
        var estimatedDuration = 240; // 4 hours

        // Act
        var training = Training.Create(
            title: title,
            description: description,
            type: type,
            category: category,
            priority: priority,
            deliveryMethod: deliveryMethod,
            trainerId: trainerId,
            scheduledDate: scheduledDate,
            estimatedDurationMinutes: estimatedDuration,
            maxParticipants: 25,
            minParticipants: 5,
            isMandatory: true,
            requiresCertification: true,
            certificationValidityMonths: 12,
            passingScore: 80m,
            learningObjectives: "Learn basic safety protocols"
        );

        // Assert
        training.Should().NotBeNull();
        training.Title.Should().Be(title);
        training.Description.Should().Be(description);
        training.Type.Should().Be(type);
        training.Category.Should().Be(category);
        training.Priority.Should().Be(priority);
        training.DeliveryMethod.Should().Be(deliveryMethod);
        training.TrainerId.Should().Be(trainerId);
        training.ScheduledDate.Should().Be(scheduledDate);
        training.EstimatedDurationMinutes.Should().Be(estimatedDuration);
        training.MaxParticipants.Should().Be(25);
        training.MinParticipants.Should().Be(5);
        training.IsMandatory.Should().BeTrue();
        training.RequiresCertification.Should().BeTrue();
        training.CertificationValidityMonths.Should().Be(12);
        training.PassingScore.Should().Be(80m);
        training.LearningObjectives.Should().Be("Learn basic safety protocols");
        training.Status.Should().Be(TrainingStatus.Draft);
        training.TrainingNumber.Should().NotBeNullOrEmpty();
        training.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Domain event should be raised
        training.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TrainingCreatedEvent>();
        
        var domainEvent = (TrainingCreatedEvent)training.DomainEvents.First();
        domainEvent.TrainingId.Should().Be(training.Id);
        domainEvent.TrainingNumber.Should().Be(training.TrainingNumber);
        domainEvent.Type.Should().Be(type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Act & Assert
        var act = () => Training.Create(
            title: invalidTitle,
            description: "Valid description",
            type: TrainingType.SafetyOrientation,
            category: TrainingCategory.Mandatory,
            priority: TrainingPriority.Medium,
            deliveryMethod: TrainingDeliveryMethod.Classroom,
            trainerId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(1),
            estimatedDurationMinutes: 120,
            maxParticipants: 20,
            minParticipants: 5,
            isMandatory: true,
            requiresCertification: false,
            certificationValidityMonths: null,
            passingScore: null,
            learningObjectives: "Learn safety"
        );

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Title*");
    }

    [Fact]
    public void Create_WithInvalidParticipantNumbers_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => Training.Create(
            title: "Valid Title",
            description: "Valid description",
            type: TrainingType.SafetyOrientation,
            category: TrainingCategory.Mandatory,
            priority: TrainingPriority.Medium,
            deliveryMethod: TrainingDeliveryMethod.Classroom,
            trainerId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(1),
            estimatedDurationMinutes: 120,
            maxParticipants: 5, // Less than min
            minParticipants: 10,
            isMandatory: true,
            requiresCertification: false,
            certificationValidityMonths: null,
            passingScore: null,
            learningObjectives: "Learn safety"
        );

        act.Should().Throw<ArgumentException>()
            .WithMessage("*participants*");
    }

    [Fact]
    public void Create_WithPastScheduledDate_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => Training.Create(
            title: "Valid Title",
            description: "Valid description",
            type: TrainingType.SafetyOrientation,
            category: TrainingCategory.Mandatory,
            priority: TrainingPriority.Medium,
            deliveryMethod: TrainingDeliveryMethod.Classroom,
            trainerId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(-1), // Past date
            estimatedDurationMinutes: 120,
            maxParticipants: 20,
            minParticipants: 5,
            isMandatory: true,
            requiresCertification: false,
            certificationValidityMonths: null,
            passingScore: null,
            learningObjectives: "Learn safety"
        );

        act.Should().Throw<ArgumentException>()
            .WithMessage("*future*");
    }

    [Fact]
    public void Schedule_ShouldUpdateScheduleAndRaiseDomainEvent()
    {
        // Arrange
        var training = CreateValidTraining();
        var newScheduledDate = DateTime.UtcNow.AddDays(14);
        var newDuration = 180;

        // Act
        training.Schedule(newScheduledDate, newDuration);

        // Assert
        training.ScheduledDate.Should().Be(newScheduledDate);
        training.EstimatedDurationMinutes.Should().Be(newDuration);
        training.Status.Should().Be(TrainingStatus.Scheduled);
        training.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Should raise domain event
        training.DomainEvents.Should().Contain(e => e is TrainingScheduledEvent);
        var scheduledEvent = training.DomainEvents.OfType<TrainingScheduledEvent>().First();
        scheduledEvent.TrainingId.Should().Be(training.Id);
        scheduledEvent.ScheduledDate.Should().Be(newScheduledDate);
    }

    [Fact]
    public void StartTraining_WhenScheduled_ShouldUpdateStatusAndSetStartTime()
    {
        // Arrange
        var training = CreateValidTraining();
        training.Schedule(DateTime.UtcNow.AddDays(1), 120);

        // Act
        training.StartTraining();

        // Assert
        training.Status.Should().Be(TrainingStatus.InProgress);
        training.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        training.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Should raise domain event
        training.DomainEvents.Should().Contain(e => e is TrainingStartedEvent);
        var startedEvent = training.DomainEvents.OfType<TrainingStartedEvent>().First();
        startedEvent.TrainingId.Should().Be(training.Id);
        startedEvent.StartTime.Should().Be(training.StartTime.Value);
    }

    [Fact]
    public void StartTraining_WhenNotScheduled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var training = CreateValidTraining(); // Status is Draft

        // Act & Assert
        var act = () => training.StartTraining();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Scheduled*");
    }

    [Fact]
    public void CompleteTraining_WhenInProgress_ShouldUpdateStatusAndSetCompletedDate()
    {
        // Arrange
        var training = CreateValidTraining();
        training.Schedule(DateTime.UtcNow.AddDays(1), 120);
        training.StartTraining();
        var feedbackSummary = "Excellent training session";
        var effectivenessScore = 95m;

        // Act
        training.CompleteTraining(feedbackSummary, effectivenessScore);

        // Assert
        training.Status.Should().Be(TrainingStatus.Completed);
        training.CompletedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        training.FeedbackSummary.Should().Be(feedbackSummary);
        training.OverallEffectivenessScore.Should().Be(effectivenessScore);
        training.EffectivenessRating.Should().Be(TrainingEffectiveness.Excellent); // 95% = Excellent
        training.ActualDurationMinutes.Should().BeGreaterThan(0);

        // Should raise domain event
        training.DomainEvents.Should().Contain(e => e is TrainingCompletedEvent);
        var completedEvent = training.DomainEvents.OfType<TrainingCompletedEvent>().First();
        completedEvent.TrainingId.Should().Be(training.Id);
        completedEvent.CompletedDate.Should().Be(training.CompletedDate.Value);
    }

    [Fact]
    public void CompleteTraining_WhenNotInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var training = CreateValidTraining(); // Status is Draft

        // Act & Assert
        var act = () => training.CompleteTraining();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*InProgress*");
    }

    [Fact]
    public void Cancel_ShouldUpdateStatusAndRaiseDomainEvent()
    {
        // Arrange
        var training = CreateValidTraining();
        training.Schedule(DateTime.UtcNow.AddDays(1), 120);
        var reason = "Trainer unavailable";

        // Act
        training.Cancel(reason);

        // Assert
        training.Status.Should().Be(TrainingStatus.Cancelled);
        training.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Should raise domain event
        training.DomainEvents.Should().Contain(e => e is TrainingCancelledEvent);
        var cancelledEvent = training.DomainEvents.OfType<TrainingCancelledEvent>().First();
        cancelledEvent.TrainingId.Should().Be(training.Id);
        cancelledEvent.Reason.Should().Be(reason);
    }

    [Fact]
    public void AddParticipant_ShouldAddParticipantToCollection()
    {
        // Arrange
        var training = CreateValidTraining();
        var participant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1,
            enrolledBy: 2,
            isWaitlisted: false
        );

        // Act
        training.AddParticipant(participant);

        // Assert
        training.Participants.Should().ContainSingle()
            .Which.Should().Be(participant);
        training.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddParticipant_WhenTrainingIsFull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var training = CreateValidTraining();
        
        // Fill training to capacity
        for (int i = 1; i <= training.MaxParticipants; i++)
        {
            var participant = TrainingParticipant.Create(
                trainingId: training.Id,
                participantId: i,
                enrolledBy: 1,
                isWaitlisted: false
            );
            training.AddParticipant(participant);
        }

        // Try to add one more
        var extraParticipant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 999,
            enrolledBy: 1,
            isWaitlisted: false
        );

        // Act & Assert
        var act = () => training.AddParticipant(extraParticipant);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*capacity*");
    }

    [Fact]
    public void AddParticipant_WhenAlreadyEnrolled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var training = CreateValidTraining();
        var participant1 = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1,
            enrolledBy: 2,
            isWaitlisted: false
        );
        training.AddParticipant(participant1);

        var participant2 = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1, // Same participant ID
            enrolledBy: 2,
            isWaitlisted: false
        );

        // Act & Assert
        var act = () => training.AddParticipant(participant2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already enrolled*");
    }

    [Fact]
    public void RemoveParticipant_ShouldRemoveParticipantFromCollection()
    {
        // Arrange
        var training = CreateValidTraining();
        var participant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1,
            enrolledBy: 2,
            isWaitlisted: false
        );
        training.AddParticipant(participant);

        // Act
        training.RemoveParticipant(1);

        // Assert
        training.Participants.Should().BeEmpty();
        training.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAttendance_ShouldUpdateParticipantAttendance()
    {
        // Arrange
        var training = CreateValidTraining();
        var participant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1,
            enrolledBy: 2,
            isWaitlisted: false
        );
        training.AddParticipant(participant);
        training.Schedule(DateTime.UtcNow.AddDays(1), 120);
        training.StartTraining();

        // Act
        training.MarkAttendance(1, true, "Present and engaged");

        // Assert
        var updatedParticipant = training.Participants.First();
        updatedParticipant.IsPresent.Should().BeTrue();
        updatedParticipant.AttendanceNotes.Should().Be("Present and engaged");
        updatedParticipant.AttendanceMarkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAttendance_WhenTrainingNotInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var training = CreateValidTraining();
        var participant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1,
            enrolledBy: 2,
            isWaitlisted: false
        );
        training.AddParticipant(participant);

        // Act & Assert
        var act = () => training.MarkAttendance(1, true);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*InProgress*");
    }

    [Fact]
    public void RecordResults_ShouldUpdateParticipantResults()
    {
        // Arrange
        var training = CreateValidTraining();
        var participant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1,
            enrolledBy: 2,
            isWaitlisted: false
        );
        training.AddParticipant(participant);
        training.Schedule(DateTime.UtcNow.AddDays(1), 120);
        training.StartTraining();

        // Act
        training.RecordResults(1, 85m, true, "Excellent performance");

        // Assert
        var updatedParticipant = training.Participants.First();
        updatedParticipant.Score.Should().Be(85m);
        updatedParticipant.Passed.Should().BeTrue();
        updatedParticipant.Feedback.Should().Be("Excellent performance");
        updatedParticipant.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        updatedParticipant.Status.Should().Be(ParticipantStatus.Completed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void RecordResults_WithInvalidScore_ShouldThrowArgumentException(decimal invalidScore)
    {
        // Arrange
        var training = CreateValidTraining();
        var participant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1,
            enrolledBy: 2,
            isWaitlisted: false
        );
        training.AddParticipant(participant);
        training.Schedule(DateTime.UtcNow.AddDays(1), 120);
        training.StartTraining();

        // Act & Assert
        var act = () => training.RecordResults(1, invalidScore, true);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*0 and 100*");
    }

    [Fact]
    public void AssignTrainer_ShouldUpdateTrainer()
    {
        // Arrange
        var training = CreateValidTraining();
        var newTrainerId = 999;

        // Act
        training.AssignTrainer(newTrainerId);

        // Assert
        training.TrainerId.Should().Be(newTrainerId);
        training.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetEffectivenessRating_ShouldReturnCorrectRating()
    {
        // Arrange & Act & Assert
        Training.GetEffectivenessRating(95m).Should().Be(TrainingEffectiveness.Excellent);
        Training.GetEffectivenessRating(85m).Should().Be(TrainingEffectiveness.Good);
        Training.GetEffectivenessRating(75m).Should().Be(TrainingEffectiveness.Satisfactory);
        Training.GetEffectivenessRating(65m).Should().Be(TrainingEffectiveness.NeedsImprovement);
        Training.GetEffectivenessRating(55m).Should().Be(TrainingEffectiveness.Unsatisfactory);
    }

    [Fact]
    public void IsOverdue_WhenScheduledDatePassed_ShouldReturnTrue()
    {
        // Arrange
        var training = CreateValidTraining();
        training.Schedule(DateTime.UtcNow.AddDays(-1), 120); // Past date

        // Act & Assert
        training.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_WhenScheduledDateFuture_ShouldReturnFalse()
    {
        // Arrange
        var training = CreateValidTraining();
        training.Schedule(DateTime.UtcNow.AddDays(1), 120); // Future date

        // Act & Assert
        training.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void GetEnrolledParticipants_ShouldReturnNonWaitlistedParticipants()
    {
        // Arrange
        var training = CreateValidTraining();
        
        var enrolledParticipant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 1,
            enrolledBy: 2,
            isWaitlisted: false
        );
        
        var waitlistedParticipant = TrainingParticipant.Create(
            trainingId: training.Id,
            participantId: 2,
            enrolledBy: 2,
            isWaitlisted: true
        );

        training.AddParticipant(enrolledParticipant);
        training.AddParticipant(waitlistedParticipant);

        // Act
        var enrolledCount = training.GetEnrolledParticipants().Count();

        // Assert
        enrolledCount.Should().Be(1);
    }

    private static Training CreateValidTraining()
    {
        return Training.Create(
            title: "Test Training",
            description: "Test Description",
            type: TrainingType.SafetyOrientation,
            category: TrainingCategory.Mandatory,
            priority: TrainingPriority.Medium,
            deliveryMethod: TrainingDeliveryMethod.Classroom,
            trainerId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(7),
            estimatedDurationMinutes: 120,
            maxParticipants: 25,
            minParticipants: 5,
            isMandatory: true,
            requiresCertification: false,
            certificationValidityMonths: null,
            passingScore: null,
            learningObjectives: "Learn basics"
        );
    }
}