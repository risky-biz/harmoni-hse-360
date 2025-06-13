using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public enum ContactRelationship
{
    Parent = 1,
    Guardian = 2,
    Spouse = 3,
    Sibling = 4,
    GrandParent = 5,
    Relative = 6,
    Friend = 7,
    Colleague = 8,
    Other = 9
}

public class EmergencyContact : BaseEntity, IAuditableEntity
{
    public int HealthRecordId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ContactRelationship Relationship { get; private set; }
    public string? CustomRelationship { get; private set; }
    public string PrimaryPhone { get; private set; } = string.Empty;
    public string? SecondaryPhone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public bool IsPrimaryContact { get; private set; }
    public bool AuthorizedForPickup { get; private set; }
    public bool AuthorizedForMedicalDecisions { get; private set; }
    public int ContactOrder { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // Navigation properties
    public HealthRecord HealthRecord { get; private set; } = null!;

    protected EmergencyContact() { } // For EF Core

    public static EmergencyContact Create(
        int healthRecordId,
        string name,
        ContactRelationship relationship,
        string primaryPhone,
        string? email = null,
        string? secondaryPhone = null,
        string? address = null,
        bool isPrimaryContact = false,
        bool authorizedForPickup = false,
        bool authorizedForMedicalDecisions = false,
        string? customRelationship = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Contact name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(primaryPhone))
            throw new ArgumentException("Primary phone cannot be empty", nameof(primaryPhone));

        if (relationship == ContactRelationship.Other && string.IsNullOrWhiteSpace(customRelationship))
            throw new ArgumentException("Custom relationship must be specified when relationship is Other", nameof(customRelationship));

        if (!IsValidPhoneNumber(primaryPhone))
            throw new ArgumentException("Primary phone number format is invalid", nameof(primaryPhone));

        if (!string.IsNullOrWhiteSpace(secondaryPhone) && !IsValidPhoneNumber(secondaryPhone))
            throw new ArgumentException("Secondary phone number format is invalid", nameof(secondaryPhone));

        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            throw new ArgumentException("Email format is invalid", nameof(email));

        var contact = new EmergencyContact
        {
            HealthRecordId = healthRecordId,
            Name = name.Trim(),
            Relationship = relationship,
            CustomRelationship = customRelationship?.Trim(),
            PrimaryPhone = primaryPhone.Trim(),
            SecondaryPhone = secondaryPhone?.Trim(),
            Email = email?.Trim(),
            Address = address?.Trim(),
            IsPrimaryContact = isPrimaryContact,
            AuthorizedForPickup = authorizedForPickup,
            AuthorizedForMedicalDecisions = authorizedForMedicalDecisions,
            ContactOrder = 1, // Will be set properly by business logic
            IsActive = true,
            Notes = notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System" // Will be set by infrastructure
        };

        return contact;
    }

    public void Update(
        string name,
        ContactRelationship relationship,
        string primaryPhone,
        string? email = null,
        string? secondaryPhone = null,
        string? address = null,
        bool isPrimaryContact = false,
        bool authorizedForPickup = false,
        bool authorizedForMedicalDecisions = false,
        string? customRelationship = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Contact name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(primaryPhone))
            throw new ArgumentException("Primary phone cannot be empty", nameof(primaryPhone));

        if (relationship == ContactRelationship.Other && string.IsNullOrWhiteSpace(customRelationship))
            throw new ArgumentException("Custom relationship must be specified when relationship is Other", nameof(customRelationship));

        if (!IsValidPhoneNumber(primaryPhone))
            throw new ArgumentException("Primary phone number format is invalid", nameof(primaryPhone));

        if (!string.IsNullOrWhiteSpace(secondaryPhone) && !IsValidPhoneNumber(secondaryPhone))
            throw new ArgumentException("Secondary phone number format is invalid", nameof(secondaryPhone));

        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            throw new ArgumentException("Email format is invalid", nameof(email));

        Name = name.Trim();
        Relationship = relationship;
        CustomRelationship = customRelationship?.Trim();
        PrimaryPhone = primaryPhone.Trim();
        SecondaryPhone = secondaryPhone?.Trim();
        Email = email?.Trim();
        Address = address?.Trim();
        IsPrimaryContact = isPrimaryContact;
        AuthorizedForPickup = authorizedForPickup;
        AuthorizedForMedicalDecisions = authorizedForMedicalDecisions;
        Notes = notes?.Trim();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetAsPrimary()
    {
        IsPrimaryContact = true;
        ContactOrder = 1;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetAsSecondary()
    {
        IsPrimaryContact = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetContactOrder(int order)
    {
        if (order < 1)
            throw new ArgumentException("Contact order must be greater than 0", nameof(order));

        ContactOrder = order;
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

    public string GetDisplayRelationship()
    {
        return Relationship == ContactRelationship.Other && !string.IsNullOrWhiteSpace(CustomRelationship)
            ? CustomRelationship
            : Relationship.ToString();
    }

    public string GetFullContactInfo()
    {
        var info = $"{Name} ({GetDisplayRelationship()}) - {PrimaryPhone}";
        if (!string.IsNullOrWhiteSpace(Email))
            info += $" - {Email}";
        return info;
    }

    public bool HasValidContactMethod()
    {
        return !string.IsNullOrWhiteSpace(PrimaryPhone) || !string.IsNullOrWhiteSpace(Email);
    }

    private static bool IsValidPhoneNumber(string phone)
    {
        // Basic phone validation - can be enhanced with more sophisticated validation
        var cleanPhone = new string(phone.Where(char.IsDigit).ToArray());
        return cleanPhone.Length >= 10 && cleanPhone.Length <= 15;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}