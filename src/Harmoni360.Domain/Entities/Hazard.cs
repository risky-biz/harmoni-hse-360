using Harmoni360.Domain.Common;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Domain.Entities;

public class Hazard : BaseEntity, IAuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int? CategoryId { get; private set; }
    public HazardCategory? Category { get; private set; }
    public int? TypeId { get; private set; }
    public HazardType? Type { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public GeoLocation? GeoLocation { get; private set; }
    public HazardStatus Status { get; private set; }
    public HazardSeverity Severity { get; private set; }
    public DateTime IdentifiedDate { get; private set; }
    public DateTime? ExpectedResolutionDate { get; private set; }
    
    // Reporter information
    public int ReporterId { get; private set; }
    public User Reporter { get; private set; } = null!;
    public string ReporterDepartment { get; private set; } = string.Empty;
    
    // Risk assessment
    public int? CurrentRiskAssessmentId { get; private set; }
    public RiskAssessment? CurrentRiskAssessment { get; private set; }
    
    // Navigation properties
    private readonly List<HazardAttachment> _attachments = new();
    public IReadOnlyCollection<HazardAttachment> Attachments => _attachments.AsReadOnly();
    
    private readonly List<RiskAssessment> _riskAssessments = new();
    public IReadOnlyCollection<RiskAssessment> RiskAssessments => _riskAssessments.AsReadOnly();
    
    private readonly List<HazardMitigationAction> _mitigationActions = new();
    public IReadOnlyCollection<HazardMitigationAction> MitigationActions => _mitigationActions.AsReadOnly();
    
    private readonly List<HazardReassessment> _reassessments = new();
    public IReadOnlyCollection<HazardReassessment> Reassessments => _reassessments.AsReadOnly();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected Hazard() { } // For EF Core

    public static Hazard Create(
        string title,
        string description,
        int? categoryId,
        int? typeId,
        string location,
        HazardSeverity severity,
        int reporterId,
        string reporterDepartment,
        GeoLocation? geoLocation = null)
    {
        var hazard = new Hazard
        {
            Title = title,
            Description = description,
            CategoryId = categoryId,
            TypeId = typeId,
            Location = location,
            Severity = severity,
            Status = HazardStatus.Reported,
            IdentifiedDate = DateTime.UtcNow,
            ReporterId = reporterId,
            ReporterDepartment = reporterDepartment,
            GeoLocation = geoLocation,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = $"User_{reporterId}"
        };

        hazard.AddDomainEvent(new HazardIdentifiedEvent(hazard));

        if (severity >= HazardSeverity.Major)
        {
            hazard.AddDomainEvent(new HighSeverityHazardIdentifiedEvent(hazard));
        }

        return hazard;
    }

    public void UpdateDetails(string title, string description, string modifiedBy)
    {
        Title = title;
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new HazardUpdatedEvent(this));
    }

    public void UpdateStatus(HazardStatus status, string modifiedBy)
    {
        var previousStatus = Status;
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new HazardStatusChangedEvent(this, previousStatus, status));

        if (status == HazardStatus.Resolved)
        {
            AddDomainEvent(new HazardResolvedEvent(this));
        }
    }

    public void UpdateSeverity(HazardSeverity severity, string modifiedBy)
    {
        var previousSeverity = Severity;
        Severity = severity;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new HazardSeverityChangedEvent(this, previousSeverity, severity));
    }

    public void UpdateCategory(int? categoryId, string modifiedBy)
    {
        CategoryId = categoryId;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new HazardUpdatedEvent(this));
    }

    public void UpdateType(int? typeId, string modifiedBy)
    {
        TypeId = typeId;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new HazardUpdatedEvent(this));
    }

    public void SetGeoLocation(double latitude, double longitude)
    {
        GeoLocation = GeoLocation.Create(latitude, longitude);
    }

    public void SetExpectedResolutionDate(DateTime expectedResolutionDate)
    {
        ExpectedResolutionDate = expectedResolutionDate;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddRiskAssessment(RiskAssessment riskAssessment)
    {
        _riskAssessments.Add(riskAssessment);
        
        // Set as current if it's the most recent active assessment
        if (riskAssessment.IsActive && 
            (CurrentRiskAssessment == null || riskAssessment.AssessmentDate > CurrentRiskAssessment.AssessmentDate))
        {
            CurrentRiskAssessmentId = riskAssessment.Id;
            CurrentRiskAssessment = riskAssessment;
        }

        AddDomainEvent(new RiskAssessmentCompletedEvent(this, riskAssessment));
    }

    public void AddMitigationAction(HazardMitigationAction action)
    {
        _mitigationActions.Add(action);
        
        // Update status if first mitigation action is added
        if (Status == HazardStatus.UnderAssessment)
        {
            Status = HazardStatus.Mitigating;
        }

        AddDomainEvent(new MitigationActionAddedEvent(this, action));
    }

    public void AddAttachment(string fileName, string filePath, long fileSize, string uploadedBy)
    {
        var attachment = new HazardAttachment(Id, fileName, filePath, fileSize, uploadedBy);
        _attachments.Add(attachment);
    }

    public void ScheduleReassessment(DateTime nextAssessmentDate, string reason)
    {
        var reassessment = new HazardReassessment(Id, nextAssessmentDate, reason);
        _reassessments.Add(reassessment);

        AddDomainEvent(new HazardReassessmentScheduledEvent(this, nextAssessmentDate, reason));
    }

    public void Close(string closureNotes, string closedBy)
    {
        // Check if all critical mitigation actions are completed
        var pendingCriticalActions = _mitigationActions
            .Where(ma => ma.Status != MitigationActionStatus.Completed && 
                        ma.Type == MitigationActionType.Elimination)
            .ToList();

        if (pendingCriticalActions.Any())
        {
            throw new InvalidOperationException("Cannot close hazard with pending critical mitigation actions");
        }

        Status = HazardStatus.Closed;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = closedBy;

        AddDomainEvent(new HazardClosedEvent(this, closureNotes));
    }
}


public enum HazardStatus
{
    Reported = 1,
    UnderAssessment = 2,
    ActionRequired = 3,
    Mitigating = 4,
    Monitoring = 5,
    Resolved = 6,
    Closed = 7
}

public enum HazardSeverity
{
    Negligible = 1,
    Minor = 2,
    Moderate = 3,
    Major = 4,
    Catastrophic = 5
}