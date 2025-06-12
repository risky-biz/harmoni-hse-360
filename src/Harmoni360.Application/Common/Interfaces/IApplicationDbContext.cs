using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<ModulePermission> ModulePermissions { get; }
    DbSet<RoleModulePermission> RoleModulePermissions { get; }
    DbSet<Incident> Incidents { get; }
    DbSet<IncidentAttachment> IncidentAttachments { get; }
    DbSet<IncidentInvolvedPerson> IncidentInvolvedPersons { get; }
    DbSet<CorrectiveAction> CorrectiveActions { get; }
    DbSet<IncidentAuditLog> IncidentAuditLogs { get; }
    
    // Configuration Management
    DbSet<Department> Departments { get; }
    DbSet<IncidentCategory> IncidentCategories { get; }
    DbSet<IncidentLocation> IncidentLocations { get; }
    
    // PPE Management
    DbSet<PPECategory> PPECategories { get; }
    DbSet<PPESize> PPESizes { get; }
    DbSet<PPEStorageLocation> PPEStorageLocations { get; }
    DbSet<PPEItem> PPEItems { get; }
    DbSet<PPEAssignment> PPEAssignments { get; }
    DbSet<PPEInspection> PPEInspections { get; }
    DbSet<PPERequest> PPERequests { get; }
    DbSet<PPERequestItem> PPERequestItems { get; }
    DbSet<PPEComplianceRequirement> PPEComplianceRequirements { get; }
    
    // Hazard Management
    DbSet<Hazard> Hazards { get; }
    DbSet<HazardAttachment> HazardAttachments { get; }
    DbSet<RiskAssessment> RiskAssessments { get; }
    DbSet<HazardMitigationAction> HazardMitigationActions { get; }
    DbSet<HazardReassessment> HazardReassessments { get; }
    DbSet<HazardAuditLog> HazardAuditLogs { get; }
    
    // Health Management
    DbSet<HealthRecord> HealthRecords { get; }
    DbSet<MedicalCondition> MedicalConditions { get; }
    DbSet<VaccinationRecord> VaccinationRecords { get; }
    DbSet<HealthIncident> HealthIncidents { get; }
    DbSet<EmergencyContact> EmergencyContacts { get; }
    
    // Security Management
    DbSet<SecurityIncident> SecurityIncidents { get; }
    DbSet<SecurityIncidentAttachment> SecurityIncidentAttachments { get; }
    DbSet<SecurityIncidentInvolvedPerson> SecurityIncidentInvolvedPersons { get; }
    DbSet<SecurityIncidentResponse> SecurityIncidentResponses { get; }
    DbSet<ThreatAssessment> ThreatAssessments { get; }
    DbSet<SecurityControl> SecurityControls { get; }
    DbSet<ThreatIndicator> ThreatIndicators { get; }
    DbSet<SecurityAuditLog> SecurityAuditLogs { get; }
    
    // Work Permit Management
    DbSet<WorkPermit> WorkPermits { get; }
    DbSet<WorkPermitAttachment> WorkPermitAttachments { get; }
    DbSet<WorkPermitApproval> WorkPermitApprovals { get; }
    DbSet<WorkPermitHazard> WorkPermitHazards { get; }
    DbSet<WorkPermitPrecaution> WorkPermitPrecautions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}