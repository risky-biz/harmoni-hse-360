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

    // Navigation properties
    public int ReporterId { get; private set; }
    public User Reporter { get; private set; } = null!;

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
        int reporterId,
        string createdBy)
    {
        var incident = new Incident
        {
            Title = title,
            Description = description,
            Severity = severity,
            Status = IncidentStatus.Reported,
            IncidentDate = incidentDate,
            Location = location,
            ReporterId = reporterId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
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
        GeoLocation = new GeoLocation(latitude, longitude);
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

    public void AddCorrectiveAction(string description, int assignedToId, DateTime dueDate, string createdBy)
    {
        var action = CorrectiveAction.Create(Id, description, assignedToId, dueDate, createdBy);
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
    Serious = 3,
    Critical = 4
}

public enum IncidentStatus
{
    Reported = 1,
    UnderInvestigation = 2,
    AwaitingAction = 3,
    Resolved = 4,
    Closed = 5
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