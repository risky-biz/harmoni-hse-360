using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Application.Features.Trainings.Queries;
using Harmoni360.Application.Features.Trainings.Services;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Infrastructure.Services;

public class CachedTrainingService : ICachedTrainingService
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CachedTrainingService> _logger;

    private const string StatisticsCacheKey = "training_statistics";
    private const string RecentTrainingsCacheKey = "recent_trainings";
    private const string UpcomingTrainingsCacheKey = "upcoming_trainings";
    private static readonly TimeSpan StatisticsCacheDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan TrainingsCacheDuration = TimeSpan.FromMinutes(10);

    public CachedTrainingService(
        IApplicationDbContext context,
        IMemoryCache memoryCache,
        ILogger<CachedTrainingService> logger)
    {
        _context = context;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<TrainingStatisticsDto> GetCachedStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await _memoryCache.GetOrCreateAsync(
            StatisticsCacheKey,
            async entry =>
            {
                entry.SetAbsoluteExpiration(StatisticsCacheDuration);
                entry.SetPriority(CacheItemPriority.High);
                
                _logger.LogInformation("Computing training statistics for cache");
                return await ComputeStatisticsAsync(cancellationToken);
            });
    }

    public async Task<IEnumerable<TrainingSummaryDto>> GetCachedRecentTrainingsAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{RecentTrainingsCacheKey}_{count}";
        
        return await _memoryCache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.SetAbsoluteExpiration(TrainingsCacheDuration);
                entry.SetPriority(CacheItemPriority.Normal);
                
                _logger.LogInformation("Loading recent trainings for cache (count: {Count})", count);
                return await LoadRecentTrainingsAsync(count, cancellationToken);
            });
    }

    public async Task<IEnumerable<TrainingSummaryDto>> GetCachedUpcomingTrainingsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UpcomingTrainingsCacheKey}_{count}";
        
        return await _memoryCache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.SetAbsoluteExpiration(TrainingsCacheDuration);
                entry.SetPriority(CacheItemPriority.Normal);
                
                _logger.LogInformation("Loading upcoming trainings for cache (count: {Count})", count);
                return await LoadUpcomingTrainingsAsync(count, cancellationToken);
            });
    }

    public Task InvalidateStatisticsCacheAsync()
    {
        _memoryCache.Remove(StatisticsCacheKey);
        _logger.LogInformation("Training statistics cache invalidated");
        return Task.CompletedTask;
    }

    public Task InvalidateTrainingCacheAsync(int trainingId)
    {
        // Remove all training-related caches when a specific training changes
        _memoryCache.Remove(RecentTrainingsCacheKey + "_5");
        _memoryCache.Remove(UpcomingTrainingsCacheKey + "_10");
        _memoryCache.Remove(StatisticsCacheKey);
        
        _logger.LogInformation("Training caches invalidated for training ID: {TrainingId}", trainingId);
        return Task.CompletedTask;
    }

    public Task InvalidateAllTrainingCachesAsync()
    {
        var cacheKeys = new[]
        {
            StatisticsCacheKey,
            RecentTrainingsCacheKey + "_5",
            UpcomingTrainingsCacheKey + "_10"
        };

        foreach (var key in cacheKeys)
        {
            _memoryCache.Remove(key);
        }
        
        _logger.LogInformation("All training caches invalidated");
        return Task.CompletedTask;
    }

    private async Task<TrainingStatisticsDto> ComputeStatisticsAsync(CancellationToken cancellationToken)
    {
        // Use optimized queries for statistics calculation
        var statusCounts = await _context.Trainings
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalCount = await _context.Trainings.CountAsync(cancellationToken);
        var totalParticipants = await _context.TrainingParticipants.CountAsync(cancellationToken);
        var totalCertificates = await _context.TrainingCertifications.CountAsync(cancellationToken);
        
        var completedCount = statusCounts.FirstOrDefault(s => s.Status == TrainingStatus.Completed)?.Count ?? 0;
        
        var averageRating = await _context.Trainings
            .Where(t => t.Status == TrainingStatus.Completed && t.AverageRating > 0)
            .AverageAsync(t => t.AverageRating, cancellationToken);

        var overdueCount = await _context.Trainings
            .CountAsync(t => t.Status == TrainingStatus.Scheduled && t.ScheduledStartDate < DateTime.UtcNow, cancellationToken);

        var typeStatistics = await _context.Trainings
            .GroupBy(t => t.Type)
            .Select(g => new TrainingTypeStatDto
            {
                Type = g.Key.ToString(),
                Count = g.Count(),
                CompletedCount = g.Count(t => t.Status == TrainingStatus.Completed),
                CompletionRate = g.Any() ? (decimal)g.Count(t => t.Status == TrainingStatus.Completed) / g.Count() * 100 : 0
            })
            .ToListAsync(cancellationToken);

        var categoryStatistics = await _context.Trainings
            .GroupBy(t => t.Category)
            .Select(g => new TrainingCategoryStatDto
            {
                Category = g.Key.ToString(),
                Count = g.Count(),
                ParticipantCount = g.Sum(t => t.Participants.Count),
                CertificatesIssued = g.Sum(t => t.Certifications.Count)
            })
            .ToListAsync(cancellationToken);

        return new TrainingStatisticsDto
        {
            TotalTrainings = totalCount,
            ScheduledTrainings = statusCounts.FirstOrDefault(s => s.Status == TrainingStatus.Scheduled)?.Count ?? 0,
            InProgressTrainings = statusCounts.FirstOrDefault(s => s.Status == TrainingStatus.InProgress)?.Count ?? 0,
            CompletedTrainings = completedCount,
            OverdueTrainings = overdueCount,
            CancelledTrainings = statusCounts.FirstOrDefault(s => s.Status == TrainingStatus.Cancelled)?.Count ?? 0,
            TotalParticipants = totalParticipants,
            CertificatesIssued = totalCertificates,
            AverageRating = averageRating,
            CompletionRate = totalCount > 0 ? (decimal)completedCount / totalCount * 100 : 0,
            TypeStatistics = typeStatistics,
            CategoryStatistics = categoryStatistics
        };
    }

    private async Task<IEnumerable<TrainingSummaryDto>> LoadRecentTrainingsAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.Trainings
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.Participants)
            .Select(t => new TrainingSummaryDto
            {
                Id = t.Id,
                TrainingCode = t.TrainingCode,
                Title = t.Title,
                Type = t.Type.ToString(),
                Category = t.Category.ToString(),
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                DeliveryMethod = t.DeliveryMethod.ToString(),
                ScheduledStartDate = t.ScheduledStartDate,
                ScheduledEndDate = t.ScheduledEndDate,
                InstructorName = t.InstructorName,
                Venue = t.Venue,
                EnrolledParticipants = t.Participants.Count,
                MaxParticipants = t.MaxParticipants,
                IsK3MandatoryTraining = t.IsK3MandatoryTraining,
                RequiresGovernmentCertification = t.RequiresGovernmentCertification,
                AverageRating = t.AverageRating,
                IsOverdue = t.Status == TrainingStatus.Scheduled && t.ScheduledStartDate < DateTime.UtcNow,
                CanEdit = t.Status == TrainingStatus.Draft || t.Status == TrainingStatus.Scheduled,
                CanStart = t.Status == TrainingStatus.Scheduled && t.Participants.Count >= t.MinParticipants,
                CanEnroll = t.Status == TrainingStatus.Scheduled && t.Participants.Count < t.MaxParticipants,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<TrainingSummaryDto>> LoadUpcomingTrainingsAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.Trainings
            .Where(t => t.Status == TrainingStatus.Scheduled && t.ScheduledStartDate > DateTime.UtcNow)
            .OrderBy(t => t.ScheduledStartDate)
            .Take(count)
            .Include(t => t.Participants)
            .Select(t => new TrainingSummaryDto
            {
                Id = t.Id,
                TrainingCode = t.TrainingCode,
                Title = t.Title,
                Type = t.Type.ToString(),
                Category = t.Category.ToString(),
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                DeliveryMethod = t.DeliveryMethod.ToString(),
                ScheduledStartDate = t.ScheduledStartDate,
                ScheduledEndDate = t.ScheduledEndDate,
                InstructorName = t.InstructorName,
                Venue = t.Venue,
                EnrolledParticipants = t.Participants.Count,
                MaxParticipants = t.MaxParticipants,
                IsK3MandatoryTraining = t.IsK3MandatoryTraining,
                RequiresGovernmentCertification = t.RequiresGovernmentCertification,
                AverageRating = t.AverageRating,
                IsOverdue = false, // Upcoming trainings cannot be overdue
                CanEdit = t.Status == TrainingStatus.Draft || t.Status == TrainingStatus.Scheduled,
                CanStart = t.Status == TrainingStatus.Scheduled && t.Participants.Count >= t.MinParticipants,
                CanEnroll = t.Status == TrainingStatus.Scheduled && t.Participants.Count < t.MaxParticipants,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy
            })
            .ToListAsync(cancellationToken);
    }
}