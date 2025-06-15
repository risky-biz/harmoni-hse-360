using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Application.Features.Trainings.Queries;

namespace Harmoni360.Application.Features.Trainings.Services;

public interface ICachedTrainingService
{
    Task<TrainingStatisticsDto> GetCachedStatisticsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TrainingSummaryDto>> GetCachedRecentTrainingsAsync(int count = 5, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrainingSummaryDto>> GetCachedUpcomingTrainingsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task InvalidateStatisticsCacheAsync();
    Task InvalidateTrainingCacheAsync(int trainingId);
    Task InvalidateAllTrainingCachesAsync();
}