using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Application.Features.Trainings.DTOs;

public class TrainingDto
{
    public int Id { get; set; }
    public string TrainingCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = string.Empty;
    
    // Scheduling Information
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int DurationHours { get; set; }
    public int MaxParticipants { get; set; }
    public int MinParticipants { get; set; }
    public int EnrolledParticipants { get; set; }
    
    // Location Information
    public string Venue { get; set; } = string.Empty;
    public string VenueAddress { get; set; } = string.Empty;
    public GeoLocationDto? GeoLocation { get; set; }
    public string OnlineLink { get; set; } = string.Empty;
    public string OnlinePlatform { get; set; } = string.Empty;
    
    // Training Content
    public string LearningObjectives { get; set; } = string.Empty;
    public string CourseOutline { get; set; } = string.Empty;
    public string Prerequisites { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string AssessmentMethod { get; set; } = string.Empty;
    public decimal PassingScore { get; set; }
    
    // Instructor Information
    public string InstructorName { get; set; } = string.Empty;
    public string InstructorQualifications { get; set; } = string.Empty;
    public string InstructorContact { get; set; } = string.Empty;
    public bool IsExternalInstructor { get; set; }
    
    // Certification Details
    public bool IssuesCertificate { get; set; }
    public string CertificationType { get; set; } = string.Empty;
    public string CertificateValidityPeriod { get; set; } = string.Empty;
    public string CertifyingBody { get; set; } = string.Empty;
    
    // Cost Information
    public decimal CostPerParticipant { get; set; }
    public decimal TotalBudget { get; set; }
    public string Currency { get; set; } = "IDR";
    
    // Indonesian Compliance Fields
    public bool IsK3MandatoryTraining { get; set; }
    public string K3RegulationReference { get; set; } = string.Empty;
    public bool IsBPJSCompliant { get; set; }
    public string MinistryApprovalNumber { get; set; } = string.Empty;
    public bool RequiresGovernmentCertification { get; set; }
    public string IndonesianTrainingStandard { get; set; } = string.Empty;
    
    // Evaluation and Feedback
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public string EvaluationSummary { get; set; } = string.Empty;
    public string ImprovementActions { get; set; } = string.Empty;
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Navigation Properties
    public List<TrainingParticipantDto> Participants { get; set; } = new();
    public List<TrainingRequirementDto> Requirements { get; set; } = new();
    public List<TrainingAttachmentDto> Attachments { get; set; } = new();
    public List<TrainingCommentDto> Comments { get; set; } = new();
    public List<TrainingCertificationDto> Certifications { get; set; } = new();
    
    // Computed Properties
    public bool CanEdit => Status == "Draft" || Status == "Scheduled";
    public bool CanStart => Status == "Scheduled" && EnrolledParticipants >= MinParticipants;
    public bool CanComplete => Status == "InProgress";
    public bool CanCancel => Status != "Completed" && Status != "Cancelled";
    public bool CanEnroll => Status == "Scheduled" && EnrolledParticipants < MaxParticipants;
    public bool IsOverdue => Status == "Scheduled" && ScheduledStartDate < DateTime.Now;
    public int AvailableSpots => MaxParticipants - EnrolledParticipants;
    public decimal CompletionRate => EnrolledParticipants > 0 ? 
        (decimal)Participants.Count(p => p.Status == "Completed") / EnrolledParticipants * 100 : 0;
}

public class TrainingSummaryDto
{
    public int Id { get; set; }
    public string TrainingCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = string.Empty;
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public int EnrolledParticipants { get; set; }
    public int MaxParticipants { get; set; }
    public bool IsK3MandatoryTraining { get; set; }
    public bool RequiresGovernmentCertification { get; set; }
    public decimal AverageRating { get; set; }
    public bool IsOverdue { get; set; }
    public bool CanEdit { get; set; }
    public bool CanStart { get; set; }
    public bool CanEnroll { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class GeoLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string LocationDescription { get; set; } = string.Empty;
}