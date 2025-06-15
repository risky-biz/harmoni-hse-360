using FluentAssertions;
using Harmoni360.Application.Features.Trainings.Commands;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;
using Harmoni360.Integration.Tests.Common;
using Harmoni360.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Harmoni360.Integration.Tests.Controllers;

public class TrainingControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public TrainingControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        // Configure JSON serialization to match API conventions
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Set up authenticated user
        _factory.SetCurrentUser(1, "test.user@harmoni360.com");
    }

    [Fact]
    public async Task CreateTraining_WithValidData_ShouldReturnCreatedTraining()
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
            TrainerId = 2,
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
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trainings", command, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdTraining = await response.Content.ReadFromJsonAsync<TrainingDto>(_jsonOptions);
        createdTraining.Should().NotBeNull();
        createdTraining!.Id.Should().BeGreaterThan(0);
        createdTraining.TrainingNumber.Should().NotBeNullOrEmpty();
        createdTraining.TrainingNumber.Should().StartWith("TRN-");
        createdTraining.Title.Should().Be(command.Title);
        createdTraining.Description.Should().Be(command.Description);
        createdTraining.Type.Should().Be(command.Type);
        createdTraining.Category.Should().Be(command.Category);
        createdTraining.Priority.Should().Be(command.Priority);
        createdTraining.DeliveryMethod.Should().Be(command.DeliveryMethod);
        createdTraining.TrainerName.Should().Be("John Trainer");
        createdTraining.Status.Should().Be(TrainingStatus.Draft);
        createdTraining.EstimatedDurationMinutes.Should().Be(command.EstimatedDurationMinutes);
        createdTraining.MaxParticipants.Should().Be(command.MaxParticipants);
        createdTraining.MinParticipants.Should().Be(command.MinParticipants);
        createdTraining.IsMandatory.Should().Be(command.IsMandatory);
        createdTraining.RequiresCertification.Should().Be(command.RequiresCertification);
        createdTraining.CertificationValidityMonths.Should().Be(command.CertificationValidityMonths);
        createdTraining.PassingScore.Should().Be(command.PassingScore);
        createdTraining.LearningObjectives.Should().Be(command.LearningObjectives);
        createdTraining.CompetencyStandard.Should().Be(command.CompetencyStandard);
        createdTraining.RegulatoryRequirement.Should().Be(command.RegulatoryRequirement);
        createdTraining.DepartmentName.Should().Be("Safety Department");
        createdTraining.CreatedBy.Should().Be("Test User");
        createdTraining.Requirements.Should().HaveCount(1);
        createdTraining.Requirements.First().Description.Should().Be("Basic safety orientation completed");
        createdTraining.Requirements.First().Type.Should().Be(TrainingRequirementType.Prerequisite);
        createdTraining.Requirements.First().IsMandatory.Should().BeTrue();

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.PathAndQuery.Should().Be($"/api/trainings/{createdTraining.Id}");
    }

    [Fact]
    public async Task CreateTraining_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateTrainingCommand
        {
            Title = "", // Invalid: empty title
            Description = "Test description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(-1), // Invalid: past date
            EstimatedDurationMinutes = 0, // Invalid: zero duration
            MaxParticipants = 5,
            MinParticipants = 10, // Invalid: min > max
            IsMandatory = true,
            RequiresCertification = false,
            LearningObjectives = "Test objectives"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trainings", command, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTraining_WithNonExistentTrainer_ShouldReturnBadRequest()
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/trainings", command, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTraining_WithValidId_ShouldReturnTraining()
    {
        // Arrange - First create a training
        var createCommand = new CreateTrainingCommand
        {
            Title = "Test Training for Get",
            Description = "Test description",
            Type = TrainingType.Technical,
            Category = TrainingCategory.Optional,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Online,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(5),
            EstimatedDurationMinutes = 90,
            MaxParticipants = 15,
            MinParticipants = 3,
            IsMandatory = false,
            RequiresCertification = false,
            LearningObjectives = "Learn technical skills"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/trainings", createCommand, _jsonOptions);
        var createdTraining = await createResponse.Content.ReadFromJsonAsync<TrainingDto>(_jsonOptions);

        // Act
        var getResponse = await _client.GetAsync($"/api/trainings/{createdTraining!.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var retrievedTraining = await getResponse.Content.ReadFromJsonAsync<TrainingDto>(_jsonOptions);
        retrievedTraining.Should().NotBeNull();
        retrievedTraining!.Id.Should().Be(createdTraining.Id);
        retrievedTraining.Title.Should().Be(createCommand.Title);
        retrievedTraining.Description.Should().Be(createCommand.Description);
        retrievedTraining.TrainerName.Should().Be("John Trainer");
    }

    [Fact]
    public async Task GetTraining_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/trainings/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTrainings_ShouldReturnPaginatedList()
    {
        // Arrange - Create multiple trainings first
        for (int i = 1; i <= 3; i++)
        {
            var createCommand = new CreateTrainingCommand
            {
                Title = $"Test Training {i}",
                Description = $"Test description {i}",
                Type = TrainingType.SafetyOrientation,
                Category = TrainingCategory.Mandatory,
                Priority = TrainingPriority.Medium,
                DeliveryMethod = TrainingDeliveryMethod.Classroom,
                TrainerId = 2,
                ScheduledDate = DateTime.UtcNow.AddDays(i),
                EstimatedDurationMinutes = 120,
                MaxParticipants = 20,
                MinParticipants = 5,
                IsMandatory = true,
                RequiresCertification = false,
                LearningObjectives = $"Learn skills {i}"
            };

            await _client.PostAsJsonAsync("/api/trainings", createCommand, _jsonOptions);
        }

        // Act
        var response = await _client.GetAsync("/api/trainings?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
        
        // The response should contain training data
        responseContent.Should().Contain("Test Training");
    }

    [Fact]
    public async Task GetTrainings_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange - Create trainings with different types
        var safetyTraining = new CreateTrainingCommand
        {
            Title = "Safety Training Filter Test",
            Description = "Safety description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.High,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            EstimatedDurationMinutes = 120,
            MaxParticipants = 20,
            MinParticipants = 5,
            IsMandatory = true,
            RequiresCertification = false,
            LearningObjectives = "Learn safety"
        };

        var technicalTraining = new CreateTrainingCommand
        {
            Title = "Technical Training Filter Test",
            Description = "Technical description",
            Type = TrainingType.Technical,
            Category = TrainingCategory.Optional,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Online,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(2),
            EstimatedDurationMinutes = 90,
            MaxParticipants = 15,
            MinParticipants = 3,
            IsMandatory = false,
            RequiresCertification = false,
            LearningObjectives = "Learn technical skills"
        };

        await _client.PostAsJsonAsync("/api/trainings", safetyTraining, _jsonOptions);
        await _client.PostAsJsonAsync("/api/trainings", technicalTraining, _jsonOptions);

        // Act - Filter by safety training type
        var response = await _client.GetAsync($"/api/trainings?type={TrainingType.SafetyOrientation}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Safety Training Filter Test");
        responseContent.Should().NotContain("Technical Training Filter Test");
    }

    [Fact]
    public async Task DeleteTraining_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Create a training first
        var createCommand = new CreateTrainingCommand
        {
            Title = "Training to Delete",
            Description = "Test description",
            Type = TrainingType.Technical,
            Category = TrainingCategory.Optional,
            Priority = TrainingPriority.Low,
            DeliveryMethod = TrainingDeliveryMethod.Online,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(3),
            EstimatedDurationMinutes = 60,
            MaxParticipants = 10,
            MinParticipants = 2,
            IsMandatory = false,
            RequiresCertification = false,
            LearningObjectives = "Test objectives"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/trainings", createCommand, _jsonOptions);
        var createdTraining = await createResponse.Content.ReadFromJsonAsync<TrainingDto>(_jsonOptions);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/trainings/{createdTraining!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify training is deleted
        var getResponse = await _client.GetAsync($"/api/trainings/{createdTraining.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTraining_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/trainings/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTraining_WithValidData_ShouldReturnUpdatedTraining()
    {
        // Arrange - Create a training first
        var createCommand = new CreateTrainingCommand
        {
            Title = "Original Training Title",
            Description = "Original description",
            Type = TrainingType.Technical,
            Category = TrainingCategory.Optional,
            Priority = TrainingPriority.Low,
            DeliveryMethod = TrainingDeliveryMethod.Online,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(5),
            EstimatedDurationMinutes = 60,
            MaxParticipants = 10,
            MinParticipants = 2,
            IsMandatory = false,
            RequiresCertification = false,
            LearningObjectives = "Original objectives"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/trainings", createCommand, _jsonOptions);
        var createdTraining = await createResponse.Content.ReadFromJsonAsync<TrainingDto>(_jsonOptions);

        var updateCommand = new UpdateTrainingCommand
        {
            Id = createdTraining!.Id,
            Title = "Updated Training Title",
            Description = "Updated description",
            Type = TrainingType.SafetyOrientation,
            Category = TrainingCategory.Mandatory,
            Priority = TrainingPriority.High,
            DeliveryMethod = TrainingDeliveryMethod.Classroom,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(10),
            EstimatedDurationMinutes = 120,
            MaxParticipants = 25,
            MinParticipants = 5,
            IsMandatory = true,
            RequiresCertification = true,
            CertificationValidityMonths = 12,
            PassingScore = 75m,
            LearningObjectives = "Updated objectives"
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/trainings/{createdTraining.Id}", updateCommand, _jsonOptions);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedTraining = await updateResponse.Content.ReadFromJsonAsync<TrainingDto>(_jsonOptions);
        updatedTraining.Should().NotBeNull();
        updatedTraining!.Id.Should().Be(createdTraining.Id);
        updatedTraining.Title.Should().Be("Updated Training Title");
        updatedTraining.Description.Should().Be("Updated description");
        updatedTraining.Type.Should().Be(TrainingType.SafetyOrientation);
        updatedTraining.Category.Should().Be(TrainingCategory.Mandatory);
        updatedTraining.Priority.Should().Be(TrainingPriority.High);
        updatedTraining.DeliveryMethod.Should().Be(TrainingDeliveryMethod.Classroom);
        updatedTraining.EstimatedDurationMinutes.Should().Be(120);
        updatedTraining.MaxParticipants.Should().Be(25);
        updatedTraining.MinParticipants.Should().Be(5);
        updatedTraining.IsMandatory.Should().BeTrue();
        updatedTraining.RequiresCertification.Should().BeTrue();
        updatedTraining.CertificationValidityMonths.Should().Be(12);
        updatedTraining.PassingScore.Should().Be(75m);
        updatedTraining.LearningObjectives.Should().Be("Updated objectives");
    }

    [Fact]
    public async Task UpdateTraining_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var updateCommand = new UpdateTrainingCommand
        {
            Id = 999999, // Non-existent ID
            Title = "Updated Title",
            Description = "Updated description",
            Type = TrainingType.Technical,
            Category = TrainingCategory.Optional,
            Priority = TrainingPriority.Medium,
            DeliveryMethod = TrainingDeliveryMethod.Online,
            TrainerId = 2,
            ScheduledDate = DateTime.UtcNow.AddDays(5),
            EstimatedDurationMinutes = 90,
            MaxParticipants = 15,
            MinParticipants = 3,
            IsMandatory = false,
            RequiresCertification = false,
            LearningObjectives = "Updated objectives"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/trainings/999999", updateCommand, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}