using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record CreateLicenseCommand : IRequest<LicenseDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public LicenseType Type { get; init; }
    public LicensePriority Priority { get; init; } = LicensePriority.Medium;
    
    // License Details
    public string LicenseNumber { get; init; } = string.Empty;
    public string IssuingAuthority { get; init; } = string.Empty;
    public string HolderName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    
    // Dates
    public DateTime IssuedDate { get; init; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; init; }
    
    // Additional Properties
    public string Scope { get; init; } = string.Empty;
    public string Restrictions { get; init; } = string.Empty;
    public string Conditions { get; init; } = string.Empty;
    public RiskLevel RiskLevel { get; init; } = RiskLevel.Medium;
    public decimal? LicenseFee { get; init; }
    public string Currency { get; init; } = "USD";
    public bool IsCriticalLicense { get; init; } = false;
    public bool RequiresInsurance { get; init; } = false;
    public decimal? RequiredInsuranceAmount { get; init; }
    
    // Regulatory Information
    public string RegulatoryFramework { get; init; } = string.Empty;
    public string ApplicableRegulations { get; init; } = string.Empty;
    public string ComplianceStandards { get; init; } = string.Empty;
    
    // Renewal Information
    public bool RenewalRequired { get; init; } = true;
    public int RenewalPeriodDays { get; init; } = 90;
    public bool AutoRenewal { get; init; } = false;
    public string RenewalProcedure { get; init; } = string.Empty;
    
    // Conditions
    public List<CreateLicenseConditionCommand> LicenseConditions { get; init; } = new();
}

public record CreateLicenseConditionCommand
{
    public string ConditionType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsMandatory { get; init; } = true;
    public DateTime? DueDate { get; init; }
    public string ResponsiblePerson { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}