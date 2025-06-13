using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public class UpdateThreatAssessmentCommandHandler : IRequestHandler<UpdateThreatAssessmentCommand, ThreatAssessmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public UpdateThreatAssessmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<ThreatAssessmentDto> Handle(UpdateThreatAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _context.ThreatAssessments
            .Include(ta => ta.SecurityIncident)
            .FirstOrDefaultAsync(ta => ta.Id == request.Id, cancellationToken);

        if (assessment == null)
        {
            throw new KeyNotFoundException($"Threat assessment with ID {request.Id} not found");
        }

        // Update the assessment risk factors
        assessment.UpdateRiskFactors(
            request.ThreatCapability,
            request.ThreatIntent,
            request.TargetVulnerability,
            request.ImpactPotential,
            _currentUserService.Name);

        // Update threat intelligence if provided
        if (request.ExternalThreatIntelUsed)
        {
            assessment.AddThreatIntelligence(
                request.ThreatIntelSource ?? string.Empty,
                request.ThreatIntelDetails ?? string.Empty,
                _currentUserService.Name);
        }

        // Update the related security incident's threat assessment
        assessment.SecurityIncident.UpdateThreatAssessment(
            assessment.CurrentThreatLevel,
            null,
            null,
            false,
            _currentUserService.Name);

        // Save changes
        await _context.SaveChangesAsync(cancellationToken);

        // Publish domain events
        foreach (var domainEvent in assessment.DomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        // Map to DTO
        return await MapToDto(assessment, cancellationToken);
    }

    private async Task<ThreatAssessmentDto> MapToDto(Domain.Entities.Security.ThreatAssessment assessment, CancellationToken cancellationToken)
    {
        var assessedBy = await _context.Users
            .Where(u => u.Id == assessment.AssessedById)
            .Select(u => new { u.Name })
            .FirstOrDefaultAsync(cancellationToken);

        return new ThreatAssessmentDto
        {
            Id = assessment.Id,
            CurrentThreatLevel = assessment.CurrentThreatLevel,
            PreviousThreatLevel = assessment.PreviousThreatLevel,
            AssessmentRationale = assessment.AssessmentRationale,
            AssessmentDateTime = assessment.AssessmentDateTime,
            AssessedByName = assessedBy?.Name ?? "Unknown",
            ExternalThreatIntelUsed = assessment.ExternalThreatIntelUsed,
            ThreatIntelSource = assessment.ThreatIntelSource,
            ThreatIntelDetails = assessment.ThreatIntelDetails,
            ThreatCapability = assessment.ThreatCapability,
            ThreatIntent = assessment.ThreatIntent,
            TargetVulnerability = assessment.TargetVulnerability,
            ImpactPotential = assessment.ImpactPotential
        };
    }
}