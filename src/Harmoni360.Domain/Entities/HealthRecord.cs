using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public enum PersonType
{
    Student = 1,
    Staff = 2,
    Visitor = 3,
    Contractor = 4
}

public enum BloodType
{
    APositive = 1,
    ANegative = 2,
    BPositive = 3,
    BNegative = 4,
    ABPositive = 5,
    ABNegative = 6,
    OPositive = 7,
    ONegative = 8
}

public class HealthRecord : BaseEntity, IAuditableEntity
{
    public int PersonId { get; private set; }
    public PersonType PersonType { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public BloodType? BloodType { get; private set; }
    public string? MedicalNotes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // Navigation properties
    public Person Person { get; private set; } = null!;
    
    private readonly List<MedicalCondition> _medicalConditions = new();
    public IReadOnlyCollection<MedicalCondition> MedicalConditions => _medicalConditions.AsReadOnly();
    
    private readonly List<VaccinationRecord> _vaccinations = new();
    public IReadOnlyCollection<VaccinationRecord> Vaccinations => _vaccinations.AsReadOnly();
    
    private readonly List<HealthIncident> _healthIncidents = new();
    public IReadOnlyCollection<HealthIncident> HealthIncidents => _healthIncidents.AsReadOnly();
    
    private readonly List<EmergencyContact> _emergencyContacts = new();
    public IReadOnlyCollection<EmergencyContact> EmergencyContacts => _emergencyContacts.AsReadOnly();

    protected HealthRecord() { } // For EF Core

    public static HealthRecord Create(
        int personId,
        PersonType personType,
        DateTime? dateOfBirth = null,
        BloodType? bloodType = null,
        string? medicalNotes = null)
    {
        var healthRecord = new HealthRecord
        {
            PersonId = personId,
            PersonType = personType,
            DateOfBirth = dateOfBirth,
            BloodType = bloodType,
            MedicalNotes = medicalNotes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System" // Will be set by infrastructure
        };

        return healthRecord;
    }

    public void UpdateBasicInfo(
        DateTime? dateOfBirth,
        BloodType? bloodType,
        string? medicalNotes)
    {
        DateOfBirth = dateOfBirth;
        BloodType = bloodType;
        MedicalNotes = medicalNotes;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddMedicalCondition(MedicalCondition condition)
    {
        _medicalConditions.Add(condition);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveMedicalCondition(MedicalCondition condition)
    {
        _medicalConditions.Remove(condition);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddVaccination(VaccinationRecord vaccination)
    {
        _vaccinations.Add(vaccination);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddHealthIncident(HealthIncident incident)
    {
        _healthIncidents.Add(incident);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddEmergencyContact(EmergencyContact contact)
    {
        _emergencyContacts.Add(contact);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveEmergencyContact(EmergencyContact contact)
    {
        _emergencyContacts.Remove(contact);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool HasCriticalMedicalConditions()
    {
        return _medicalConditions.Any(mc => mc.RequiresEmergencyAction);
    }

    public IEnumerable<VaccinationRecord> GetExpiringVaccinations(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        return _vaccinations.Where(v => v.ExpiryDate.HasValue && v.ExpiryDate <= cutoffDate);
    }
}