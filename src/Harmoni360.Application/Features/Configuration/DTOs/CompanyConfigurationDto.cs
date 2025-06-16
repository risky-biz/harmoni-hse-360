namespace Harmoni360.Application.Features.Configuration.DTOs;

public class CompanyConfigurationDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    
    // Contact Information
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? EmergencyContactNumber { get; set; }
    
    // Address Information
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? FullAddress { get; set; }
    
    // Geographic Coordinates
    public double? DefaultLatitude { get; set; }
    public double? DefaultLongitude { get; set; }
    public bool HasGeographicCoordinates { get; set; }
    
    // Branding & Themes
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public bool HasBrandingColors { get; set; }
    
    // Compliance & Industry
    public string? IndustryType { get; set; }
    public string? ComplianceStandards { get; set; }
    public string? RegulatoryAuthority { get; set; }
    
    // System Settings
    public string? TimeZone { get; set; }
    public string? DateFormat { get; set; }
    public string? Currency { get; set; }
    public string? Language { get; set; }
    
    // System Information
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class CompanyConfigurationSummaryDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

public class UpdateCompanyConfigurationDto
{
    // Basic Information
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    
    // Contact Information
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? EmergencyContactNumber { get; set; }
    
    // Address Information
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    // Geographic Coordinates
    public double? DefaultLatitude { get; set; }
    public double? DefaultLongitude { get; set; }
    
    // Branding & Themes
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    
    // Compliance & Industry
    public string? IndustryType { get; set; }
    public string? ComplianceStandards { get; set; }
    public string? RegulatoryAuthority { get; set; }
    
    // System Settings
    public string? TimeZone { get; set; }
    public string? DateFormat { get; set; }
    public string? Currency { get; set; }
    public string? Language { get; set; }
}