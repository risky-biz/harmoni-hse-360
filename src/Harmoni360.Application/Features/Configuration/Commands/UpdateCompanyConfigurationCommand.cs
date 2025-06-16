using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Configuration.Commands;

public record UpdateCompanyConfigurationCommand : IRequest<CompanyConfigurationDto>
{
    // Basic Information
    public string CompanyName { get; init; } = string.Empty;
    public string CompanyCode { get; init; } = string.Empty;
    public string? CompanyDescription { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }
    
    // Contact Information
    public string? PrimaryEmail { get; init; }
    public string? PrimaryPhone { get; init; }
    public string? EmergencyContactNumber { get; init; }
    
    // Address Information
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    
    // Geographic Coordinates
    public double? DefaultLatitude { get; init; }
    public double? DefaultLongitude { get; init; }
    
    // Branding & Themes
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? AccentColor { get; init; }
    
    // Compliance & Industry
    public string? IndustryType { get; init; }
    public string? ComplianceStandards { get; init; }
    public string? RegulatoryAuthority { get; init; }
    
    // System Settings
    public string? TimeZone { get; init; }
    public string? DateFormat { get; init; }
    public string? Currency { get; init; }
    public string? Language { get; init; }
}

public record CreateCompanyConfigurationCommand : IRequest<CompanyConfigurationDto>
{
    public string CompanyName { get; init; } = string.Empty;
    public string CompanyCode { get; init; } = string.Empty;
    public string? CompanyDescription { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? PrimaryEmail { get; init; }
}