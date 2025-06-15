using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record UpdateLicenseCommand : IRequest<LicenseDto>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public LicensePriority Priority { get; init; } = LicensePriority.Medium;
    
    // Scope and Coverage (these exist in License entity)
    public string Scope { get; init; } = string.Empty;
    public string Restrictions { get; init; } = string.Empty;
    public string Conditions { get; init; } = string.Empty;
    
    // Regulatory Information (these exist in License entity)
    public string RegulatoryFramework { get; init; } = string.Empty;
    public string ApplicableRegulations { get; init; } = string.Empty;
    public string ComplianceStandards { get; init; } = string.Empty;
    
    // Risk Information (these exist in License entity)
    public RiskLevel? RiskLevel { get; init; }
    public bool? IsCriticalLicense { get; init; }
    public bool? RequiresInsurance { get; init; }
    public decimal? RequiredInsuranceAmount { get; init; }
    
    // Renewal Information (these exist in License entity)
    public bool? RenewalRequired { get; init; }
    public int? RenewalPeriodDays { get; init; }
    public bool? AutoRenewal { get; init; }
    public string? RenewalProcedure { get; init; }
}