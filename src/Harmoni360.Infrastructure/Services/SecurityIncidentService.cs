using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.SecurityIncidents.Commands;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Application.Features.SecurityIncidents.Queries;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services;

public class SecurityIncidentService : ISecurityIncidentService
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SecurityIncidentService> _logger;

    public SecurityIncidentService(
        IMediator mediator,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        ILogger<SecurityIncidentService> logger)
    {
        _mediator = mediator;
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<SecurityIncidentDto> CreateIncidentAsync(CreateSecurityIncidentRequest request)
    {
        _logger.LogInformation("Creating security incident: {Title}", request.Title);

        var command = new CreateSecurityIncidentCommand
        {
            IncidentType = request.IncidentType,
            Category = request.Category,
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            IncidentDateTime = request.IncidentDateTime,
            Location = request.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ThreatActorType = request.ThreatActorType,
            ThreatActorDescription = request.ThreatActorDescription,
            IsInternalThreat = request.IsInternalThreat,
            DataBreachSuspected = request.DataBreachSuspected,
            AffectedPersonsCount = request.AffectedPersonsCount,
            EstimatedLoss = request.EstimatedLoss,
            Impact = request.Impact,
            AssignedToId = request.AssignedToId,
            InvestigatorId = request.InvestigatorId,
            InvolvedPersonIds = request.InvolvedPersonIds,
            ContainmentActions = request.ContainmentActions,
            DetectionDateTime = request.DetectionDateTime
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("Security incident created successfully: {IncidentNumber}", result.IncidentNumber);

        // Send automatic notifications for high severity incidents
        if (request.Severity >= SecuritySeverity.High)
        {
            await SendAutomaticNotifications(result.Id, result.Severity);
        }

        return result;
    }

    public async Task<SecurityIncidentDto> UpdateIncidentAsync(int id, UpdateSecurityIncidentRequest request)
    {
        _logger.LogInformation("Updating security incident: {IncidentId}", id);

        var command = new UpdateSecurityIncidentCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            Status = request.Status,
            IncidentDateTime = request.IncidentDateTime,
            Location = request.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ThreatActorType = request.ThreatActorType,
            ThreatActorDescription = request.ThreatActorDescription,
            IsInternalThreat = request.IsInternalThreat,
            DataBreachOccurred = request.DataBreachOccurred,
            AffectedPersonsCount = request.AffectedPersonsCount,
            EstimatedLoss = request.EstimatedLoss,
            Impact = request.Impact,
            ContainmentActions = request.ContainmentActions,
            RootCause = request.RootCause,
            DetectionDateTime = request.DetectionDateTime,
            ContainmentDateTime = request.ContainmentDateTime,
            ResolutionDateTime = request.ResolutionDateTime,
            AssignedToId = request.AssignedToId,
            InvestigatorId = request.InvestigatorId
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("Security incident updated successfully: {IncidentId}", id);

        return result;
    }

    public async Task<SecurityIncidentDetailDto> GetIncidentDetailAsync(int id)
    {
        var incident = await _context.SecurityIncidents
            .Include(i => i.Reporter)
            .Include(i => i.AssignedTo)
            .Include(i => i.Investigator)
            .Include(i => i.Attachments)
            .Include(i => i.Responses)
            .Include(i => i.InvolvedPersons)
            .Include(i => i.ImplementedControls)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {id} not found");
        }

        return MapToDetailDto(incident);
    }

    public async Task<bool> EscalateIncidentAsync(int incidentId, string reason, int escalatedById)
    {
        _logger.LogInformation("Escalating security incident: {IncidentId}", incidentId);

        var incident = await _context.SecurityIncidents.FindAsync(incidentId);
        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {incidentId} not found");
        }

        // Create escalation response record - using available response types
        var response = SecurityIncidentResponse.Create(
            incidentId,
            SecurityResponseType.Initial,
            $"Incident escalated. Reason: {reason}",
            DateTime.UtcNow,
            escalatedById);

        _context.SecurityIncidentResponses.Add(response);
        await _context.SaveChangesAsync();

        // Send escalation notifications
        await _notificationService.SendSecurityEscalationNotificationAsync(incidentId, reason);

        _logger.LogInformation("Security incident escalated successfully: {IncidentId}", incidentId);

        return true;
    }

    public async Task<bool> AssignIncidentAsync(int incidentId, int assigneeId, int assignedById)
    {
        _logger.LogInformation("Assigning security incident {IncidentId} to user {AssigneeId}", incidentId, assigneeId);

        var incident = await _context.SecurityIncidents.FindAsync(incidentId);
        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {incidentId} not found");
        }

        incident.AssignTo(assigneeId, _currentUserService.Name);

        await _context.SaveChangesAsync();

        // Send assignment notification
        await _notificationService.SendSecurityAssignmentNotificationAsync(incidentId, assigneeId);

        _logger.LogInformation("Security incident assigned successfully: {IncidentId}", incidentId);

        return true;
    }

    public async Task<bool> CloseIncidentAsync(int incidentId, string rootCause, int closedById)
    {
        _logger.LogInformation("Closing security incident: {IncidentId}", incidentId);

        var incident = await _context.SecurityIncidents.FindAsync(incidentId);
        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {incidentId} not found");
        }

        incident.ResolveIncident(rootCause, DateTime.UtcNow, _currentUserService.Name);

        // Create closure response record
        var response = SecurityIncidentResponse.Create(
            incidentId,
            SecurityResponseType.LessonsLearned,
            $"Incident closed. Root cause: {rootCause}",
            DateTime.UtcNow,
            closedById);

        _context.SecurityIncidentResponses.Add(response);
        await _context.SaveChangesAsync();

        // Publish domain event for incident closure
        await _mediator.Publish(new SecurityIncidentResolvedEvent(incident));

        _logger.LogInformation("Security incident closed successfully: {IncidentId}", incidentId);

        return true;
    }

    public async Task<ThreatAssessmentDto> AssessThreatLevelAsync(int incidentId, CreateThreatAssessmentRequest request)
    {
        _logger.LogInformation("Creating threat assessment for incident: {IncidentId}", incidentId);

        var command = new CreateThreatAssessmentCommand
        {
            SecurityIncidentId = incidentId,
            ThreatLevel = request.ThreatLevel,
            AssessmentRationale = request.AssessmentRationale,
            ThreatCapability = request.ThreatCapability,
            ThreatIntent = request.ThreatIntent,
            TargetVulnerability = request.TargetVulnerability,
            ImpactPotential = request.ImpactPotential,
            ExternalThreatIntelUsed = request.ExternalThreatIntelUsed,
            ThreatIntelSource = request.ThreatIntelSource,
            ThreatIntelDetails = request.ThreatIntelDetails,
            AssessmentDateTime = request.AssessmentDateTime
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("Threat assessment created successfully for incident: {IncidentId}", incidentId);

        return result;
    }

    public async Task<List<SecurityControlDto>> RecommendControlsAsync(int incidentId)
    {
        var incident = await _context.SecurityIncidents.FindAsync(incidentId);
        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {incidentId} not found");
        }

        var recommendations = new List<SecurityControlDto>();

        // Generate recommendations based on incident type and category
        switch (incident.IncidentType)
        {
            case SecurityIncidentType.PhysicalSecurity:
                recommendations.AddRange(GetPhysicalSecurityControlRecommendations(incident));
                break;
            case SecurityIncidentType.Cybersecurity:
                recommendations.AddRange(GetCybersecurityControlRecommendations(incident));
                break;
            case SecurityIncidentType.PersonnelSecurity:
                recommendations.AddRange(GetPersonnelSecurityControlRecommendations(incident));
                break;
            case SecurityIncidentType.InformationSecurity:
                recommendations.AddRange(GetInformationSecurityControlRecommendations(incident));
                break;
        }

        return recommendations;
    }

    public async Task<SecurityIncidentDto> LinkToSafetyIncidentAsync(int securityIncidentId, int safetyIncidentId)
    {
        var securityIncident = await _context.SecurityIncidents.FindAsync(securityIncidentId);
        if (securityIncident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {securityIncidentId} not found");
        }

        // Create a response record documenting the link
        var response = SecurityIncidentResponse.Create(
            securityIncidentId,
            SecurityResponseType.Initial,
            $"Linked to Safety Incident ID: {safetyIncidentId}",
            DateTime.UtcNow,
            _currentUserService.UserId);

        _context.SecurityIncidentResponses.Add(response);
        await _context.SaveChangesAsync();

        return await _mediator.Send(new UpdateSecurityIncidentCommand { Id = securityIncidentId });
    }

    public async Task<List<SecurityIncidentDto>> GetRelatedIncidentsAsync(int incidentId)
    {
        var incident = await _context.SecurityIncidents.FindAsync(incidentId);
        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {incidentId} not found");
        }

        // Find related incidents based on various criteria
        var relatedIncidents = await _context.SecurityIncidents
            .Where(i => i.Id != incidentId &&
                       (i.Location == incident.Location ||
                        i.ThreatActorType == incident.ThreatActorType ||
                        (i.IncidentDateTime >= incident.IncidentDateTime.AddDays(-7) &&
                         i.IncidentDateTime <= incident.IncidentDateTime.AddDays(7))))
            .Take(10)
            .ToListAsync();

        return relatedIncidents.Select(MapToDto).ToList();
    }

    public async Task<bool> SendNotificationAsync(int incidentId, string message, List<int> recipientIds)
    {
        await _notificationService.SendSecurityIncidentNotificationAsync(incidentId, message, recipientIds);
        return true;
    }

    public async Task<SecurityComplianceReportDto> GenerateComplianceReportAsync(int incidentId)
    {
        var incident = await _context.SecurityIncidents.FindAsync(incidentId);
        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {incidentId} not found");
        }

        return new SecurityComplianceReportDto
        {
            IncidentId = incidentId,
            IncidentNumber = incident.IncidentNumber,
            ISO27001Compliant = ValidateISO27001Compliance(incident),
            ITELawCompliant = ValidateITELawCompliance(incident),
            SMK3Compliant = ValidateSMK3Compliance(incident),
            ComplianceIssues = GetComplianceIssues(incident),
            RequiredActions = GetRequiredActions(incident),
            ReportGeneratedAt = DateTime.UtcNow,
            GeneratedBy = _currentUserService.Name ?? "System"
        };
    }

    public async Task<bool> ValidateIncidentDataAsync(int incidentId)
    {
        var incident = await _context.SecurityIncidents.FindAsync(incidentId);
        if (incident == null)
        {
            return false;
        }

        // Perform data validation checks
        var validationIssues = new List<string>();

        if (string.IsNullOrEmpty(incident.Title))
            validationIssues.Add("Title is required");

        if (string.IsNullOrEmpty(incident.Description))
            validationIssues.Add("Description is required");

        if (incident.Severity == SecuritySeverity.Critical && incident.AssignedToId == null)
            validationIssues.Add("Critical incidents must be assigned");

        return validationIssues.Count == 0;
    }

    public async Task<SecurityIncidentAnalyticsDto> GetIncidentAnalyticsAsync(int incidentId)
    {
        var incident = await _context.SecurityIncidents
            .Include(i => i.Responses)
            .FirstOrDefaultAsync(i => i.Id == incidentId);

        if (incident == null)
        {
            throw new KeyNotFoundException($"Security incident with ID {incidentId} not found");
        }

        var timeToDetection = incident.DetectionDateTime.HasValue
            ? incident.DetectionDateTime.Value - incident.IncidentDateTime
            : TimeSpan.Zero;

        var timeToContainment = incident.ContainmentDateTime.HasValue
            ? incident.ContainmentDateTime.Value - incident.IncidentDateTime
            : TimeSpan.Zero;

        var timeToResolution = incident.ResolutionDateTime.HasValue
            ? incident.ResolutionDateTime.Value - incident.IncidentDateTime
            : TimeSpan.Zero;

        return new SecurityIncidentAnalyticsDto
        {
            IncidentId = incidentId,
            TimeToDetection = timeToDetection,
            TimeToContainment = timeToContainment,
            TimeToResolution = timeToResolution,
            ResponseActionsCount = incident.Responses.Count,
            TotalCost = incident.Responses.Sum(r => r.Cost ?? 0),
            TotalEffortHours = incident.Responses.Sum(r => r.EffortHours ?? 0),
            LessonsLearned = GetLessonsLearned(incident),
            ImprovementRecommendations = GetImprovementRecommendations(incident),
            EffectivenessScore = CalculateEffectivenessScore(incident)
        };
    }

    #region Private Helper Methods

    private async Task SendAutomaticNotifications(int incidentId, SecuritySeverity severity)
    {
        // Send notifications to security team and management for high severity incidents
        var notificationMessage = $"High severity security incident created. Incident ID: {incidentId}";
        await _notificationService.SendSecurityIncidentNotificationAsync(incidentId, notificationMessage, new List<int>());
    }

    private List<SecurityControlDto> GetPhysicalSecurityControlRecommendations(SecurityIncident incident)
    {
        return new List<SecurityControlDto>
        {
            new() { ControlName = "Enhanced Access Control", ControlType = SecurityControlType.Preventive, Category = SecurityControlCategory.Physical },
            new() { ControlName = "CCTV Monitoring", ControlType = SecurityControlType.Detective, Category = SecurityControlCategory.Technical },
            new() { ControlName = "Security Guard Patrol", ControlType = SecurityControlType.Detective, Category = SecurityControlCategory.Administrative }
        };
    }

    private List<SecurityControlDto> GetCybersecurityControlRecommendations(SecurityIncident incident)
    {
        return new List<SecurityControlDto>
        {
            new() { ControlName = "Network Segmentation", ControlType = SecurityControlType.Preventive, Category = SecurityControlCategory.Technical },
            new() { ControlName = "Intrusion Detection System", ControlType = SecurityControlType.Detective, Category = SecurityControlCategory.Technical },
            new() { ControlName = "Incident Response Plan", ControlType = SecurityControlType.Corrective, Category = SecurityControlCategory.Administrative }
        };
    }

    private List<SecurityControlDto> GetPersonnelSecurityControlRecommendations(SecurityIncident incident)
    {
        return new List<SecurityControlDto>
        {
            new() { ControlName = "Enhanced Background Checks", ControlType = SecurityControlType.Preventive, Category = SecurityControlCategory.Administrative },
            new() { ControlName = "Security Awareness Training", ControlType = SecurityControlType.Preventive, Category = SecurityControlCategory.Administrative },
            new() { ControlName = "User Activity Monitoring", ControlType = SecurityControlType.Detective, Category = SecurityControlCategory.Technical }
        };
    }

    private List<SecurityControlDto> GetInformationSecurityControlRecommendations(SecurityIncident incident)
    {
        return new List<SecurityControlDto>
        {
            new() { ControlName = "Data Encryption", ControlType = SecurityControlType.Preventive, Category = SecurityControlCategory.Technical },
            new() { ControlName = "Data Loss Prevention", ControlType = SecurityControlType.Detective, Category = SecurityControlCategory.Technical },
            new() { ControlName = "Data Classification Policy", ControlType = SecurityControlType.Preventive, Category = SecurityControlCategory.Administrative }
        };
    }

    private bool ValidateISO27001Compliance(SecurityIncident incident)
    {
        // Basic ISO 27001 compliance checks
        return !string.IsNullOrEmpty(incident.Description) &&
               incident.ContainmentDateTime.HasValue;
    }

    private bool ValidateITELawCompliance(SecurityIncident incident)
    {
        // Indonesian ITE Law compliance checks
        return incident.DataBreachOccurred == false ||
               (incident.DataBreachOccurred && incident.DetectionDateTime.HasValue);
    }

    private bool ValidateSMK3Compliance(SecurityIncident incident)
    {
        // SMK3 compliance checks
        return incident.RootCause != null &&
               incident.Status == SecurityIncidentStatus.Closed;
    }

    private List<string> GetComplianceIssues(SecurityIncident incident)
    {
        var issues = new List<string>();

        if (incident.DataBreachOccurred && !incident.DetectionDateTime.HasValue)
            issues.Add("Data breach incidents must have detection time recorded");

        if (incident.Severity >= SecuritySeverity.High && string.IsNullOrEmpty(incident.RootCause))
            issues.Add("High severity incidents require root cause analysis");

        return issues;
    }

    private List<string> GetRequiredActions(SecurityIncident incident)
    {
        var actions = new List<string>();

        if (incident.Status != SecurityIncidentStatus.Closed)
            actions.Add("Complete incident investigation and resolution");

        if (string.IsNullOrEmpty(incident.RootCause))
            actions.Add("Conduct root cause analysis");

        return actions;
    }

    private List<string> GetLessonsLearned(SecurityIncident incident)
    {
        return new List<string>
        {
            "Improve incident detection mechanisms",
            "Enhance security awareness training",
            "Review and update security policies"
        };
    }

    private List<string> GetImprovementRecommendations(SecurityIncident incident)
    {
        return new List<string>
        {
            "Implement additional security controls",
            "Enhance monitoring capabilities",
            "Improve incident response procedures"
        };
    }

    private double CalculateEffectivenessScore(SecurityIncident incident)
    {
        var score = 100.0;

        // Deduct points for delays
        if (incident.ContainmentDateTime.HasValue)
        {
            var containmentTime = incident.ContainmentDateTime.Value - incident.IncidentDateTime;
            if (containmentTime.TotalHours > 24)
                score -= 20;
        }

        if (incident.ResolutionDateTime.HasValue)
        {
            var resolutionTime = incident.ResolutionDateTime.Value - incident.IncidentDateTime;
            if (resolutionTime.TotalDays > 7)
                score -= 30;
        }

        return Math.Max(0, score);
    }

    private SecurityIncidentDetailDto MapToDetailDto(SecurityIncident incident)
    {
        return new SecurityIncidentDetailDto
        {
            Id = incident.Id,
            IncidentNumber = incident.IncidentNumber,
            Title = incident.Title,
            Description = incident.Description,
            IncidentType = incident.IncidentType,
            Category = incident.Category,
            Severity = incident.Severity,
            Status = incident.Status,
            ThreatLevel = incident.ThreatLevel,
            IncidentDateTime = incident.IncidentDateTime,
            DetectionDateTime = incident.DetectionDateTime,
            Location = incident.Location,
            Latitude = incident.GeoLocation?.Latitude,
            Longitude = incident.GeoLocation?.Longitude,
            ThreatActorType = incident.ThreatActorType,
            ThreatActorDescription = incident.ThreatActorDescription,
            IsInternalThreat = incident.IsInternalThreat,
            Impact = incident.Impact,
            AffectedPersonsCount = incident.AffectedPersonsCount,
            EstimatedLoss = incident.EstimatedLoss,
            DataBreachOccurred = incident.DataBreachOccurred,
            ContainmentDateTime = incident.ContainmentDateTime,
            ResolutionDateTime = incident.ResolutionDateTime,
            ContainmentActions = incident.ContainmentActions,
            RootCause = incident.RootCause,
            ReporterName = $"{incident.Reporter.Name} {incident.Reporter.Name}".Trim(),
            ReporterEmail = incident.Reporter.Email,
            AssignedToName = incident.AssignedTo != null ? $"{incident.AssignedTo.Name} {incident.AssignedTo.Name}".Trim() : null,
            InvestigatorName = incident.Investigator != null ? $"{incident.Investigator.Name} {incident.Investigator.Name}".Trim() : null,
            CreatedAt = incident.CreatedAt,
            LastModifiedAt = incident.LastModifiedAt
        };
    }

    private SecurityIncidentDto MapToDto(SecurityIncident incident)
    {
        return new SecurityIncidentDto
        {
            Id = incident.Id,
            IncidentNumber = incident.IncidentNumber,
            Title = incident.Title,
            Description = incident.Description,
            IncidentType = incident.IncidentType,
            Category = incident.Category,
            Severity = incident.Severity,
            Status = incident.Status,
            ThreatLevel = incident.ThreatLevel,
            IncidentDateTime = incident.IncidentDateTime,
            DetectionDateTime = incident.DetectionDateTime,
            Location = incident.Location,
            CreatedAt = incident.CreatedAt,
            LastModifiedAt = incident.LastModifiedAt
        };
    }

    #endregion
}