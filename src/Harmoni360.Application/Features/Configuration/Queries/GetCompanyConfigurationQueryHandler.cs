using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Configuration.Queries;

public class GetCompanyConfigurationQueryHandler : 
    IRequestHandler<GetCompanyConfigurationQuery, CompanyConfigurationDto?>,
    IRequestHandler<GetActiveCompanyConfigurationQuery, CompanyConfigurationDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCompanyConfigurationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyConfigurationDto?> Handle(GetCompanyConfigurationQuery request, CancellationToken cancellationToken)
    {
        var config = await _context.CompanyConfigurations
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return config == null ? null : MapToDto(config);
    }

    public async Task<CompanyConfigurationDto?> Handle(GetActiveCompanyConfigurationQuery request, CancellationToken cancellationToken)
    {
        var config = await _context.CompanyConfigurations
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return config == null ? null : MapToDto(config);
    }

    private static CompanyConfigurationDto MapToDto(Domain.Entities.CompanyConfiguration config)
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