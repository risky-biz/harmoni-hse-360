using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class UpdateWorkPermitCommand : IRequest<WorkPermitDto>
    {
        public int Id { get; set; }
        
        // Basic Information
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public WorkPermitType Type { get; set; }
        public WorkPermitPriority Priority { get; set; }
        
        // Work Details
        public string WorkLocation { get; set; } = string.Empty;
        public GeoLocation? GeoLocation { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public int EstimatedDuration { get; set; }
        
        // Personnel Information
        public string ContactPhone { get; set; } = string.Empty;
        public string WorkSupervisor { get; set; } = string.Empty;
        public string SafetyOfficer { get; set; } = string.Empty;
        
        // Work Scope
        public string WorkScope { get; set; } = string.Empty;
        public string EquipmentToBeUsed { get; set; } = string.Empty;
        public string MaterialsInvolved { get; set; } = string.Empty;
        public int NumberOfWorkers { get; set; }
        public string ContractorCompany { get; set; } = string.Empty;
        
        // Safety Requirements
        public bool RequiresHotWorkPermit { get; set; }
        public bool RequiresConfinedSpaceEntry { get; set; }
        public bool RequiresElectricalIsolation { get; set; }
        public bool RequiresHeightWork { get; set; }
        public bool RequiresRadiationWork { get; set; }
        public bool RequiresExcavation { get; set; }
        public bool RequiresFireWatch { get; set; }
        public bool RequiresGasMonitoring { get; set; }
        
        // Indonesian K3 Compliance
        public string K3LicenseNumber { get; set; } = string.Empty;
        public string CompanyWorkPermitNumber { get; set; } = string.Empty;
        public bool IsJamsostekCompliant { get; set; }
        public bool HasSMK3Compliance { get; set; }
        public string EnvironmentalPermitNumber { get; set; } = string.Empty;
        
        // Risk Assessment
        public RiskLevel RiskLevel { get; set; }
        public string RiskAssessmentSummary { get; set; } = string.Empty;
        public string EmergencyProcedures { get; set; } = string.Empty;
    }
}