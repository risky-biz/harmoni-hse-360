using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Configuration.DTOs;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Configuration.Commands;

public class UpdateCompanyConfigurationCommandHandler : 
    IRequestHandler<UpdateCompanyConfigurationCommand, CompanyConfigurationDto>,
    IRequestHandler<CreateCompanyConfigurationCommand, CompanyConfigurationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCompanyConfigurationCommandHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CompanyConfigurationDto> Handle(UpdateCompanyConfigurationCommand request, CancellationToken cancellationToken)
    {
        // Get the active configuration or create a new one if none exists
        var existingConfig = await _context.CompanyConfigurations
            .Where(c => c.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        CompanyConfiguration config;

        if (existingConfig != null)
        {
            // Update existing configuration
            existingConfig.UpdateBasicInfo(
                request.CompanyName,
                request.CompanyCode,
                request.CompanyDescription,
                request.WebsiteUrl);

            existingConfig.UpdateContactInfo(
                request.PrimaryEmail,
                request.PrimaryPhone,
                request.EmergencyContactNumber);

            existingConfig.UpdateAddress(
                request.Address,
                request.City,
                request.State,
                request.PostalCode,
                request.Country);

            existingConfig.UpdateGeographicInfo(
                request.DefaultLatitude,
                request.DefaultLongitude);

            existingConfig.UpdateBranding(
                request.LogoUrl,
                request.FaviconUrl,
                request.PrimaryColor,
                request.SecondaryColor,
                request.AccentColor);

            existingConfig.UpdateComplianceInfo(
                request.IndustryType,
                request.ComplianceStandards,
                request.RegulatoryAuthority);

            existingConfig.UpdateSystemSettings(
                request.TimeZone,
                request.DateFormat,
                request.Currency,
                request.Language);

            existingConfig.IncrementVersion();
            existingConfig.LastModifiedBy = _currentUserService.Name ?? "System";

            config = existingConfig;
        }
        else
        {
            // Create new configuration if none exists
            config = CompanyConfiguration.Create(
                request.CompanyName,
                request.CompanyCode,
                request.CompanyDescription,
                request.WebsiteUrl,
                request.PrimaryEmail);

            config.CreatedBy = _currentUserService.Name ?? "System";

            // Update all other fields
            config.UpdateContactInfo(
                request.PrimaryEmail,
                request.PrimaryPhone,
                request.EmergencyContactNumber);

            config.UpdateAddress(
                request.Address,
                request.City,
                request.State,
                request.PostalCode,
                request.Country);

            config.UpdateGeographicInfo(
                request.DefaultLatitude,
                request.DefaultLongitude);

            config.UpdateBranding(
                request.LogoUrl,
                request.FaviconUrl,
                request.PrimaryColor,
                request.SecondaryColor,
                request.AccentColor);

            config.UpdateComplianceInfo(
                request.IndustryType,
                request.ComplianceStandards,
                request.RegulatoryAuthority);

            config.UpdateSystemSettings(
                request.TimeZone,
                request.DateFormat,
                request.Currency,
                request.Language);

            _context.CompanyConfigurations.Add(config);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(config);
    }

    public async Task<CompanyConfigurationDto> Handle(CreateCompanyConfigurationCommand request, CancellationToken cancellationToken)
    {
        // Deactivate any existing active configurations
        var existingConfigs = await _context.CompanyConfigurations
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingConfigs)
        {
            existing.Deactivate();
        }

        // Create new configuration
        var config = CompanyConfiguration.Create(
            request.CompanyName,
            request.CompanyCode,
            request.CompanyDescription,
            request.WebsiteUrl,
            request.PrimaryEmail);

        config.CreatedBy = _currentUserService.Name ?? "System";

        _context.CompanyConfigurations.Add(config);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(config);
    }

    private static CompanyConfigurationDto MapToDto(CompanyConfiguration config)
    {
        return new CompanyConfigurationDto
        {
            Id = config.Id,
            CompanyName = config.CompanyName,
            CompanyCode = config.CompanyCode,
            CompanyDescription = config.CompanyDescription,
            WebsiteUrl = config.WebsiteUrl,
            LogoUrl = config.LogoUrl,
            FaviconUrl = config.FaviconUrl,
            PrimaryEmail = config.PrimaryEmail,
            PrimaryPhone = config.PrimaryPhone,
            EmergencyContactNumber = config.EmergencyContactNumber,
            Address = config.Address,
            City = config.City,
            State = config.State,
            PostalCode = config.PostalCode,
            Country = config.Country,
            FullAddress = config.FullAddress,
            DefaultLatitude = config.DefaultLatitude,
            DefaultLongitude = config.DefaultLongitude,
            HasGeographicCoordinates = config.HasGeographicCoordinates,
            PrimaryColor = config.PrimaryColor,
            SecondaryColor = config.SecondaryColor,
            AccentColor = config.AccentColor,
            HasBrandingColors = config.HasBrandingColors,
            IndustryType = config.IndustryType,
            ComplianceStandards = config.ComplianceStandards,
            RegulatoryAuthority = config.RegulatoryAuthority,
            TimeZone = config.TimeZone,
            DateFormat = config.DateFormat,
            Currency = config.Currency,
            Language = config.Language,
            IsActive = config.IsActive,
            Version = config.Version,
            CreatedAt = config.CreatedAt,
            CreatedBy = config.CreatedBy,
            LastModifiedAt = config.LastModifiedAt,
            LastModifiedBy = config.LastModifiedBy
        };
    }
}