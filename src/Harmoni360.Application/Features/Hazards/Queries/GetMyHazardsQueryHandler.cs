using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Hazards.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Hazards.Queries;

public class GetMyHazardsQueryHandler : IRequestHandler<GetMyHazardsQuery, GetHazardsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;
    private const string MY_HAZARDS_CACHE_KEY_PREFIX = "my_hazards";
    private const string MY_HAZARDS_CACHE_TAG = "user_hazards";

    public GetMyHazardsQueryHandler(IApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<GetHazardsResponse> Handle(GetMyHazardsQuery request, CancellationToken cancellationToken)
    {
        // Generate cache key based on query parameters including user ID
        var cacheKey = _cache.GenerateKey(MY_HAZARDS_CACHE_KEY_PREFIX,
            request.UserId, request.PageNumber, request.PageSize, request.Status ?? "", request.Severity ?? "", request.SearchTerm ?? "");

        // Try to get from cache first
        var cachedResponse = await _cache.GetAsync<GetHazardsResponse>(cacheKey);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var query = _context.Hazards
            .Include(h => h.Reporter)
            .Include(h => h.Category)
            .Include(h => h.Type)
            .Include(h => h.CurrentRiskAssessment)
            .Include(h => h.RiskAssessments)
            .Include(h => h.Attachments)
            .Include(h => h.MitigationActions)
            .Where(h => h.ReporterId == request.UserId)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(h =>
                h.Title.ToLower().Contains(searchTermLower) ||
                h.Description.ToLower().Contains(searchTermLower) ||
                h.Location.ToLower().Contains(searchTermLower));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<HazardStatus>(request.Status, out var status))
        {
            query = query.Where(h => h.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Severity) && Enum.TryParse<HazardSeverity>(request.Severity, out var severity))
        {
            query = query.Where(h => h.Severity == severity);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var hazards = await query
            .OrderByDescending(h => h.IdentifiedDate)
            .ThenByDescending(h => h.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var hazardDtos = hazards.Select(h => new HazardDto
        {
            Id = h.Id,
            Title = h.Title,
            Description = h.Description,
            Category = h.Category?.Name ?? string.Empty,
            Type = h.Type?.Name ?? string.Empty,
            Location = h.Location,
            Latitude = h.GeoLocation?.Latitude,
            Longitude = h.GeoLocation?.Longitude,
            Status = h.Status.ToString(),
            Severity = h.Severity.ToString(),
            IdentifiedDate = h.IdentifiedDate,
            ExpectedResolutionDate = h.ExpectedResolutionDate,
            ReporterName = h.Reporter?.Name ?? string.Empty,
            ReporterEmail = h.Reporter?.Email,
            ReporterDepartment = h.ReporterDepartment,
            CurrentRiskLevel = h.CurrentRiskAssessment?.RiskLevel.ToString(),
            CurrentRiskScore = h.CurrentRiskAssessment?.RiskScore,
            LastAssessmentDate = h.CurrentRiskAssessment?.AssessmentDate,
            AttachmentsCount = h.Attachments.Count,
            RiskAssessmentsCount = h.RiskAssessments?.Count ?? 0,
            MitigationActionsCount = h.MitigationActions.Count,
            PendingActionsCount = h.MitigationActions.Count(a => a.Status != MitigationActionStatus.Completed),
            CreatedAt = h.CreatedAt,
            CreatedBy = h.CreatedBy,
            LastModifiedAt = h.LastModifiedAt,
            LastModifiedBy = h.LastModifiedBy
        }).ToList();

        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var response = new GetHazardsResponse
        {
            Hazards = hazardDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages,
            Summary = new HazardsSummary
            {
                TotalHazards = totalCount,
                OpenHazards = hazards.Count(h => h.Status != HazardStatus.Closed && h.Status != HazardStatus.Resolved),
                HighRiskHazards = hazards.Count(h => h.Severity == HazardSeverity.Major || h.Severity == HazardSeverity.Catastrophic),
                UnassessedHazards = hazards.Count(h => h.CurrentRiskAssessment == null)
            }
        };

        // Cache the response with tags for easy invalidation
        var userTag = $"user_{request.UserId}_hazards";
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), MY_HAZARDS_CACHE_TAG, userTag);

        return response;
    }
}