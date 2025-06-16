using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class CompanyConfiguration : BaseEntity, IAuditableEntity
{
    public string CompanyName { get; private set; } = string.Empty;
    public string CompanyCode { get; private set; } = string.Empty;
    public string? CompanyDescription { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? FaviconUrl { get; private set; }
    
    // Contact Information
    public string? PrimaryEmail { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public string? EmergencyContactNumber { get; private set; }
    
    // Address Information
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    
    // Geographic Coordinates (for maps default center)
    public double? DefaultLatitude { get; private set; }
    public double? DefaultLongitude { get; private set; }
    
    // Branding & Themes
    public string? PrimaryColor { get; private set; }
    public string? SecondaryColor { get; private set; }
    public string? AccentColor { get; private set; }
    
    // Compliance & Industry
    public string? IndustryType { get; private set; }
    public string? ComplianceStandards { get; private set; }
    public string? RegulatoryAuthority { get; private set; }
    
    // System Settings
    public string? TimeZone { get; private set; }
    public string? DateFormat { get; private set; }
    public string? Currency { get; private set; }
    public string? Language { get; private set; }
    
    // Additional Metadata
    public bool IsActive { get; private set; } = true;
    public int Version { get; private set; } = 1;
    
    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private CompanyConfiguration() { }

    public static CompanyConfiguration Create(
        string companyName,
        string companyCode,
        string? description = null,
        string? websiteUrl = null,
        string? primaryEmail = null)
    {
        return new CompanyConfiguration
        {
            CompanyName = companyName,
            CompanyCode = companyCode,
            CompanyDescription = description,
            WebsiteUrl = websiteUrl,
            PrimaryEmail = primaryEmail,
            IsActive = true,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateBasicInfo(
        string companyName,
        string companyCode,
        string? description = null,
        string? websiteUrl = null)
    {
        CompanyName = companyName;
        CompanyCode = companyCode;
        CompanyDescription = description;
        WebsiteUrl = websiteUrl;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateContactInfo(
        string? primaryEmail = null,
        string? primaryPhone = null,
        string? emergencyContactNumber = null)
    {
        PrimaryEmail = primaryEmail;
        PrimaryPhone = primaryPhone;
        EmergencyContactNumber = emergencyContactNumber;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(
        string? address = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null)
    {
        Address = address;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateGeographicInfo(
        double? defaultLatitude = null,
        double? defaultLongitude = null)
    {
        DefaultLatitude = defaultLatitude;
        DefaultLongitude = defaultLongitude;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateBranding(
        string? logoUrl = null,
        string? faviconUrl = null,
        string? primaryColor = null,
        string? secondaryColor = null,
        string? accentColor = null)
    {
        LogoUrl = logoUrl;
        FaviconUrl = faviconUrl;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        AccentColor = accentColor;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateComplianceInfo(
        string? industryType = null,
        string? complianceStandards = null,
        string? regulatoryAuthority = null)
    {
        IndustryType = industryType;
        ComplianceStandards = complianceStandards;
        RegulatoryAuthority = regulatoryAuthority;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSystemSettings(
        string? timeZone = null,
        string? dateFormat = null,
        string? currency = null,
        string? language = null)
    {
        TimeZone = timeZone;
        DateFormat = dateFormat;
        Currency = currency;
        Language = language;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void IncrementVersion()
    {
        Version++;
        LastModifiedAt = DateTime.UtcNow;
    }

    // Helper properties
    public string FullAddress => 
        string.Join(", ", new[] { Address, City, State, PostalCode, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    public bool HasGeographicCoordinates => 
        DefaultLatitude.HasValue && DefaultLongitude.HasValue;

    public bool HasBrandingColors => 
        !string.IsNullOrWhiteSpace(PrimaryColor) || 
        !string.IsNullOrWhiteSpace(SecondaryColor) || 
        !string.IsNullOrWhiteSpace(AccentColor);
}