using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Queries;

public class GetTrainingsQuery : IRequest<GetTrainingsResponse>
{
    public string? SearchTerm { get; set; }
    public TrainingType? Type { get; set; }
    public TrainingCategory? Category { get; set; }
    public TrainingStatus? Status { get; set; }
    public TrainingPriority? Priority { get; set; }
    public TrainingDeliveryMethod? DeliveryMethod { get; set; }
    public string? InstructorName { get; set; }
    public DateTime? ScheduledFromDate { get; set; }
    public DateTime? ScheduledToDate { get; set; }
    public bool? IsK3MandatoryTraining { get; set; }
    public bool? RequiresGovernmentCertification { get; set; }
    public bool? IssuesCertificate { get; set; }
    public bool? IsOverdue { get; set; }
    public bool? HasAvailableSpots { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "ScheduledStartDate";
    public bool SortDescending { get; set; } = false;
}

public class GetTrainingsResponse
{
    public List<TrainingSummaryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    
    // Summary Statistics
    public TrainingStatisticsDto Statistics { get; set; } = new();
}

public class TrainingStatisticsDto
{
    public int TotalTrainings { get; set; }
    public int ScheduledTrainings { get; set; }
    public int InProgressTrainings { get; set; }
    public int CompletedTrainings { get; set; }
    public int OverdueTrainings { get; set; }
    public int CancelledTrainings { get; set; }
    public int TotalParticipants { get; set; }
    public int CertificatesIssued { get; set; }
    public decimal AverageRating { get; set; }
    public decimal CompletionRate { get; set; }
    public List<TrainingTypeStatDto> TypeStatistics { get; set; } = new();
    public List<TrainingCategoryStatDto> CategoryStatistics { get; set; } = new();
}

public class TrainingTypeStatDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public int CompletedCount { get; set; }
    public decimal CompletionRate { get; set; }
}

public class TrainingCategoryStatDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public int ParticipantCount { get; set; }
    public int CertificatesIssued { get; set; }
}