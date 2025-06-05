using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public enum HealthIncidentType
{
    Injury = 1,
    Illness = 2,
    AllergicReaction = 3,
    MedicationIssue = 4,
    MentalHealthEpisode = 5,
    ChronicConditionFlareUp = 6,
    EmergencyResponse = 7,
    Other = 8
}

public enum HealthIncidentSeverity
{
    Minor = 1,
    Moderate = 2,
    Serious = 3,
    Critical = 4,
    LifeThreatening = 5
}

public enum TreatmentLocation
{
    SchoolNurse = 1,
    Classroom = 2,
    Hospital = 3,
    Clinic = 4,
    Homecare = 5,
    Other = 6
}

public class HealthIncident : BaseEntity, IAuditableEntity
{
    public int? IncidentId { get; private set; } // Links to main Incident system
    public int HealthRecordId { get; private set; }
    public HealthIncidentType Type { get; private set; }
    public HealthIncidentSeverity Severity { get; private set; }
    public string Symptoms { get; private set; } = string.Empty;
    public string TreatmentProvided { get; private set; } = string.Empty;
    public TreatmentLocation TreatmentLocation { get; private set; }
    public bool RequiredHospitalization { get; private set; }
    public bool ParentsNotified { get; private set; }
    public DateTime? ParentNotificationTime { get; private set; }
    public DateTime? ReturnToSchoolDate { get; private set; }
    public string? FollowUpRequired { get; private set; }
    public string? TreatedBy { get; private set; }
    public DateTime IncidentDateTime { get; private set; }
    public bool IsResolved { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // Navigation properties
    public HealthRecord HealthRecord { get; private set; } = null!;
    public Incident? Incident { get; private set; }

    protected HealthIncident() { } // For EF Core

    public static HealthIncident Create(
        int healthRecordId,
        HealthIncidentType type,
        HealthIncidentSeverity severity,
        string symptoms,
        string treatmentProvided,
        TreatmentLocation treatmentLocation,
        DateTime? incidentDateTime = null,
        bool requiredHospitalization = false,
        string? treatedBy = null,
        int? incidentId = null)
    {
        if (string.IsNullOrWhiteSpace(symptoms))
            throw new ArgumentException("Symptoms description cannot be empty", nameof(symptoms));

        if (string.IsNullOrWhiteSpace(treatmentProvided))
            throw new ArgumentException("Treatment description cannot be empty", nameof(treatmentProvided));

        var healthIncident = new HealthIncident
        {
            HealthRecordId = healthRecordId,
            IncidentId = incidentId,
            Type = type,
            Severity = severity,
            Symptoms = symptoms.Trim(),
            TreatmentProvided = treatmentProvided.Trim(),
            TreatmentLocation = treatmentLocation,
            RequiredHospitalization = requiredHospitalization,
            TreatedBy = treatedBy?.Trim(),
            IncidentDateTime = incidentDateTime ?? DateTime.UtcNow,
            ParentsNotified = false,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System" // Will be set by infrastructure
        };

        return healthIncident;
    }

    public void UpdateTreatment(
        string treatmentProvided,
        TreatmentLocation treatmentLocation,
        string? treatedBy = null,
        bool requiredHospitalization = false)
    {
        if (string.IsNullOrWhiteSpace(treatmentProvided))
            throw new ArgumentException("Treatment description cannot be empty", nameof(treatmentProvided));

        TreatmentProvided = treatmentProvided.Trim();
        TreatmentLocation = treatmentLocation;
        TreatedBy = treatedBy?.Trim();
        RequiredHospitalization = requiredHospitalization;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void NotifyParents()
    {
        ParentsNotified = true;
        ParentNotificationTime = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetReturnToSchoolDate(DateTime returnDate)
    {
        ReturnToSchoolDate = returnDate;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetFollowUpRequired(string followUpRequired)
    {
        if (string.IsNullOrWhiteSpace(followUpRequired))
            throw new ArgumentException("Follow-up requirements cannot be empty", nameof(followUpRequired));

        FollowUpRequired = followUpRequired.Trim();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Resolve(string? resolutionNotes = null)
    {
        IsResolved = true;
        ResolutionNotes = resolutionNotes?.Trim();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        IsResolved = false;
        ResolutionNotes = null;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void LinkToIncident(int incidentId)
    {
        IncidentId = incidentId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsCritical()
    {
        return Severity >= HealthIncidentSeverity.Critical ||
               RequiredHospitalization ||
               Type == HealthIncidentType.AllergicReaction ||
               Type == HealthIncidentType.EmergencyResponse;
    }

    public bool RequiresParentNotification()
    {
        return Severity >= HealthIncidentSeverity.Moderate ||
               RequiredHospitalization ||
               Type == HealthIncidentType.AllergicReaction;
    }

    public bool IsOverdue()
    {
        if (IsResolved) return false;
        
        var daysOld = (DateTime.UtcNow - CreatedAt).TotalDays;
        return daysOld > 7; // Health incidents should be resolved within a week
    }

    public string GetSeverityDisplayText()
    {
        return Severity switch
        {
            HealthIncidentSeverity.Minor => "Minor",
            HealthIncidentSeverity.Moderate => "Moderate",
            HealthIncidentSeverity.Serious => "Serious",
            HealthIncidentSeverity.Critical => "Critical",
            HealthIncidentSeverity.LifeThreatening => "Life Threatening",
            _ => "Unknown"
        };
    }
}