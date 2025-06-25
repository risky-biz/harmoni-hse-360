using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Entities.Inspections;
using Harmoni360.Domain.Entities.Audits;
using Harmoni360.Domain.Entities.Waste;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<UserActivityLog> UserActivityLogs { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<ModulePermission> ModulePermissions { get; }
    DbSet<RoleModulePermission> RoleModulePermissions { get; }
    DbSet<Incident> Incidents { get; }
    DbSet<IncidentAttachment> IncidentAttachments { get; }
    DbSet<IncidentInvolvedPerson> IncidentInvolvedPersons { get; }
    DbSet<CorrectiveAction> CorrectiveActions { get; }
    DbSet<IncidentAuditLog> IncidentAuditLogs { get; }
    
    // Configuration Management
    DbSet<CompanyConfiguration> CompanyConfigurations { get; }
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
    DbSet<Person> Persons { get; }
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
    DbSet<WorkPermitSettings> WorkPermitSettings { get; }
    DbSet<WorkPermitSafetyVideo> WorkPermitSafetyVideos { get; }
    
    // Inspection Management
    DbSet<Inspection> Inspections { get; }
    DbSet<InspectionItem> InspectionItems { get; }
    DbSet<InspectionFinding> InspectionFindings { get; }
    DbSet<InspectionAttachment> InspectionAttachments { get; }
    DbSet<InspectionComment> InspectionComments { get; }
    DbSet<Domain.Entities.Inspections.FindingAttachment> FindingAttachments { get; }
    
    // Audit Management
    DbSet<Audit> Audits { get; }
    DbSet<AuditItem> AuditItems { get; }
    DbSet<AuditFinding> AuditFindings { get; }
    DbSet<AuditAttachment> AuditAttachments { get; }
    DbSet<AuditComment> AuditComments { get; }
    DbSet<Domain.Entities.Audits.FindingAttachment> AuditFindingAttachments { get; }
    
    // Training Management
    DbSet<Training> Trainings { get; }
    DbSet<TrainingParticipant> TrainingParticipants { get; }
    DbSet<TrainingRequirement> TrainingRequirements { get; }
    DbSet<TrainingAttachment> TrainingAttachments { get; }
    DbSet<TrainingComment> TrainingComments { get; }
    DbSet<TrainingCertification> TrainingCertifications { get; }
    
    // License Management
    DbSet<License> Licenses { get; }
    DbSet<LicenseAttachment> LicenseAttachments { get; }
    DbSet<LicenseRenewal> LicenseRenewals { get; }
    DbSet<LicenseCondition> LicenseConditions { get; }
    DbSet<LicenseAuditLog> LicenseAuditLogs { get; }
    // Waste Management
    DbSet<WasteReport> WasteReports { get; }
    DbSet<WasteAttachment> WasteAttachments { get; }
    DbSet<WasteType> WasteTypes { get; }
    DbSet<DisposalProvider> DisposalProviders { get; }
    DbSet<WasteDisposalRecord> WasteDisposalRecords { get; }
    DbSet<WasteComment> WasteComments { get; }
    DbSet<WasteCompliance> WasteCompliances { get; }
    DbSet<WasteAuditLog> WasteAuditLogs { get; }

    // Module Configuration Management
    DbSet<ModuleConfiguration> ModuleConfigurations { get; }
    DbSet<ModuleDependency> ModuleDependencies { get; }
    DbSet<ModuleConfigurationAuditLog> ModuleConfigurationAuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
