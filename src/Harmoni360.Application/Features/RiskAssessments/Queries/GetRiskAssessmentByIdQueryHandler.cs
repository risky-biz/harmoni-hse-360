using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.RiskAssessments.DTOs;

namespace Harmoni360.Application.Features.RiskAssessments.Queries;

public class GetRiskAssessmentByIdQueryHandler : IRequestHandler<GetRiskAssessmentByIdQuery, RiskAssessmentDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public GetRiskAssessmentByIdQueryHandler(IApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<RiskAssessmentDetailDto?> Handle(GetRiskAssessmentByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = _cache.GenerateKey("risk_assessment_detail", request.Id);

        // Try to get from cache first
        var cachedResult = await _cache.GetAsync<RiskAssessmentDetailDto>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var riskAssessment = await _context.RiskAssessments
            .Include(ra => ra.Hazard)
                .ThenInclude(h => h.Category)
            .Include(ra => ra.Hazard)
                .ThenInclude(h => h.Type)
            .Include(ra => ra.Hazard)
                .ThenInclude(h => h.Reporter)
            .Include(ra => ra.Assessor)
            .Include(ra => ra.ApprovedBy)
            .FirstOrDefaultAsync(ra => ra.Id == request.Id, cancellationToken);

        if (riskAssessment == null)
        {
            return null;
        }

        var result = new RiskAssessmentDetailDto
        {
            Id = riskAssessment.Id,
            HazardId = riskAssessment.HazardId,
            HazardTitle = riskAssessment.Hazard?.Title ?? string.Empty,
            Type = riskAssessment.Type.ToString(),
            AssessorName = riskAssessment.Assessor?.Name ?? string.Empty,
            AssessmentDate = riskAssessment.AssessmentDate,
            ProbabilityScore = riskAssessment.ProbabilityScore,
            SeverityScore = riskAssessment.SeverityScore,
            RiskScore = riskAssessment.RiskScore,
            RiskLevel = riskAssessment.RiskLevel.ToString(),
            PotentialConsequences = riskAssessment.PotentialConsequences,
            ExistingControls = riskAssessment.ExistingControls,
            RecommendedActions = riskAssessment.RecommendedActions,
            AdditionalNotes = riskAssessment.AdditionalNotes,
            NextReviewDate = riskAssessment.NextReviewDate,
            IsActive = riskAssessment.IsActive,
            IsApproved = riskAssessment.IsApproved,
            ApprovedByName = riskAssessment.ApprovedBy?.Name,
            ApprovedAt = riskAssessment.ApprovedAt,
            ApprovalNotes = riskAssessment.ApprovalNotes,
            CreatedAt = riskAssessment.CreatedAt,
            CreatedBy = riskAssessment.CreatedBy,
            LastModifiedAt = riskAssessment.LastModifiedAt,
            LastModifiedBy = riskAssessment.LastModifiedBy,
            Hazard = riskAssessment.Hazard != null ? new HazardSummaryDto
            {
                Id = riskAssessment.Hazard.Id,
                Title = riskAssessment.Hazard.Title,
                Description = riskAssessment.Hazard.Description,
                Category = riskAssessment.Hazard.Category?.Name ?? string.Empty,
                Type = riskAssessment.Hazard.Type?.Name ?? string.Empty,
                Location = riskAssessment.Hazard.Location,
                Status = riskAssessment.Hazard.Status.ToString(),
                Severity = riskAssessment.Hazard.Severity.ToString(),
                IdentifiedDate = riskAssessment.Hazard.IdentifiedDate,
                ReporterName = riskAssessment.Hazard.Reporter?.Name ?? string.Empty
            } : new HazardSummaryDto(),
            Assessor = riskAssessment.Assessor != null ? new UserSummaryDto
            {
                Id = riskAssessment.Assessor.Id,
                Name = riskAssessment.Assessor.Name,
                Email = riskAssessment.Assessor.Email,
                Department = riskAssessment.Assessor.Department,
                Position = riskAssessment.Assessor.Position
            } : new UserSummaryDto(),
            ApprovedBy = riskAssessment.ApprovedBy != null ? new UserSummaryDto
            {
                Id = riskAssessment.ApprovedBy.Id,
                Name = riskAssessment.ApprovedBy.Name,
                Email = riskAssessment.ApprovedBy.Email,
                Department = riskAssessment.ApprovedBy.Department,
                Position = riskAssessment.ApprovedBy.Position
            } : null
        };

        // Cache the result
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), "risk_assessment_detail");

        return result;
    }
}