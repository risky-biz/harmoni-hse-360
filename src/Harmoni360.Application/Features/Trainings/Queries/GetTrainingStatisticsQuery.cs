using MediatR;

namespace Harmoni360.Application.Features.Trainings.Queries;

public record GetTrainingStatisticsQuery : IRequest<DetailedTrainingStatisticsDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int? DepartmentId { get; init; }
    public int? UserId { get; init; }
}

public class DetailedTrainingStatisticsDto
{
    public int TotalTrainings { get; set; }
    public int CompletedTrainings { get; set; }
    public int OngoingTrainings { get; set; }
    public int CancelledTrainings { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalCertifications { get; set; }
    public decimal AveragePassRate { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCostPerParticipant { get; set; }
    public int TotalTrainingHours { get; set; }
    public decimal AverageEffectivenessScore { get; set; }
}