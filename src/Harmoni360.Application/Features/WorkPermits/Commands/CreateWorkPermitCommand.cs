using MediatR;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Application.Features.WorkPermits.Commands;

public record CreateWorkPermitCommand : IRequest<WorkPermitDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public WorkPermitType Type { get; init; }
    public string WorkLocation { get; init; } = string.Empty;
    public DateTime PlannedStartDate { get; init; }
    public DateTime PlannedEndDate { get; init; }
    public string WorkScope { get; init; } = string.Empty;
    public int NumberOfWorkers { get; init; }
    
    // Personnel (requestor details will be from current user)
    public string ContactPhone { get; init; } = string.Empty;
    public string WorkSupervisor { get; init; } = string.Empty;
    public string SafetyOfficer { get; init; } = string.Empty;
    public string ContractorCompany { get; init; } = string.Empty;
    
    // Equipment and Materials
    public string EquipmentToBeUsed { get; init; } = string.Empty;
    public string MaterialsInvolved { get; init; } = string.Empty;
    
    // Location
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string Address { get; init; } = string.Empty;
    public string LocationDescription { get; init; } = string.Empty;
    
    // Safety Requirements
    public bool RequiresHotWorkPermit { get; init; }
    public bool RequiresConfinedSpaceEntry { get; init; }
    public bool RequiresElectricalIsolation { get; init; }
    public bool RequiresHeightWork { get; init; }
    public bool RequiresRadiationWork { get; init; }
    public bool RequiresExcavation { get; init; }
    public bool RequiresFireWatch { get; init; }
    public bool RequiresGasMonitoring { get; init; }
    
    // Indonesian Compliance
    public string K3LicenseNumber { get; init; } = string.Empty;
    public string CompanyWorkPermitNumber { get; init; } = string.Empty;
    public bool IsJamsostekCompliant { get; init; }
    public bool HasSMK3Compliance { get; init; }
    public string EnvironmentalPermitNumber { get; init; } = string.Empty;
    
    // Risk Assessment
    public string RiskAssessmentSummary { get; init; } = string.Empty;
    public string EmergencyProcedures { get; init; } = string.Empty;
    
    // Hazards
    public List<CreateWorkPermitHazardCommand> Hazards { get; init; } = new();
    
    // Precautions
    public List<CreateWorkPermitPrecautionCommand> Precautions { get; init; } = new();
}

public record CreateWorkPermitHazardCommand
{
    public string HazardDescription { get; init; } = string.Empty;
    public Harmoni360.Domain.Enums.HazardCategory Category { get; init; }
    public int Likelihood { get; init; } = 1;
    public int Severity { get; init; } = 1;
    public string ControlMeasures { get; init; } = string.Empty;
    public string ResponsiblePerson { get; init; } = string.Empty;
}

public record CreateWorkPermitPrecautionCommand
{
    public string PrecautionDescription { get; init; } = string.Empty;
    public PrecautionCategory Category { get; init; }
    public bool IsRequired { get; init; } = true;
    public int Priority { get; init; } = 1;
    public string ResponsiblePerson { get; init; } = string.Empty;
    public string VerificationMethod { get; init; } = string.Empty;
    public bool IsK3Requirement { get; init; }
    public string K3StandardReference { get; init; } = string.Empty;
    public bool IsMandatoryByLaw { get; init; }
}