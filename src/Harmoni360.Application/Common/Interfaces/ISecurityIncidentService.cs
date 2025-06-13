using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Common.Interfaces;

public interface ISecurityIncidentService
{
    Task<SecurityIncidentDto> CreateIncidentAsync(CreateSecurityIncidentRequest request);
    Task<SecurityIncidentDto> UpdateIncidentAsync(int id, UpdateSecurityIncidentRequest request);
    Task<SecurityIncidentDetailDto> GetIncidentDetailAsync(int id);
    Task<bool> EscalateIncidentAsync(int incidentId, string reason, int escalatedById);
    Task<bool> AssignIncidentAsync(int incidentId, int assigneeId, int assignedById);
    Task<bool> CloseIncidentAsync(int incidentId, string rootCause, int closedById);
    Task<ThreatAssessmentDto> AssessThreatLevelAsync(int incidentId, CreateThreatAssessmentRequest request);
    Task<List<SecurityControlDto>> RecommendControlsAsync(int incidentId);
    Task<SecurityIncidentDto> LinkToSafetyIncidentAsync(int securityIncidentId, int safetyIncidentId);
    Task<List<SecurityIncidentDto>> GetRelatedIncidentsAsync(int incidentId);
    Task<bool> SendNotificationAsync(int incidentId, string message, List<int> recipientIds);
    Task<SecurityComplianceReportDto> GenerateComplianceReportAsync(int incidentId);
    Task<bool> ValidateIncidentDataAsync(int incidentId);
    Task<SecurityIncidentAnalyticsDto> GetIncidentAnalyticsAsync(int incidentId);
}

public class CreateSecurityIncidentRequest
{
    public SecurityIncidentType IncidentType { get; set; }
    public SecurityIncidentCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public DateTime IncidentDateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public ThreatActorType? ThreatActorType { get; set; }
    public string? ThreatActorDescription { get; set; }
    public bool IsInternalThreat { get; set; }
    public bool DataBreachSuspected { get; set; }
    public int? AffectedPersonsCount { get; set; }
    public decimal? EstimatedLoss { get; set; }
    public SecurityImpact Impact { get; set; }
    public int? AssignedToId { get; set; }
    public int? InvestigatorId { get; set; }
    public List<CreateAttachmentRequest>? Attachments { get; set; }
    public List<int>? InvolvedPersonIds { get; set; }
    public string? ContainmentActions { get; set; }
    public DateTime? DetectionDateTime { get; set; }
}

public class UpdateSecurityIncidentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public SecurityIncidentStatus Status { get; set; }
    public DateTime IncidentDateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public ThreatActorType? ThreatActorType { get; set; }
    public string? ThreatActorDescription { get; set; }
    public bool IsInternalThreat { get; set; }
    public bool DataBreachOccurred { get; set; }
    public int? AffectedPersonsCount { get; set; }
    public decimal? EstimatedLoss { get; set; }
    public SecurityImpact Impact { get; set; }
    public string? ContainmentActions { get; set; }
    public string? RootCause { get; set; }
    public DateTime? DetectionDateTime { get; set; }
    public DateTime? ContainmentDateTime { get; set; }
    public DateTime? ResolutionDateTime { get; set; }
    public int? AssignedToId { get; set; }
    public int? InvestigatorId { get; set; }
}

public class CreateThreatAssessmentRequest
{
    public ThreatLevel ThreatLevel { get; set; }
    public string AssessmentRationale { get; set; } = string.Empty;
    public int ThreatCapability { get; set; }
    public int ThreatIntent { get; set; }
    public int TargetVulnerability { get; set; }
    public int ImpactPotential { get; set; }
    public bool ExternalThreatIntelUsed { get; set; }
    public string? ThreatIntelSource { get; set; }
    public string? ThreatIntelDetails { get; set; }
    public DateTime? AssessmentDateTime { get; set; }
}

public class CreateAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public SecurityAttachmentType AttachmentType { get; set; }
    public string? Description { get; set; }
    public bool IsConfidential { get; set; } = true;
}

public class SecurityComplianceReportDto
{
    public int IncidentId { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public bool ISO27001Compliant { get; set; }
    public bool ITELawCompliant { get; set; }
    public bool SMK3Compliant { get; set; }
    public List<string> ComplianceIssues { get; set; } = new();
    public List<string> RequiredActions { get; set; } = new();
    public DateTime ReportGeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}

public class SecurityIncidentAnalyticsDto
{
    public int IncidentId { get; set; }
    public TimeSpan TimeToDetection { get; set; }
    public TimeSpan TimeToContainment { get; set; }
    public TimeSpan TimeToResolution { get; set; }
    public int ResponseActionsCount { get; set; }
    public decimal TotalCost { get; set; }
    public int TotalEffortHours { get; set; }
    public List<string> LessonsLearned { get; set; } = new();
    public List<string> ImprovementRecommendations { get; set; } = new();
    public double EffectivenessScore { get; set; }
}