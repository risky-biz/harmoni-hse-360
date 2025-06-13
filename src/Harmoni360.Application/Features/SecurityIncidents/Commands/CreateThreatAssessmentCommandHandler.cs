using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.SecurityIncidents.Commands;

public class CreateThreatAssessmentCommandHandler : IRequestHandler<CreateThreatAssessmentCommand, ThreatAssessmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public CreateThreatAssessmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<ThreatAssessmentDto> Handle(CreateThreatAssessmentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        // Verify the security incident exists
        var incident = await _context.SecurityIncidents
            .FirstOrDefaultAsync(i => i.Id == request.SecurityIncidentId, cancellationToken);

        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {request.SecurityIncidentId} not found");
        }

        // Get the current threat level from the incident (for tracking previous level)
        var previousThreatLevel = incident.ThreatLevel;

        // Create the threat assessment
        var assessment = ThreatAssessment.Create(
            request.SecurityIncidentId,
            request.ThreatLevel,
            previousThreatLevel,
            request.AssessmentRationale,
            currentUserId,
            _currentUserService.Name);

        // Update risk factors
        assessment.UpdateRiskFactors(
            request.ThreatCapability,
            request.ThreatIntent,
            request.TargetVulnerability,
            request.ImpactPotential,
            _currentUserService.Name);

        // Set threat intelligence fields if provided
        if (request.ExternalThreatIntelUsed)
        {
            assessment.AddThreatIntelligence(
                request.ThreatIntelSource ?? string.Empty,
                request.ThreatIntelDetails ?? string.Empty,
                _currentUserService.Name);
        }

        // Update the incident's threat assessment
        incident.UpdateThreatAssessment(
            request.ThreatLevel,
            null,
            null,
            false,
            _currentUserService.Name);

        // Add the assessment to context
        _context.ThreatAssessments.Add(assessment);

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

    private async Task<ThreatAssessmentDto> MapToDto(ThreatAssessment assessment, CancellationToken cancellationToken)
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