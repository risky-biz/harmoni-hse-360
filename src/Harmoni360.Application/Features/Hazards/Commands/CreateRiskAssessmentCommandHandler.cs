using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Hazards.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Hazards.Commands;

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
        // Verify hazard exists
        var hazard = await _context.Hazards
            .Include(h => h.Reporter)
            .FirstOrDefaultAsync(h => h.Id == request.HazardId, cancellationToken);

        if (hazard == null)
        {
            throw new ArgumentException($"Hazard with ID {request.HazardId} not found.");
        }

        // Verify assessor exists
        var assessor = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AssessorId, cancellationToken);

        if (assessor == null)
        {
            throw new ArgumentException($"Assessor with ID {request.AssessorId} not found.");
        }

        // Create the risk assessment using the static factory method
        var riskAssessment = RiskAssessment.Create(
            hazardId: request.HazardId,
            type: request.Type,
            assessorId: request.AssessorId,
            probabilityScore: request.ProbabilityScore,
            severityScore: request.SeverityScore,
            potentialConsequences: request.PotentialConsequences,
            existingControls: request.ExistingControls,
            recommendedActions: request.RecommendedActions,
            additionalNotes: request.AdditionalNotes ?? string.Empty
        );

        // Set next review date if provided, otherwise use calculated date
        if (request.NextReviewDate != default)
        {
            riskAssessment.ScheduleReview(request.NextReviewDate);
        }

        // Add to context
        _context.RiskAssessments.Add(riskAssessment);

        // If setting as current, deactivate other assessments and update hazard
        if (request.SetAsCurrent)
        {
            // Deactivate other active assessments for this hazard
            var existingAssessments = await _context.RiskAssessments
                .Where(ra => ra.HazardId == request.HazardId && ra.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingAssessments)
            {
                existing.Deactivate("Replaced by new assessment");
            }

            // Update hazard's current risk assessment through the entity method
            hazard.AddRiskAssessment(riskAssessment);

            // Update hazard severity if risk level is higher
            var newSeverity = MapRiskLevelToSeverity(riskAssessment.RiskLevel);
            if (newSeverity > hazard.Severity)
            {
                hazard.UpdateSeverity(newSeverity, _currentUserService.Name);
            }
        }

        // Save changes
        await _context.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var riskAssessmentDto = new RiskAssessmentDto
        {
            Id = riskAssessment.Id,
            HazardId = riskAssessment.HazardId,
            Type = riskAssessment.Type.ToString(),
            AssessorName = assessor.Name,
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
            Assessor = new UserDto
            {
                Id = assessor.Id,
                Name = assessor.Name,
                Email = assessor.Email,
                Department = assessor.Department,
                Position = assessor.Position,
                EmployeeId = assessor.EmployeeId
            }
        };

        return riskAssessmentDto;
    }

    private static HazardSeverity MapRiskLevelToSeverity(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.VeryLow => HazardSeverity.Negligible,
            RiskLevel.Low => HazardSeverity.Minor,
            RiskLevel.Medium => HazardSeverity.Moderate,
            RiskLevel.High => HazardSeverity.Major,
            RiskLevel.Critical => HazardSeverity.Catastrophic,
            _ => HazardSeverity.Minor
        };
    }
}