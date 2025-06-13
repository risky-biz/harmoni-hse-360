using Harmoni360.Domain.Common;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Security;

/// <summary>
/// Represents a security incident in the HSSE system
/// </summary>
public class SecurityIncident : BaseEntity, IAuditableEntity
{
    // Core Properties
    public string IncidentNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public SecurityIncidentType IncidentType { get; private set; }
    public SecurityIncidentCategory Category { get; private set; }
    public SecuritySeverity Severity { get; private set; }
    public SecurityIncidentStatus Status { get; private set; }
    public ThreatLevel ThreatLevel { get; private set; }
    
    // Location and Time
    public DateTime IncidentDateTime { get; private set; }
    public DateTime? DetectionDateTime { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public GeoLocation? GeoLocation { get; private set; }
    
    // Threat Actor Information
    public ThreatActorType? ThreatActorType { get; private set; }
    public string? ThreatActorDescription { get; private set; }
    public bool IsInternalThreat { get; private set; }
    
    // Impact Assessment
    public SecurityImpact Impact { get; private set; }
    public int? AffectedPersonsCount { get; private set; }
    public decimal? EstimatedLoss { get; private set; }
    public bool DataBreachOccurred { get; private set; }
    
    // Response Information
    public DateTime? ContainmentDateTime { get; private set; }
    public DateTime? ResolutionDateTime { get; private set; }
    public string? ContainmentActions { get; private set; }
    public string? RootCause { get; private set; }
    
    // Navigation Properties
    public int ReporterId { get; private set; }
    public User Reporter { get; private set; } = null!;
    
    public int? AssignedToId { get; private set; }
    public User? AssignedTo { get; private set; }
    
    public int? InvestigatorId { get; private set; }
    public User? Investigator { get; private set; }
    
    // Collections
    private readonly List<SecurityIncidentAttachment> _attachments = new();
    public IReadOnlyCollection<SecurityIncidentAttachment> Attachments => _attachments.AsReadOnly();
    
    private readonly List<SecurityIncidentInvolvedPerson> _involvedPersons = new();
    public IReadOnlyCollection<SecurityIncidentInvolvedPerson> InvolvedPersons => _involvedPersons.AsReadOnly();
    
    private readonly List<SecurityIncidentResponse> _responses = new();
    public IReadOnlyCollection<SecurityIncidentResponse> Responses => _responses.AsReadOnly();
    
    private readonly List<SecurityControl> _implementedControls = new();
    public IReadOnlyCollection<SecurityControl> ImplementedControls => _implementedControls.AsReadOnly();
    
    private readonly List<ThreatIndicator> _threatIndicators = new();
    public IReadOnlyCollection<ThreatIndicator> ThreatIndicators => _threatIndicators.AsReadOnly();
    
    // Audit Properties
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }
    
    // Constructor for EF Core
    protected SecurityIncident() { }
    
    // Factory method
    public static SecurityIncident Create(
        SecurityIncidentType incidentType,
        SecurityIncidentCategory category,
        string title,
        string description,
        SecuritySeverity severity,
        DateTime incidentDateTime,
        string location,
        int reporterId,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));
        
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location is required", nameof(location));
        
        var incident = new SecurityIncident
        {
            IncidentNumber = GenerateIncidentNumber(),
            IncidentType = incidentType,
            Category = category,
            Title = title,
            Description = description,
            Severity = severity,
            Status = SecurityIncidentStatus.Open,
            ThreatLevel = ThreatLevel.Low, // Default, to be assessed
            IncidentDateTime = incidentDateTime,
            DetectionDateTime = DateTime.UtcNow,
            Location = location,
            Impact = SecurityImpact.None, // To be assessed
            IsInternalThreat = false,
            DataBreachOccurred = false,
            ReporterId = reporterId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        
        // Raise domain event
        incident.AddDomainEvent(new SecurityIncidentCreatedEvent(incident));
        
        // Auto-escalate critical incidents
        if (severity == SecuritySeverity.Critical)
        {
            incident.AddDomainEvent(new CriticalSecurityIncidentEvent(incident));
        }
        
        return incident;
    }
    
    // Business Methods
    public void AssignTo(int userId, string modifiedBy)
    {
        AssignedToId = userId;
        Status = SecurityIncidentStatus.Assigned;
        UpdateAudit(modifiedBy);
        
        AddDomainEvent(new SecurityIncidentAssignedEvent(this, userId));
    }
    
    public void AssignInvestigator(int investigatorId, string modifiedBy)
    {
        InvestigatorId = investigatorId;
        Status = SecurityIncidentStatus.Investigating;
        UpdateAudit(modifiedBy);
        
        AddDomainEvent(new InvestigatorAssignedToSecurityIncidentEvent(this, investigatorId));
    }
    
    public void UpdateThreatAssessment(
        ThreatLevel threatLevel, 
        ThreatActorType? actorType,
        string? actorDescription,
        bool isInternal,
        string modifiedBy)
    {
        var previousLevel = ThreatLevel;
        ThreatLevel = threatLevel;
        ThreatActorType = actorType;
        ThreatActorDescription = actorDescription;
        IsInternalThreat = isInternal;
        UpdateAudit(modifiedBy);
        
        if (threatLevel > previousLevel)
        {
            AddDomainEvent(new SecurityThreatEscalatedEvent(this, previousLevel, threatLevel));
        }
    }
    
    public void UpdateImpactAssessment(
        SecurityImpact impact,
        int? affectedPersons,
        decimal? estimatedLoss,
        bool dataBreachOccurred,
        string modifiedBy)
    {
        Impact = impact;
        AffectedPersonsCount = affectedPersons;
        EstimatedLoss = estimatedLoss;
        DataBreachOccurred = dataBreachOccurred;
        UpdateAudit(modifiedBy);
        
        if (dataBreachOccurred)
        {
            AddDomainEvent(new DataBreachDetectedEvent(this));
        }
    }
    
    public void RecordContainment(string actions, DateTime containmentTime, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(actions))
            throw new ArgumentException("Containment actions are required", nameof(actions));
        
        ContainmentActions = actions;
        ContainmentDateTime = containmentTime;
        Status = SecurityIncidentStatus.Contained;
        UpdateAudit(modifiedBy);
        
        AddDomainEvent(new SecurityIncidentContainedEvent(this));
    }
    
    public void StartEradication(string modifiedBy)
    {
        if (Status != SecurityIncidentStatus.Contained)
            throw new InvalidOperationException("Incident must be contained before eradication");
        
        Status = SecurityIncidentStatus.Eradicating;
        UpdateAudit(modifiedBy);
    }
    
    public void StartRecovery(string modifiedBy)
    {
        if (Status != SecurityIncidentStatus.Eradicating)
            throw new InvalidOperationException("Incident must be in eradication phase before recovery");
        
        Status = SecurityIncidentStatus.Recovering;
        UpdateAudit(modifiedBy);
    }
    
    public void ResolveIncident(string rootCause, DateTime resolutionTime, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(rootCause))
            throw new ArgumentException("Root cause is required", nameof(rootCause));
        
        RootCause = rootCause;
        ResolutionDateTime = resolutionTime;
        Status = SecurityIncidentStatus.Resolved;
        UpdateAudit(modifiedBy);
        
        AddDomainEvent(new SecurityIncidentResolvedEvent(this));
    }
    
    public void CloseIncident(string modifiedBy)
    {
        if (Status != SecurityIncidentStatus.Resolved)
            throw new InvalidOperationException("Incident must be resolved before closing");
        
        if (!_responses.Any(r => r.ResponseType == SecurityResponseType.LessonsLearned))
            throw new InvalidOperationException("Lessons learned must be documented before closing");
        
        Status = SecurityIncidentStatus.Closed;
        UpdateAudit(modifiedBy);
        
        AddDomainEvent(new SecurityIncidentClosedEvent(this));
    }
    
    public void SetGeoLocation(double latitude, double longitude)
    {
        GeoLocation = GeoLocation.Create(latitude, longitude);
    }
    
    // Collection Management Methods
    public void AddAttachment(string fileName, string filePath, long fileSize, SecurityAttachmentType attachmentType, string uploadedBy)
    {
        var attachment = SecurityIncidentAttachment.Create(Id, fileName, filePath, fileSize, attachmentType, uploadedBy);
        _attachments.Add(attachment);
    }
    
    public void AddInvolvedPerson(int personId, string involvement, bool isWitness = false)
    {
        var involvedPerson = SecurityIncidentInvolvedPerson.Create(Id, personId, involvement, isWitness);
        _involvedPersons.Add(involvedPerson);
    }
    
    public void AddResponse(SecurityResponseType responseType, string actionTaken, DateTime actionDateTime, int responderId)
    {
        var response = SecurityIncidentResponse.Create(Id, responseType, actionTaken, actionDateTime, responderId);
        _responses.Add(response);
    }
    
    public void AddSecurityControl(SecurityControl control)
    {
        if (control == null)
            throw new ArgumentNullException(nameof(control));
        
        _implementedControls.Add(control);
    }
    
    public void LinkThreatIndicator(ThreatIndicator indicator)
    {
        if (indicator == null)
            throw new ArgumentNullException(nameof(indicator));
        
        _threatIndicators.Add(indicator);
    }
    
    // Private Methods
    private static string GenerateIncidentNumber()
    {
        var year = DateTime.UtcNow.Year;
        var timestamp = DateTime.UtcNow.ToString("MMddHHmmss");
        return $"SEC-{year}-{timestamp}";
    }
    
    private void UpdateAudit(string modifiedBy)
    {
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}