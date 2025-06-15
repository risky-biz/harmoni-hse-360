using MediatR;

namespace Harmoni360.Application.Features.Trainings.Queries;

public record GetTrainingDashboardQuery : IRequest<TrainingDashboardDto>
{
    public int? UserId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class TrainingDashboardDto
{
    public int TotalTrainings { get; set; }
    public int CompletedTrainings { get; set; }
    public int OngoingTrainings { get; set; }
    public int UpcomingTrainings { get; set; }
    public int CancelledTrainings { get; set; }
    public int TotalParticipants { get; set; }
    public int CertificationsIssued { get; set; }
    public decimal AveragePassRate { get; set; }
    public decimal AverageEffectivenessScore { get; set; }
    public List<TrainingTypeStatDto> TrainingsByType { get; set; } = new();
    public List<TrainingCategoryStatDto> TrainingsByCategory { get; set; } = new();
    public List<MonthlyTrainingStatDto> MonthlyStats { get; set; } = new();
}


public class MonthlyTrainingStatDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TrainingCount { get; set; }
    public int ParticipantCount { get; set; }
    public int CertificationCount { get; set; }
}