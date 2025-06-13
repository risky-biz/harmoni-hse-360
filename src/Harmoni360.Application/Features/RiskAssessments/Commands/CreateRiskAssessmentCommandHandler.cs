using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.RiskAssessments.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.RiskAssessments.Commands;

public class CreateRiskAssessmentCommandHandler : IRequestHandler<CreateRiskAssessmentCommand, RiskAssessmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateRiskAssessmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<RiskAssessmentDto> Handle(CreateRiskAssessmentCommand request, CancellationToken cancellationToken)
    {
        // Verify hazard exists and user has access
        var hazard = await _context.Hazards
            .FirstOrDefaultAsync(h => h.Id == request.HazardId, cancellationToken);

        if (hazard == null)
        {
            throw new InvalidOperationException($"Hazard with ID {request.HazardId} not found");
        }

        // Convert string type to enum
        if (!Enum.TryParse<RiskAssessmentType>(request.Type, out var assessmentType))
        {
            throw new ArgumentException($"Invalid assessment type: {request.Type}");
        }

        // Get current user ID (default to 1 if not available)
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == 0)
        {
            currentUserId = 1;
        }

        // Create the risk assessment entity using the domain method
        var riskAssessment = RiskAssessment.Create(
            hazardId: request.HazardId,
            type: assessmentType,
            assessorId: currentUserId,
            probabilityScore: request.ProbabilityScore,
            severityScore: request.SeverityScore,
            potentialConsequences: request.PotentialConsequences,
            existingControls: request.ExistingControls,
            recommendedActions: request.RecommendedActions ?? string.Empty,
            additionalNotes: request.AdditionalNotes ?? string.Empty
        );

        // Update the assessment date if different from default
        if (request.AssessmentDate != DateTime.Today)
        {
            // Convert to UTC before setting
            var assessmentDateUtc = DateTime.SpecifyKind(request.AssessmentDate, DateTimeKind.Utc);
            
            // Use reflection to set the assessment date since it's private
            var assessmentDateProperty = typeof(RiskAssessment).GetProperty("AssessmentDate");
            assessmentDateProperty?.SetValue(riskAssessment, assessmentDateUtc);
        }

        // Set next review date if provided
        if (request.NextReviewDate != default)
        {
            // Convert to UTC before scheduling
            var nextReviewDateUtc = DateTime.SpecifyKind(request.NextReviewDate, DateTimeKind.Utc);
            riskAssessment.ScheduleReview(nextReviewDateUtc);
        }

        _context.RiskAssessments.Add(riskAssessment);
        await _context.SaveChangesAsync(cancellationToken);

        // Get assessor name from Users table
        var assessor = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        // Return DTO
        return new RiskAssessmentDto
        {
            Id = riskAssessment.Id,
            HazardId = riskAssessment.HazardId,
            HazardTitle = hazard.Title,
            Type = riskAssessment.Type.ToString(),
            AssessorName = assessor?.Name ?? request.AssessorName,
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
            CreatedAt = riskAssessment.CreatedAt,
            CreatedBy = riskAssessment.CreatedBy
        };
    }
}