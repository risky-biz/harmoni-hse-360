using HarmoniHSE360.Domain.Common;
using HarmoniHSE360.Domain.Events;
using HarmoniHSE360.Domain.ValueObjects;

namespace HarmoniHSE360.Domain.Entities;

public class Incident : BaseEntity, IAuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IncidentSeverity Severity { get; private set; }
    public IncidentStatus Status { get; private set; }
    public DateTime IncidentDate { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public GeoLocation? GeoLocation { get; private set; }

    // Reporter details (stored directly for quick access)
    public string ReporterName { get; private set; } = string.Empty;
    public string ReporterEmail { get; private set; } = string.Empty;
    public string ReporterDepartment { get; private set; } = string.Empty;
    
    // Additional fields for escalation
    public string Department => ReporterDepartment;
    public DateTime? LastResponseAt { get; private set; }
    
    // Injury and incident details
    public InjuryType? InjuryType { get; private set; }
    public bool MedicalTreatmentProvided { get; private set; }
    public bool EmergencyServicesContacted { get; private set; }
    public string? WitnessNames { get; private set; }
    public string? ImmediateActionsTaken { get; private set; }

    // Navigation properties
    public int? ReporterId { get; private set; }
    public User? Reporter { get; private set; }

    public int? InvestigatorId { get; private set; }
    public User? Investigator { get; private set; }

    private readonly List<IncidentAttachment> _attachments = new();
    public IReadOnlyCollection<IncidentAttachment> Attachments => _attachments.AsReadOnly();

    private readonly List<IncidentInvolvedPerson> _involvedPersons = new();
    public IReadOnlyCollection<IncidentInvolvedPerson> InvolvedPersons => _involvedPersons.AsReadOnly();

    private readonly List<CorrectiveAction> _correctiveActions = new();
    public IReadOnlyCollection<CorrectiveAction> CorrectiveActions => _correctiveActions.AsReadOnly();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected Incident() { } // For EF Core

    public static Incident Create(
        string title,
        string description,
        IncidentSeverity severity,
        DateTime incidentDate,
        string location,
        string reporterName,
        string reporterEmail,
        string reporterDepartment,
        GeoLocation? geoLocation = null,
        int? reporterId = null)
    {
        var incident = new Incident
        {
            Title = title,
            Description = description,
            Severity = severity,
            Status = IncidentStatus.Reported,
            IncidentDate = incidentDate,
            Location = location,
            ReporterName = reporterName,
            ReporterEmail = reporterEmail,
            ReporterDepartment = reporterDepartment,
            GeoLocation = geoLocation,
            ReporterId = reporterId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = reporterEmail,
            MedicalTreatmentProvided = false,
            EmergencyServicesContacted = false
        };

        incident.AddDomainEvent(new IncidentCreatedEvent(incident));

        if (severity >= IncidentSeverity.Serious)
        {
            incident.AddDomainEvent(new SeriousIncidentReportedEvent(incident));
        }

        return incident;
    }

    public void UpdateDetails(string title, string description, string modifiedBy)
    {
        Title = title;
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new IncidentUpdatedEvent(this));
    }

    public void AssignInvestigator(int investigatorId, string modifiedBy)
    {
        InvestigatorId = investigatorId;
        Status = IncidentStatus.UnderInvestigation;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new InvestigatorAssignedEvent(this, investigatorId));
    }

    public void SetGeoLocation(double latitude, double longitude)
    {
        GeoLocation = GeoLocation.Create(latitude, longitude);
    }
    
    public void UpdateInjuryDetails(InjuryType? injuryType, bool medicalTreatmentProvided, bool emergencyServicesContacted)
    {
        InjuryType = injuryType;
        MedicalTreatmentProvided = medicalTreatmentProvided;
        EmergencyServicesContacted = emergencyServicesContacted;
    }
    
    public void AddWitnessInformation(string witnessNames)
    {
        WitnessNames = witnessNames;
    }
    
    public void RecordImmediateActions(string immediateActionsTaken)
    {
        ImmediateActionsTaken = immediateActionsTaken;
    }
    
    public void UpdateStatus(IncidentStatus status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSeverity(IncidentSeverity severity)
    {
        Severity = severity;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateLocation(string location)
    {
        Location = location;
        LastModifiedAt = DateTime.UtcNow;
    }
    
    public void RecordResponse()
    {
        LastResponseAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddAttachment(string fileName, string filePath, long fileSize, string uploadedBy)
    {
        var attachment = new IncidentAttachment(Id, fileName, filePath, fileSize, uploadedBy);
        _attachments.Add(attachment);
    }

    public void AddInvolvedPerson(int personId, InvolvementType involvementType, string? injuryDescription = null)
    {
        var involvedPerson = new IncidentInvolvedPerson(Id, personId, involvementType, injuryDescription);
        _involvedPersons.Add(involvedPerson);
    }

    public void AddCorrectiveAction(CorrectiveAction action)
    {
        _correctiveActions.Add(action);
        AddDomainEvent(new CorrectiveActionAddedEvent(this, action));
    }

    public void CloseIncident(string closureNotes, string closedBy)
    {
        if (_correctiveActions.Any(ca => ca.Status != ActionStatus.Completed))
        {
            throw new InvalidOperationException("Cannot close incident with pending corrective actions");
        }

        Status = IncidentStatus.Closed;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = closedBy;

        AddDomainEvent(new IncidentClosedEvent(this, closureNotes));
    }
}

public enum IncidentSeverity
{
    Minor = 1,
    Moderate = 2,
    Major = 3,
    Serious = 4,
    Critical = 5,
    Emergency = 6
}

public enum IncidentStatus
{
    Open = 1,
    Reported = 2,
    InProgress = 3,
    UnderInvestigation = 4,
    AwaitingAction = 5,
    Resolved = 6,
    Closed = 7
}

public enum InvolvementType
{
    Witness = 1,
    Victim = 2,
    FirstResponder = 3
}

public enum ActionStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Overdue = 4
}

public enum InjuryType
{
    None = 1,
    Cut = 2,
    Bruise = 3,
    Burn = 4,
    Fracture = 5,
    Sprain = 6,
    Other = 7
}