using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Hazards.DTOs;
using Harmoni360.Domain.Entities;
using System.Linq.Expressions;

namespace Harmoni360.Application.Features.Hazards.Queries;

public class GetHazardsQueryHandler : IRequestHandler<GetHazardsQuery, GetHazardsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public GetHazardsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<GetHazardsResponse> Handle(GetHazardsQuery request, CancellationToken cancellationToken)
    {
        // Create base query
        var query = _context.Hazards
            .Include(h => h.Reporter)
            .Include(h => h.CurrentRiskAssessment)
            .AsQueryable();

        // Apply conditional includes
        if (request.IncludeAttachments)
        {
            query = query.Include(h => h.Attachments);
        }

        if (request.IncludeRiskAssessments)
        {
            query = query.Include(h => h.RiskAssessments);
        }

        if (request.IncludeMitigationActions)
        {
            query = query.Include(h => h.MitigationActions)
                        .ThenInclude(ma => ma.AssignedTo);
        }

        // Apply filters
        query = ApplyFilters(query, request);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        // Apply pagination
        var pagedQuery = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        // Execute query
        var hazards = await pagedQuery.ToListAsync(cancellationToken);

        // Map to DTOs
        var hazardDtos = hazards.Select(h => MapToDto(h, request)).ToList();

        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        // Get summary statistics
        var summary = await GetHazardsSummary(cancellationToken);

        return new GetHazardsResponse
        {
            Hazards = hazardDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages,
            Summary = summary
        };
    }

    private IQueryable<Hazard> ApplyFilters(IQueryable<Hazard> query, GetHazardsQuery request)
    {
        // Text search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(h => 
                h.Title.ToLower().Contains(searchTerm) ||
                h.Description.ToLower().Contains(searchTerm) ||
                h.Location.ToLower().Contains(searchTerm) ||
                h.Reporter.Name.ToLower().Contains(searchTerm) ||
                h.ReporterDepartment.ToLower().Contains(searchTerm));
        }

        // Category filter
        if (request.Category.HasValue)
        {
            query = query.Where(h => h.Category == request.Category.Value);
        }

        // Type filter
        if (request.Type.HasValue)
        {
            query = query.Where(h => h.Type == request.Type.Value);
        }

        // Status filter
        if (request.Status.HasValue)
        {
            query = query.Where(h => h.Status == request.Status.Value);
        }

        // Severity filter
        if (request.Severity.HasValue)
        {
            query = query.Where(h => h.Severity == request.Severity.Value);
        }

        // Risk level filter
        if (request.RiskLevel.HasValue)
        {
            query = query.Where(h => h.CurrentRiskAssessment != null && 
                                   h.CurrentRiskAssessment.RiskLevel == request.RiskLevel.Value);
        }

        // Location filter
        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(h => h.Location.ToLower().Contains(request.Location.ToLower()));
        }

        // Department filter
        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(h => h.ReporterDepartment.ToLower().Contains(request.Department.ToLower()));
        }

        // Reporter filter
        if (request.ReporterId.HasValue)
        {
            query = query.Where(h => h.ReporterId == request.ReporterId.Value);
        }

        // Assigned to filter (for mitigation actions)
        if (request.AssignedToId.HasValue)
        {
            query = query.Where(h => h.MitigationActions.Any(ma => ma.AssignedToId == request.AssignedToId.Value));
        }

        // Date range filters
        if (request.IdentifiedDateFrom.HasValue)
        {
            query = query.Where(h => h.IdentifiedDate >= request.IdentifiedDateFrom.Value);
        }

        if (request.IdentifiedDateTo.HasValue)
        {
            query = query.Where(h => h.IdentifiedDate <= request.IdentifiedDateTo.Value);
        }

        if (request.ExpectedResolutionDateFrom.HasValue)
        {
            query = query.Where(h => h.ExpectedResolutionDate >= request.ExpectedResolutionDateFrom.Value);
        }

        if (request.ExpectedResolutionDateTo.HasValue)
        {
            query = query.Where(h => h.ExpectedResolutionDate <= request.ExpectedResolutionDateTo.Value);
        }

        // Geographic filter
        if (request.Latitude.HasValue && request.Longitude.HasValue && request.RadiusKm.HasValue)
        {
            // Simple distance calculation (for more accuracy, use spatial extensions)
            var lat = request.Latitude.Value;
            var lng = request.Longitude.Value;
            var radius = request.RadiusKm.Value;

            query = query.Where(h => h.GeoLocation != null &&
                Math.Sqrt(
                    Math.Pow(69.1 * (h.GeoLocation.Latitude - lat), 2) +
                    Math.Pow(69.1 * (lng - h.GeoLocation.Longitude) * Math.Cos(lat / 57.3), 2)
                ) <= radius);
        }

        // Special filters
        if (request.OnlyUnassessed)
        {
            query = query.Where(h => h.CurrentRiskAssessment == null);
        }

        if (request.OnlyOverdue)
        {
            query = query.Where(h => h.ExpectedResolutionDate.HasValue && 
                                   h.ExpectedResolutionDate.Value < DateTime.UtcNow &&
                                   h.Status != HazardStatus.Resolved &&
                                   h.Status != HazardStatus.Closed);
        }

        if (request.OnlyHighRisk)
        {
            query = query.Where(h => h.CurrentRiskAssessment != null &&
                                   (h.CurrentRiskAssessment.RiskLevel == RiskLevel.High ||
                                    h.CurrentRiskAssessment.RiskLevel == RiskLevel.Critical));
        }

        if (request.OnlyMyHazards)
        {
            var currentUserId = _currentUserService.UserId;
            if (currentUserId > 0)
            {
                query = query.Where(h => h.ReporterId == currentUserId ||
                                       h.MitigationActions.Any(ma => ma.AssignedToId == currentUserId));
            }
        }

        return query;
    }

    private static IQueryable<Hazard> ApplySorting(IQueryable<Hazard> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToUpper() == "DESC";

        return sortBy?.ToLower() switch
        {
            "title" => isDescending ? query.OrderByDescending(h => h.Title) : query.OrderBy(h => h.Title),
            "category" => isDescending ? query.OrderByDescending(h => h.Category) : query.OrderBy(h => h.Category),
            "severity" => isDescending ? query.OrderByDescending(h => h.Severity) : query.OrderBy(h => h.Severity),
            "status" => isDescending ? query.OrderByDescending(h => h.Status) : query.OrderBy(h => h.Status),
            "location" => isDescending ? query.OrderByDescending(h => h.Location) : query.OrderBy(h => h.Location),
            "reporter" => isDescending ? query.OrderByDescending(h => h.Reporter.Name) : query.OrderBy(h => h.Reporter.Name),
            "expectedresolutiondate" => isDescending ? query.OrderByDescending(h => h.ExpectedResolutionDate) : query.OrderBy(h => h.ExpectedResolutionDate),
            "risklevel" => isDescending ? query.OrderByDescending(h => h.CurrentRiskAssessment != null ? h.CurrentRiskAssessment.RiskLevel : 0) : query.OrderBy(h => h.CurrentRiskAssessment != null ? h.CurrentRiskAssessment.RiskLevel : 0),
            "createdat" => isDescending ? query.OrderByDescending(h => h.CreatedAt) : query.OrderBy(h => h.CreatedAt),
            _ => isDescending ? query.OrderByDescending(h => h.IdentifiedDate) : query.OrderBy(h => h.IdentifiedDate)
        };
    }

    private static HazardDto MapToDto(Hazard hazard, GetHazardsQuery request)
    {
        var dto = new HazardDto
        {
            Id = hazard.Id,
            Title = hazard.Title,
            Description = hazard.Description,
            Category = hazard.Category.ToString(),
            Type = hazard.Type.ToString(),
            Location = hazard.Location,
            Latitude = hazard.GeoLocation?.Latitude,
            Longitude = hazard.GeoLocation?.Longitude,
            Status = hazard.Status.ToString(),
            Severity = hazard.Severity.ToString(),
            IdentifiedDate = hazard.IdentifiedDate,
            ExpectedResolutionDate = hazard.ExpectedResolutionDate,
            ReporterName = hazard.Reporter.Name,
            ReporterEmail = hazard.Reporter.Email,
            ReporterDepartment = hazard.ReporterDepartment,
            CurrentRiskLevel = hazard.CurrentRiskAssessment?.RiskLevel.ToString(),
            CurrentRiskScore = hazard.CurrentRiskAssessment?.RiskScore,
            LastAssessmentDate = hazard.CurrentRiskAssessment?.AssessmentDate,
            AttachmentsCount = hazard.Attachments.Count,
            RiskAssessmentsCount = hazard.RiskAssessments.Count,
            MitigationActionsCount = hazard.MitigationActions.Count,
            PendingActionsCount = hazard.MitigationActions.Count(ma => ma.Status != MitigationActionStatus.Completed),
            CreatedAt = hazard.CreatedAt,
            CreatedBy = hazard.CreatedBy,
            LastModifiedAt = hazard.LastModifiedAt,
            LastModifiedBy = hazard.LastModifiedBy
        };

        if (request.IncludeReporter)
        {
            dto.Reporter = new UserDto
            {
                Id = hazard.Reporter.Id,
                Name = hazard.Reporter.Name,
                Email = hazard.Reporter.Email,
                Department = hazard.Reporter.Department,
                Position = hazard.Reporter.Position,
                EmployeeId = hazard.Reporter.EmployeeId
            };
        }

        if (hazard.CurrentRiskAssessment != null)
        {
            dto.CurrentRiskAssessment = new RiskAssessmentDto
            {
                Id = hazard.CurrentRiskAssessment.Id,
                HazardId = hazard.CurrentRiskAssessment.HazardId,
                Type = hazard.CurrentRiskAssessment.Type.ToString(),
                AssessmentDate = hazard.CurrentRiskAssessment.AssessmentDate,
                ProbabilityScore = hazard.CurrentRiskAssessment.ProbabilityScore,
                SeverityScore = hazard.CurrentRiskAssessment.SeverityScore,
                RiskScore = hazard.CurrentRiskAssessment.RiskScore,
                RiskLevel = hazard.CurrentRiskAssessment.RiskLevel.ToString(),
                NextReviewDate = hazard.CurrentRiskAssessment.NextReviewDate,
                IsActive = hazard.CurrentRiskAssessment.IsActive
            };
        }

        return dto;
    }

    private async Task<HazardsSummary> GetHazardsSummary(CancellationToken cancellationToken)
    {
        const string cacheKey = "hazards_summary";
        
        var cachedSummary = await _cacheService.GetAsync<HazardsSummary>(cacheKey);
        if (cachedSummary != null)
        {
            return cachedSummary;
        }

        var summary = new HazardsSummary
        {
            TotalHazards = await _context.Hazards.CountAsync(cancellationToken),
            OpenHazards = await _context.Hazards.CountAsync(h => 
                h.Status != HazardStatus.Resolved && h.Status != HazardStatus.Closed, cancellationToken),
            HighRiskHazards = await _context.Hazards.CountAsync(h => 
                h.CurrentRiskAssessment != null && 
                (h.CurrentRiskAssessment.RiskLevel == RiskLevel.High || 
                 h.CurrentRiskAssessment.RiskLevel == RiskLevel.Critical), cancellationToken),
            UnassessedHazards = await _context.Hazards.CountAsync(h => 
                h.CurrentRiskAssessment == null, cancellationToken),
            OverdueActions = await _context.HazardMitigationActions.CountAsync(ma => 
                ma.TargetDate < DateTime.UtcNow && 
                ma.Status != MitigationActionStatus.Completed, cancellationToken)
        };

        // Get counts by category
        summary.HazardsByCategory = await _context.Hazards
            .GroupBy(h => h.Category)
            .Select(g => new { Category = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count, cancellationToken);

        // Get counts by severity
        summary.HazardsBySeverity = await _context.Hazards
            .GroupBy(h => h.Severity)
            .Select(g => new { Severity = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity, x => x.Count, cancellationToken);

        // Get counts by status
        summary.HazardsByStatus = await _context.Hazards
            .GroupBy(h => h.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(5));

        return summary;
    }
}