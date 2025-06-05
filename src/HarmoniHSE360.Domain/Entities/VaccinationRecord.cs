using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public enum VaccinationStatus
{
    Scheduled = 1,
    Administered = 2,
    Overdue = 3,
    Exempted = 4,
    Expired = 5
}

public class VaccinationRecord : BaseEntity, IAuditableEntity
{
    public int HealthRecordId { get; private set; }
    public string VaccineName { get; private set; } = string.Empty;
    public DateTime? DateAdministered { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? AdministeredBy { get; private set; }
    public string? AdministrationLocation { get; private set; }
    public VaccinationStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public bool IsRequired { get; private set; }
    public string? ExemptionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // Navigation properties
    public HealthRecord HealthRecord { get; private set; } = null!;

    protected VaccinationRecord() { } // For EF Core

    public static VaccinationRecord Create(
        int healthRecordId,
        string vaccineName,
        bool isRequired = false,
        DateTime? dateAdministered = null,
        DateTime? expiryDate = null,
        string? batchNumber = null,
        string? administeredBy = null,
        string? administrationLocation = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(vaccineName))
            throw new ArgumentException("Vaccine name cannot be empty", nameof(vaccineName));

        var vaccination = new VaccinationRecord
        {
            HealthRecordId = healthRecordId,
            VaccineName = vaccineName.Trim(),
            DateAdministered = dateAdministered,
            ExpiryDate = expiryDate,
            BatchNumber = batchNumber?.Trim(),
            AdministeredBy = administeredBy?.Trim(),
            AdministrationLocation = administrationLocation?.Trim(),
            Notes = notes?.Trim(),
            IsRequired = isRequired,
            Status = dateAdministered.HasValue ? VaccinationStatus.Administered : VaccinationStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System" // Will be set by infrastructure
        };

        // Update status based on dates
        vaccination.UpdateStatus();

        return vaccination;
    }

    public void RecordAdministration(
        DateTime dateAdministered,
        string administeredBy,
        string? batchNumber = null,
        string? administrationLocation = null,
        DateTime? expiryDate = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(administeredBy))
            throw new ArgumentException("Administrator name cannot be empty", nameof(administeredBy));

        DateAdministered = dateAdministered;
        AdministeredBy = administeredBy.Trim();
        BatchNumber = batchNumber?.Trim();
        AdministrationLocation = administrationLocation?.Trim();
        ExpiryDate = expiryDate;
        Notes = notes?.Trim();
        Status = VaccinationStatus.Administered;
        LastModifiedAt = DateTime.UtcNow;

        UpdateStatus();
    }

    public void SetExemption(string exemptionReason)
    {
        if (string.IsNullOrWhiteSpace(exemptionReason))
            throw new ArgumentException("Exemption reason cannot be empty", nameof(exemptionReason));

        ExemptionReason = exemptionReason.Trim();
        Status = VaccinationStatus.Exempted;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveExemption()
    {
        ExemptionReason = null;
        Status = DateAdministered.HasValue ? VaccinationStatus.Administered : VaccinationStatus.Scheduled;
        LastModifiedAt = DateTime.UtcNow;
        
        UpdateStatus();
    }

    public void UpdateStatus()
    {
        if (Status == VaccinationStatus.Exempted)
            return;

        var now = DateTime.UtcNow;

        if (!DateAdministered.HasValue)
        {
            Status = VaccinationStatus.Scheduled;
            return;
        }

        if (ExpiryDate.HasValue && ExpiryDate.Value < now)
        {
            Status = VaccinationStatus.Expired;
            return;
        }

        Status = VaccinationStatus.Administered;
    }

    public bool IsExpiringSoon(int daysAhead = 30)
    {
        if (!ExpiryDate.HasValue || Status != VaccinationStatus.Administered)
            return false;

        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        return ExpiryDate.Value <= cutoffDate;
    }

    public bool IsExpired()
    {
        return Status == VaccinationStatus.Expired || 
               (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow);
    }

    public bool IsCompliant()
    {
        return Status == VaccinationStatus.Administered || 
               Status == VaccinationStatus.Exempted;
    }

    public int DaysUntilExpiry()
    {
        if (!ExpiryDate.HasValue)
            return int.MaxValue;

        return (int)(ExpiryDate.Value - DateTime.UtcNow).TotalDays;
    }
}