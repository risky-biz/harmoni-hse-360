using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record CreateTrainingCommand : IRequest<TrainingDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TrainingType Type { get; init; }
    public TrainingCategory Category { get; init; }
    public TrainingDeliveryMethod DeliveryMethod { get; init; }
    public DateTime ScheduledStartDate { get; init; }
    public DateTime ScheduledEndDate { get; init; }
    public int DurationHours { get; init; }
    public string Venue { get; init; } = string.Empty;
    public string VenueAddress { get; init; } = string.Empty;
    public int MaxParticipants { get; init; }
    public int MinParticipants { get; init; }
    
    // Location Information
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string OnlineLink { get; init; } = string.Empty;
    public string OnlinePlatform { get; init; } = string.Empty;
    
    // Training Content
    public string LearningObjectives { get; init; } = string.Empty;
    public string CourseOutline { get; init; } = string.Empty;
    public string Prerequisites { get; init; } = string.Empty;
    public string Materials { get; init; } = string.Empty;
    public AssessmentMethod AssessmentMethod { get; init; } = AssessmentMethod.Written;
    public decimal PassingScore { get; init; } = 70.0m;
    
    // Instructor Information
    public string InstructorName { get; init; } = string.Empty;
    public string InstructorQualifications { get; init; } = string.Empty;
    public string InstructorContact { get; init; } = string.Empty;
    public bool IsExternalInstructor { get; init; }
    
    // Certification Details
    public bool IssuesCertificate { get; init; }
    public CertificationType CertificationType { get; init; } = CertificationType.Completion;
    public ValidityPeriod CertificateValidityPeriod { get; init; } = ValidityPeriod.OneYear;
    public string CertifyingBody { get; init; } = string.Empty;
    
    // Cost Information
    public decimal CostPerParticipant { get; init; }
    public decimal TotalBudget { get; init; }
    public string Currency { get; init; } = "IDR";
    
    // Indonesian Compliance Fields
    public bool IsK3MandatoryTraining { get; init; }
    public string K3RegulationReference { get; init; } = string.Empty;
    public bool IsBPJSCompliant { get; init; }
    public string MinistryApprovalNumber { get; init; } = string.Empty;
    public bool RequiresGovernmentCertification { get; init; }
    public string IndonesianTrainingStandard { get; init; } = string.Empty;
    
    // Requirements
    public List<CreateTrainingRequirementCommand> Requirements { get; init; } = new();
}

public record CreateTrainingRequirementCommand
{
    public string RequirementDescription { get; init; } = string.Empty;
    public bool IsMandatory { get; init; } = true;
    public DateTime? DueDate { get; init; }
    public string CompetencyLevel { get; init; } = string.Empty;
    public string VerificationMethod { get; init; } = string.Empty;
}