using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Events;

// Health Record Events
public class HealthRecordCreatedEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public int PersonId { get; }
    public DateTime OccurredOn { get; }

    public HealthRecordCreatedEvent(int healthRecordId, int personId)
    {
        HealthRecordId = healthRecordId;
        PersonId = personId;
        OccurredOn = DateTime.UtcNow;
    }
}

public class CriticalMedicalConditionAddedEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public int MedicalConditionId { get; }
    public string ConditionName { get; }
    public string EmergencyInstructions { get; }
    public DateTime OccurredOn { get; }

    public CriticalMedicalConditionAddedEvent(
        int healthRecordId, 
        int medicalConditionId,
        string conditionName,
        string emergencyInstructions)
    {
        HealthRecordId = healthRecordId;
        MedicalConditionId = medicalConditionId;
        ConditionName = conditionName;
        EmergencyInstructions = emergencyInstructions;
        OccurredOn = DateTime.UtcNow;
    }
}

// Vaccination Events
public class VaccinationRequiredEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public int VaccinationId { get; }
    public string VaccineName { get; }
    public DateTime DueDate { get; }
    public DateTime OccurredOn { get; }

    public VaccinationRequiredEvent(
        int healthRecordId, 
        int vaccinationId,
        string vaccineName,
        DateTime dueDate)
    {
        HealthRecordId = healthRecordId;
        VaccinationId = vaccinationId;
        VaccineName = vaccineName;
        DueDate = dueDate;
        OccurredOn = DateTime.UtcNow;
    }
}

public class VaccinationAdministeredEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public int VaccinationId { get; }
    public string VaccineName { get; }
    public DateTime DateAdministered { get; }
    public string AdministeredBy { get; }
    public DateTime OccurredOn { get; }

    public VaccinationAdministeredEvent(
        int healthRecordId,
        int vaccinationId,
        string vaccineName,
        DateTime dateAdministered,
        string administeredBy)
    {
        HealthRecordId = healthRecordId;
        VaccinationId = vaccinationId;
        VaccineName = vaccineName;
        DateAdministered = dateAdministered;
        AdministeredBy = administeredBy;
        OccurredOn = DateTime.UtcNow;
    }
}

public class VaccinationExpiringEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public int VaccinationId { get; }
    public string VaccineName { get; }
    public DateTime ExpiryDate { get; }
    public int DaysUntilExpiry { get; }
    public DateTime OccurredOn { get; }

    public VaccinationExpiringEvent(
        int healthRecordId,
        int vaccinationId,
        string vaccineName,
        DateTime expiryDate,
        int daysUntilExpiry)
    {
        HealthRecordId = healthRecordId;
        VaccinationId = vaccinationId;
        VaccineName = vaccineName;
        ExpiryDate = expiryDate;
        DaysUntilExpiry = daysUntilExpiry;
        OccurredOn = DateTime.UtcNow;
    }
}

public class VaccinationExpiredEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public int VaccinationId { get; }
    public string VaccineName { get; }
    public DateTime ExpiryDate { get; }
    public DateTime OccurredOn { get; }

    public VaccinationExpiredEvent(
        int healthRecordId,
        int vaccinationId,
        string vaccineName,
        DateTime expiryDate)
    {
        HealthRecordId = healthRecordId;
        VaccinationId = vaccinationId;
        VaccineName = vaccineName;
        ExpiryDate = expiryDate;
        OccurredOn = DateTime.UtcNow;
    }
}

// Health Incident Events
public class HealthIncidentCreatedEvent : IDomainEvent
{
    public int HealthIncidentId { get; }
    public int HealthRecordId { get; }
    public string IncidentType { get; }
    public string Severity { get; }
    public bool RequiredHospitalization { get; }
    public DateTime IncidentDateTime { get; }
    public DateTime OccurredOn { get; }

    public HealthIncidentCreatedEvent(
        int healthIncidentId,
        int healthRecordId,
        string incidentType,
        string severity,
        bool requiredHospitalization,
        DateTime incidentDateTime)
    {
        HealthIncidentId = healthIncidentId;
        HealthRecordId = healthRecordId;
        IncidentType = incidentType;
        Severity = severity;
        RequiredHospitalization = requiredHospitalization;
        IncidentDateTime = incidentDateTime;
        OccurredOn = DateTime.UtcNow;
    }
}

public class CriticalHealthIncidentEvent : IDomainEvent
{
    public int HealthIncidentId { get; }
    public int HealthRecordId { get; }
    public string PersonName { get; }
    public string IncidentType { get; }
    public string Severity { get; }
    public string Symptoms { get; }
    public bool RequiredHospitalization { get; }
    public DateTime IncidentDateTime { get; }
    public DateTime OccurredOn { get; }

    public CriticalHealthIncidentEvent(
        int healthIncidentId,
        int healthRecordId,
        string personName,
        string incidentType,
        string severity,
        string symptoms,
        bool requiredHospitalization,
        DateTime incidentDateTime)
    {
        HealthIncidentId = healthIncidentId;
        HealthRecordId = healthRecordId;
        PersonName = personName;
        IncidentType = incidentType;
        Severity = severity;
        Symptoms = symptoms;
        RequiredHospitalization = requiredHospitalization;
        IncidentDateTime = incidentDateTime;
        OccurredOn = DateTime.UtcNow;
    }
}

public class ParentNotificationRequiredEvent : IDomainEvent
{
    public int HealthIncidentId { get; }
    public int HealthRecordId { get; }
    public string StudentName { get; }
    public string IncidentDetails { get; }
    public List<string> EmergencyContactPhones { get; }
    public DateTime OccurredOn { get; }

    public ParentNotificationRequiredEvent(
        int healthIncidentId,
        int healthRecordId,
        string studentName,
        string incidentDetails,
        List<string> emergencyContactPhones)
    {
        HealthIncidentId = healthIncidentId;
        HealthRecordId = healthRecordId;
        StudentName = studentName;
        IncidentDetails = incidentDetails;
        EmergencyContactPhones = emergencyContactPhones;
        OccurredOn = DateTime.UtcNow;
    }
}

// Emergency Contact Events
public class EmergencyContactAddedEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public int EmergencyContactId { get; }
    public string ContactName { get; }
    public string Relationship { get; }
    public bool IsPrimaryContact { get; }
    public DateTime OccurredOn { get; }

    public EmergencyContactAddedEvent(
        int healthRecordId,
        int emergencyContactId,
        string contactName,
        string relationship,
        bool isPrimaryContact)
    {
        HealthRecordId = healthRecordId;
        EmergencyContactId = emergencyContactId;
        ContactName = contactName;
        Relationship = relationship;
        IsPrimaryContact = isPrimaryContact;
        OccurredOn = DateTime.UtcNow;
    }
}

public class PrimaryEmergencyContactChangedEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public int NewPrimaryContactId { get; }
    public int? PreviousPrimaryContactId { get; }
    public string NewPrimaryContactName { get; }
    public DateTime OccurredOn { get; }

    public PrimaryEmergencyContactChangedEvent(
        int healthRecordId,
        int newPrimaryContactId,
        string newPrimaryContactName,
        int? previousPrimaryContactId = null)
    {
        HealthRecordId = healthRecordId;
        NewPrimaryContactId = newPrimaryContactId;
        PreviousPrimaryContactId = previousPrimaryContactId;
        NewPrimaryContactName = newPrimaryContactName;
        OccurredOn = DateTime.UtcNow;
    }
}

// Health Compliance Events
public class HealthComplianceCheckRequiredEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public string PersonName { get; }
    public List<string> MissingRequirements { get; }
    public DateTime OccurredOn { get; }

    public HealthComplianceCheckRequiredEvent(
        int healthRecordId,
        string personName,
        List<string> missingRequirements)
    {
        HealthRecordId = healthRecordId;
        PersonName = personName;
        MissingRequirements = missingRequirements;
        OccurredOn = DateTime.UtcNow;
    }
}

public class HealthClearanceExpiredEvent : IDomainEvent
{
    public int HealthRecordId { get; }
    public string PersonName { get; }
    public string ClearanceType { get; }
    public DateTime ExpiryDate { get; }
    public DateTime OccurredOn { get; }

    public HealthClearanceExpiredEvent(
        int healthRecordId,
        string personName,
        string clearanceType,
        DateTime expiryDate)
    {
        HealthRecordId = healthRecordId;
        PersonName = personName;
        ClearanceType = clearanceType;
        ExpiryDate = expiryDate;
        OccurredOn = DateTime.UtcNow;
    }
}