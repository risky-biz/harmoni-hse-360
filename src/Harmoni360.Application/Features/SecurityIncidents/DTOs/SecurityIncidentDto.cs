using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.SecurityIncidents.DTOs;

public class SecurityIncidentDto
{
    public int Id { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityIncidentType IncidentType { get; set; }
    public SecurityIncidentCategory Category { get; set; }
    public SecuritySeverity Severity { get; set; }
    public SecurityIncidentStatus Status { get; set; }
    public ThreatLevel ThreatLevel { get; set; }
    
    // Temporal Information
    public DateTime IncidentDateTime { get; set; }
    public DateTime? DetectionDateTime { get; set; }
    public DateTime? ContainmentDateTime { get; set; }
    public DateTime? ResolutionDateTime { get; set; }
    
    // Location Information
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Threat Actor Information
    public ThreatActorType? ThreatActorType { get; set; }
    public string? ThreatActorDescription { get; set; }
    public bool IsInternalThreat { get; set; }
    
    // Impact Assessment
    public SecurityImpact Impact { get; set; }
    public int? AffectedPersonsCount { get; set; }
    public decimal? EstimatedLoss { get; set; }
    public bool DataBreachOccurred { get; set; }
    
    // Response Information
    public string? ContainmentActions { get; set; }
    public string? RootCause { get; set; }
    
    // People
    public string? ReporterName { get; set; }
    public string? ReporterEmail { get; set; }
    public string? AssignedToName { get; set; }
    public string? InvestigatorName { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Additional Properties
    public int AttachmentCount { get; set; }
    public int ResponseCount { get; set; }
    public int InvolvedPersonCount { get; set; }
    public bool HasOpenTasks { get; set; }
    public int DaysOpen => (DateTime.UtcNow - CreatedAt).Days;
    public bool IsOverdue => Status != SecurityIncidentStatus.Closed && DaysOpen > GetSlaThreshold();
    
    private int GetSlaThreshold()
    {
        return Severity switch
        {
            SecuritySeverity.Critical => 1,
            SecuritySeverity.High => 3,
            SecuritySeverity.Medium => 7,
            SecuritySeverity.Low => 14,
            _ => 14
        };
    }
}

public class SecurityIncidentListDto
{
    public int Id { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public SecurityIncidentType IncidentType { get; set; }
    public SecurityIncidentCategory Category { get; set; }
    public SecuritySeverity Severity { get; set; }
    public SecurityIncidentStatus Status { get; set; }
    public ThreatLevel ThreatLevel { get; set; }
    public DateTime IncidentDateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ReporterName { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DaysOpen => (DateTime.UtcNow - CreatedAt).Days;
    public bool IsOverdue => Status != SecurityIncidentStatus.Closed && DaysOpen > GetSlaThreshold();
    
    private int GetSlaThreshold()
    {
        return Severity switch
        {
            SecuritySeverity.Critical => 1,
            SecuritySeverity.High => 3,
            SecuritySeverity.Medium => 7,
            SecuritySeverity.Low => 14,
            _ => 14
        };
    }
}

public class SecurityIncidentDetailDto : SecurityIncidentDto
{
    public List<SecurityIncidentAttachmentDto> Attachments { get; set; } = new();
    public List<SecurityIncidentResponseDto> Responses { get; set; } = new();
    public List<SecurityIncidentInvolvedPersonDto> InvolvedPersons { get; set; } = new();
    public List<SecurityControlDto> ImplementedControls { get; set; } = new();
    public List<ThreatIndicatorDto> ThreatIndicators { get; set; } = new();
    public ThreatAssessmentDto? CurrentThreatAssessment { get; set; }
}

public class SecurityIncidentAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public SecurityAttachmentType AttachmentType { get; set; }
    public string? Description { get; set; }
    public bool IsConfidential { get; set; }
    public string? Hash { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string FileSizeFormatted => FormatFileSize(FileSize);
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public class SecurityIncidentResponseDto
{
    public int Id { get; set; }
    public SecurityResponseType ResponseType { get; set; }
    public string ActionTaken { get; set; } = string.Empty;
    public DateTime ActionDateTime { get; set; }
    public bool WasSuccessful { get; set; }
    public bool FollowUpRequired { get; set; }
    public string? FollowUpDetails { get; set; }
    public DateTime? FollowUpDueDate { get; set; }
    public string ResponderName { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public int? EffortHours { get; set; }
    public string? ToolsUsed { get; set; }
    public string? ResourcesUsed { get; set; }
}

public class SecurityIncidentInvolvedPersonDto
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsWitness { get; set; }
    public bool IsVictim { get; set; }
    public bool IsSuspect { get; set; }
    public string? ContactInfo { get; set; }
    public DateTime? InterviewDate { get; set; }
    public string? Notes { get; set; }
}

public class SecurityControlDto
{
    public int Id { get; set; }
    public string ControlName { get; set; } = string.Empty;
    public string ControlDescription { get; set; } = string.Empty;
    public SecurityControlType ControlType { get; set; }
    public SecurityControlCategory Category { get; set; }
    public ControlImplementationStatus Status { get; set; }
    public DateTime ImplementationDate { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string ImplementedByName { get; set; } = string.Empty;
    public bool IsOverdue => ReviewDate.HasValue && ReviewDate.Value < DateTime.UtcNow;
    public int DaysUntilReview => ReviewDate?.Subtract(DateTime.UtcNow).Days ?? 0;
}

public class ThreatIndicatorDto
{
    public int Id { get; set; }
    public string IndicatorType { get; set; } = string.Empty;
    public string IndicatorValue { get; set; } = string.Empty;
    public string ThreatType { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    public string ConfidenceLevel => GetConfidenceLevel(Confidence);
    
    private static string GetConfidenceLevel(int confidence)
    {
        return confidence switch
        {
            >= 80 => "High",
            >= 60 => "Medium",
            >= 40 => "Low",
            _ => "Very Low"
        };
    }
}

public class ThreatAssessmentDto
{
    public int Id { get; set; }
    public ThreatLevel CurrentThreatLevel { get; set; }
    public ThreatLevel PreviousThreatLevel { get; set; }
    public string AssessmentRationale { get; set; } = string.Empty;
    public DateTime AssessmentDateTime { get; set; }
    public string AssessedByName { get; set; } = string.Empty;
    public bool ExternalThreatIntelUsed { get; set; }
    public string? ThreatIntelSource { get; set; }
    public string? ThreatIntelDetails { get; set; }
    public int ThreatCapability { get; set; }
    public int ThreatIntent { get; set; }
    public int TargetVulnerability { get; set; }
    public int ImpactPotential { get; set; }
    public int RiskScore => (ThreatCapability * ThreatIntent) + (TargetVulnerability * ImpactPotential);
    public string RiskLevel => GetRiskLevel(RiskScore);
    
    private static string GetRiskLevel(int score)
    {
        return score switch
        {
            >= 18 => "Severe",
            >= 14 => "High", 
            >= 10 => "Medium",
            >= 6 => "Low",
            _ => "Minimal"
        };
    }
}