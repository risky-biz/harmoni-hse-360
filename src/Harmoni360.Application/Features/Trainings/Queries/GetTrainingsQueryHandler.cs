using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Application.Features.Trainings.Services;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Queries;

public class GetTrainingsQueryHandler : IRequestHandler<GetTrainingsQuery, GetTrainingsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICachedTrainingService _cachedTrainingService;
    private readonly IPerformanceMetricsService _performanceMetrics;
    private readonly ILogger<GetTrainingsQueryHandler> _logger;

    public GetTrainingsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICachedTrainingService cachedTrainingService,
        IPerformanceMetricsService performanceMetrics,
        ILogger<GetTrainingsQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cachedTrainingService = cachedTrainingService;
        _performanceMetrics = performanceMetrics;
        _logger = logger;
    }

    public async Task<GetTrainingsResponse> Handle(GetTrainingsQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var query = _context.Trainings.AsQueryable();

            // Apply filters
            query = ApplyFilters(query, request);

            // Get total count before pagination
            var countStopwatch = Stopwatch.StartNew();
            var totalCount = await query.CountAsync(cancellationToken);
            countStopwatch.Stop();
            await _performanceMetrics.RecordQueryExecutionTimeAsync("GetTrainings_Count", countStopwatch.Elapsed);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination and get trainings
            var queryStopwatch = Stopwatch.StartNew();
            var trainings = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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
                    IsOverdue = t.Status == TrainingStatus.Scheduled && t.ScheduledStartDate < DateTime.Now,
                    CanEdit = t.Status == TrainingStatus.Draft || t.Status == TrainingStatus.Scheduled,
                    CanStart = t.Status == TrainingStatus.Scheduled && t.Participants.Count >= t.MinParticipants,
                    CanEnroll = t.Status == TrainingStatus.Scheduled && t.Participants.Count < t.MaxParticipants,
                    CreatedAt = t.CreatedAt,
                    CreatedBy = t.CreatedBy
                })
                .ToListAsync(cancellationToken);
            queryStopwatch.Stop();
            await _performanceMetrics.RecordQueryExecutionTimeAsync("GetTrainings_Query", queryStopwatch.Elapsed, trainings.Count);

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            // Get cached statistics
            var statistics = await _cachedTrainingService.GetCachedStatisticsAsync(cancellationToken);

            stopwatch.Stop();
            await _performanceMetrics.RecordQueryExecutionTimeAsync("GetTrainings_Total", stopwatch.Elapsed, trainings.Count);

            _logger.LogInformation("Retrieved {Count} trainings out of {Total} total trainings in {ElapsedMs}ms", 
                trainings.Count, totalCount, stopwatch.ElapsedMilliseconds);

            return new GetTrainingsResponse
            {
                Trainings = trainings,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages,
                Statistics = statistics
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await _performanceMetrics.RecordQueryExecutionTimeAsync("GetTrainings_Error", stopwatch.Elapsed);
            _logger.LogError(ex, "Error retrieving trainings for user {UserId} in {ElapsedMs}ms", 
                _currentUserService.UserId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static IQueryable<Training> ApplyFilters(IQueryable<Training> query, GetTrainingsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t => 
                t.Title.ToLower().Contains(searchTerm) ||
                t.Description.ToLower().Contains(searchTerm) ||
                t.TrainingCode.ToLower().Contains(searchTerm) ||
                t.InstructorName.ToLower().Contains(searchTerm) ||
                t.Venue.ToLower().Contains(searchTerm));
        }

        if (request.Type.HasValue)
        {
            query = query.Where(t => t.Type == request.Type.Value);
        }

        if (request.Category.HasValue)
        {
            query = query.Where(t => t.Category == request.Category.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == request.Priority.Value);
        }

        if (request.DeliveryMethod.HasValue)
        {
            query = query.Where(t => t.DeliveryMethod == request.DeliveryMethod.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.InstructorName))
        {
            query = query.Where(t => t.InstructorName.ToLower().Contains(request.InstructorName.ToLower()));
        }

        if (request.ScheduledFromDate.HasValue)
        {
            query = query.Where(t => t.ScheduledStartDate >= request.ScheduledFromDate.Value);
        }

        if (request.ScheduledToDate.HasValue)
        {
            query = query.Where(t => t.ScheduledStartDate <= request.ScheduledToDate.Value);
        }

        if (request.IsK3MandatoryTraining.HasValue)
        {
            query = query.Where(t => t.IsK3MandatoryTraining == request.IsK3MandatoryTraining.Value);
        }

        if (request.RequiresGovernmentCertification.HasValue)
        {
            query = query.Where(t => t.RequiresGovernmentCertification == request.RequiresGovernmentCertification.Value);
        }

        if (request.IssuesCertificate.HasValue)
        {
            query = query.Where(t => t.IssuesCertificate == request.IssuesCertificate.Value);
        }

        if (request.IsOverdue.HasValue && request.IsOverdue.Value)
        {
            query = query.Where(t => t.Status == TrainingStatus.Scheduled && t.ScheduledStartDate < DateTime.Now);
        }

        if (request.HasAvailableSpots.HasValue && request.HasAvailableSpots.Value)
        {
            query = query.Where(t => t.Status == TrainingStatus.Scheduled && t.Participants.Count < t.MaxParticipants);
        }

        return query;
    }

    private static IQueryable<Training> ApplySorting(IQueryable<Training> query, string sortBy, bool sortDescending)
    {
        query = sortBy.ToLowerInvariant() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "type" => sortDescending ? query.OrderByDescending(t => t.Type) : query.OrderBy(t => t.Type),
            "status" => sortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "scheduledstartdate" => sortDescending ? query.OrderByDescending(t => t.ScheduledStartDate) : query.OrderBy(t => t.ScheduledStartDate),
            "scheduledenddate" => sortDescending ? query.OrderByDescending(t => t.ScheduledEndDate) : query.OrderBy(t => t.ScheduledEndDate),
            "instructorname" => sortDescending ? query.OrderByDescending(t => t.InstructorName) : query.OrderBy(t => t.InstructorName),
            "venue" => sortDescending ? query.OrderByDescending(t => t.Venue) : query.OrderBy(t => t.Venue),
            "createdat" => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            "averagerating" => sortDescending ? query.OrderByDescending(t => t.AverageRating) : query.OrderBy(t => t.AverageRating),
            _ => sortDescending ? query.OrderByDescending(t => t.ScheduledStartDate) : query.OrderBy(t => t.ScheduledStartDate)
        };

        return query;
    }

    private async Task<TrainingStatisticsDto> GetStatistics(CancellationToken cancellationToken)
    {
        // Optimize statistics calculation using database aggregation instead of loading all data
        var statusCounts = await _context.Trainings
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalCount = await _context.Trainings.CountAsync(cancellationToken);
        var totalParticipants = await _context.TrainingParticipants.CountAsync(cancellationToken);
        var totalCertificates = await _context.TrainingCertifications.CountAsync(cancellationToken);
        
        var completedCount = statusCounts.FirstOrDefault(s => s.Status == TrainingStatus.Completed)?.Count ?? 0;
        
        // Calculate average rating for completed trainings only
        var averageRating = await _context.Trainings
            .Where(t => t.Status == TrainingStatus.Completed && t.AverageRating > 0)
            .AverageAsync(t => t.AverageRating, cancellationToken);

        // Calculate overdue trainings count using database query
        var overdueCount = await _context.Trainings
            .CountAsync(t => t.Status == TrainingStatus.Scheduled && t.ScheduledStartDate < DateTime.Now, cancellationToken);

        // Get type statistics with optimized query
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

        // Get category statistics with optimized query including participant and certificate counts
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
}