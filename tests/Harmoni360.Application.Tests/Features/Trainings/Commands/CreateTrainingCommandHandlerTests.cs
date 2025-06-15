using FluentAssertions;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.Commands;
using Harmoni360.Application.Tests.Common;
using Harmoni360.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harmoni360.Application.Tests.Features.Trainings.Commands;

public class CreateTrainingCommandHandlerTests : BaseTest
{
    private readonly CreateTrainingCommandHandler _handler;
    private readonly Mock<ILogger<CreateTrainingCommandHandler>> _mockLogger;

    public CreateTrainingCommandHandlerTests()
    {
        SeedData();
        _mockLogger = new Mock<ILogger<CreateTrainingCommandHandler>>();
        _handler = new CreateTrainingCommandHandler(Context, MockCurrentUserService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateTraining()
    {
        // Arrange
        var command = new CreateTrainingCommand
        {
            Title = "Safety Induction Training",
            Description = "Comprehensive safety training for new employees",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.High,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2, // John Trainer
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            EstimatedDurationMinutes = 240,
            MaxParticipants = 25,
            MinParticipants = 5,
            IsMandatory = true,
            RequiresCertification = true,
            CertificationValidityMonths = 12,
            PassingScore = 80m,
            LearningObjectives = "Learn basic safety protocols and emergency procedures",
            CompetencyStandard = "ISO 45001:2018",
            RegulatoryRequirement = "OSHA 1926.95",
            DepartmentId = 1,
            Requirements = new List<CreateTrainingRequirementDto>
            {
                new()
                {
                    Description = "Basic safety orientation completed",
                    Type = TrainingRequirementType.Prerequisite,
                    IsMandatory = true
                },
                new()
                {
                    Description = "Understand emergency evacuation procedures",
                    Type = TrainingRequirementType.LearningObjective,
                    IsMandatory = true
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.TrainingNumber.Should().NotBeNullOrEmpty();
        result.Title.Should().Be(command.Title);
        result.Description.Should().Be(command.Description);
        result.Type.Should().Be(command.Type);
        result.Category.Should().Be(command.Category);
        result.Priority.Should().Be(command.Priority);
        result.DeliveryMethod.Should().Be(command.DeliveryMethod);
        result.TrainerName.Should().Be("John Trainer");
        result.Status.Should().Be(TrainingStatus.Draft);
        result.ScheduledDate.Should().Be(command.ScheduledDate);
        result.EstimatedDurationMinutes.Should().Be(command.EstimatedDurationMinutes);
        result.MaxParticipants.Should().Be(command.MaxParticipants);
        result.MinParticipants.Should().Be(command.MinParticipants);
        result.IsMandatory.Should().Be(command.IsMandatory);
        result.RequiresCertification.Should().Be(command.RequiresCertification);
        result.CertificationValidityMonths.Should().Be(command.CertificationValidityMonths);
        result.PassingScore.Should().Be(command.PassingScore);
        result.LearningObjectives.Should().Be(command.LearningObjectives);
        result.CompetencyStandard.Should().Be(command.CompetencyStandard);
        result.RegulatoryRequirement.Should().Be(command.RegulatoryRequirement);
        result.DepartmentName.Should().Be("Safety Department");
        result.CreatedBy.Should().Be("Test User");
        result.Requirements.Should().HaveCount(2);

        // Verify training was saved to database
        var savedTraining = await Context.Trainings.FindAsync(result.Id);
        savedTraining.Should().NotBeNull();
        savedTraining!.Title.Should().Be(command.Title);
        savedTraining.CreatedBy.Should().Be(1); // Current user ID
    }

    [Fact]
    public async Task Handle_WithMinimalRequiredData_ShouldCreateTraining()
    {
        // Arrange
        var command = new CreateTrainingCommand
        {
            Title = "Basic Training",
            Description = "Simple training description",
            Type = TrainingType.Technical,
            Category = TrainingCategory.Optional,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Online,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(3),
            EstimatedDurationMinutes = 60,
            MaxParticipants = 10,
            MinParticipants = 2,
            IsMandatory = false,
            RequiresCertification = false,
            LearningObjectives = "Basic understanding"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be(command.Title);
        result.RequiresCertification.Should().BeFalse();
        result.CertificationValidityMonths.Should().BeNull();
        result.PassingScore.Should().BeNull();
        result.CompetencyStandard.Should().BeNull();
        result.RegulatoryRequirement.Should().BeNull();
        result.DepartmentName.Should().BeNull();
        result.Requirements.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithInvalidTrainer_ShouldThrowException()
    {
        // Arrange
        var command = new CreateTrainingCommand
        {
            Title = "Training with Invalid Trainer",
            Description = "Test description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 999, // Non-existent trainer
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            EstimatedDurationMinutes = 120,
            MaxParticipants = 20,
            MinParticipants = 5,
            IsMandatory = true,
            RequiresCertification = false,
            LearningObjectives = "Test objectives"
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*trainer*");
    }

    [Fact]
    public async Task Handle_WithInvalidDepartment_ShouldThrowException()
    {
        // Arrange
        var command = new CreateTrainingCommand
        {
            Title = "Training with Invalid Department",
            Description = "Test description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            EstimatedDurationMinutes = 120,
            MaxParticipants = 20,
            MinParticipants = 5,
            IsMandatory = true,
            RequiresCertification = false,
            LearningObjectives = "Test objectives",
            DepartmentId = 999 // Non-existent department
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*department*");
    }

    [Fact]
    public async Task Handle_WithPastScheduledDate_ShouldThrowException()
    {
        // Arrange
        var command = new CreateTrainingCommand
        {
            Title = "Training with Past Date",
            Description = "Test description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(-1), // Past date
            EstimatedDurationMinutes = 120,
            MaxParticipants = 20,
            MinParticipants = 5,
            IsMandatory = true,
            RequiresCertification = false,
            LearningObjectives = "Test objectives"
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*future*");
    }

    [Fact]
    public async Task Handle_WithInvalidParticipantNumbers_ShouldThrowException()
    {
        // Arrange
        var command = new CreateTrainingCommand
        {
            Title = "Training with Invalid Participants",
            Description = "Test description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            EstimatedDurationMinutes = 120,
            MaxParticipants = 5, // Less than minimum
            MinParticipants = 10,
            IsMandatory = true,
            RequiresCertification = false,
            LearningObjectives = "Test objectives"
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*participants*");
    }

    [Fact]
    public async Task Handle_WithRequirements_ShouldCreateTrainingWithRequirements()
    {
        // Arrange
        var command = new CreateTrainingCommand
        {
            Title = "Training with Requirements",
            Description = "Test description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            EstimatedDurationMinutes = 120,
            MaxParticipants = 20,
            MinParticipants = 5,
            IsMandatory = true,
            RequiresCertification = true,
            CertificationValidityMonths = 12,
            PassingScore = 75m,
            LearningObjectives = "Test objectives",
            Requirements = new List<CreateTrainingRequirementDto>
            {
                new()
                {
                    Description = "Prerequisite training completed",
                    Type = TrainingRequirementType.Prerequisite,
                    IsMandatory = true
                },
                new()
                {
                    Description = "Understand safety protocols",
                    Type = TrainingRequirementType.LearningObjective,
                    IsMandatory = false
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Requirements.Should().HaveCount(2);
        
        var prerequisite = result.Requirements.First(r => r.Type == TrainingRequirementType.Prerequisite);
        prerequisite.Description.Should().Be("Prerequisite training completed");
        prerequisite.IsMandatory.Should().BeTrue();
        
        var objective = result.Requirements.First(r => r.Type == TrainingRequirementType.LearningObjective);
        objective.Description.Should().Be("Understand safety protocols");
        objective.IsMandatory.Should().BeFalse();

        // Verify requirements were saved to database
        var savedTraining = await Context.Trainings
            .Include(t => t.Requirements)
            .FirstAsync(t => t.Id == result.Id);
        
        savedTraining.Requirements.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueTrainingNumber()
    {
        // Arrange
        var command1 = CreateValidCommand("Training 1");
        var command2 = CreateValidCommand("Training 2");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.TrainingNumber.Should().NotBe(result2.TrainingNumber);
        result1.TrainingNumber.Should().StartWith("TRN-");
        result2.TrainingNumber.Should().StartWith("TRN-");
    }

    private CreateTrainingCommand CreateValidCommand(string title)
    {
        return new CreateTrainingCommand
        {
            Title = title,
            Description = "Test description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            EstimatedDurationMinutes = 120,
            MaxParticipants = 20,
            MinParticipants = 5,
            IsMandatory = true,
            RequiresCertification = false,
            LearningObjectives = "Test objectives"
        };
    }
}