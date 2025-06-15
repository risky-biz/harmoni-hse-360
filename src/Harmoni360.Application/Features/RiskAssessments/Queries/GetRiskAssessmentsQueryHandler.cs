using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.RiskAssessments.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.RiskAssessments.Queries;

public class GetRiskAssessmentsQueryHandler : IRequestHandler<GetRiskAssessmentsQuery, GetRiskAssessmentsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PREFIX = "risk_assessments";
    private const string CACHE_TAG = "risk_assessments";

    public GetRiskAssessmentsQueryHandler(IApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<GetRiskAssessmentsResponse> Handle(GetRiskAssessmentsQuery request, CancellationToken cancellationToken)
    {
        // Generate cache key based on query parameters
        var cacheKey = _cache.GenerateKey(CACHE_KEY_PREFIX,
            request.PageNumber, request.PageSize, request.SearchTerm ?? "", request.Status ?? "", 
            request.RiskLevel ?? "", request.AssessmentType ?? "", request.HazardId?.ToString() ?? "",
            request.IsApproved?.ToString() ?? "", request.IsActive?.ToString() ?? "");

        // Try to get from cache first
        var cachedResponse = await _cache.GetAsync<GetRiskAssessmentsResponse>(cacheKey);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var query = _context.RiskAssessments
            .Include(ra => ra.Hazard)
            .Include(ra => ra.Assessor)
            .Include(ra => ra.ApprovedBy)
            .AsQueryable();

        // Apply filters
        if (request.IsActive.HasValue)
        {
            query = query.Where(ra => ra.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(ra =>
                ra.Hazard.Title.ToLower().Contains(searchTermLower) ||
                ra.PotentialConsequences.ToLower().Contains(searchTermLower) ||
                ra.RecommendedActions.ToLower().Contains(searchTermLower) ||
                ra.Assessor.Name.ToLower().Contains(searchTermLower));
        }

        if (request.IsApproved.HasValue)
        {
            query = query.Where(ra => ra.IsApproved == request.IsApproved.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.RiskLevel) && Enum.TryParse<RiskAssessmentLevel>(request.RiskLevel, out var riskLevel))
        {
            query = query.Where(ra => ra.RiskLevel == riskLevel);
        }

        if (!string.IsNullOrWhiteSpace(request.AssessmentType) && Enum.TryParse<RiskAssessmentType>(request.AssessmentType, out var assessmentType))
        {
            query = query.Where(ra => ra.Type == assessmentType);
        }

        if (request.HazardId.HasValue)
        {
            query = query.Where(ra => ra.HazardId == request.HazardId.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var riskAssessments = await query
            .OrderByDescending(ra => ra.AssessmentDate)
            .ThenByDescending(ra => ra.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var riskAssessmentDtos = riskAssessments.Select(ra => new RiskAssessmentDto
        {
            Id = ra.Id,
            HazardId = ra.HazardId,
            HazardTitle = ra.Hazard?.Title ?? string.Empty,
            Type = ra.Type.ToString(),
            AssessorName = ra.Assessor?.Name ?? string.Empty,
            AssessmentDate = ra.AssessmentDate,
            ProbabilityScore = ra.ProbabilityScore,
            SeverityScore = ra.SeverityScore,
            RiskScore = ra.RiskScore,
            RiskLevel = ra.RiskLevel.ToString(),
            PotentialConsequences = ra.PotentialConsequences,
            ExistingControls = ra.ExistingControls,
            RecommendedActions = ra.RecommendedActions,
            AdditionalNotes = ra.AdditionalNotes,
            NextReviewDate = ra.NextReviewDate,
            IsActive = ra.IsActive,
            IsApproved = ra.IsApproved,
            ApprovedByName = ra.ApprovedBy?.Name,
            ApprovedAt = ra.ApprovedAt,
            ApprovalNotes = ra.ApprovalNotes,
            CreatedAt = ra.CreatedAt,
            CreatedBy = ra.CreatedBy,
            LastModifiedAt = ra.LastModifiedAt,
            LastModifiedBy = ra.LastModifiedBy
        }).ToList();

        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var response = new GetRiskAssessmentsResponse
        {
            RiskAssessments = riskAssessmentDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages
        };

        // Cache the response with tags for easy invalidation
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), CACHE_TAG);

        return response;
    }
}