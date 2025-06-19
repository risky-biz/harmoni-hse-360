using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Entities.Inspections;
using Harmoni360.Domain.Entities.Audits;
using Harmoni360.Domain.Entities.Waste;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Harmoni360.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<ModulePermission> ModulePermissions => Set<ModulePermission>();
    public DbSet<RoleModulePermission> RoleModulePermissions => Set<RoleModulePermission>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentAttachment> IncidentAttachments => Set<IncidentAttachment>();
    public DbSet<IncidentInvolvedPerson> IncidentInvolvedPersons => Set<IncidentInvolvedPerson>();
    public DbSet<CorrectiveAction> CorrectiveActions => Set<CorrectiveAction>();
    public DbSet<IncidentAuditLog> IncidentAuditLogs => Set<IncidentAuditLog>();
    public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();
    public DbSet<EscalationAction> EscalationActions => Set<EscalationAction>();
    public DbSet<EscalationHistory> EscalationHistories => Set<EscalationHistory>();
    public DbSet<NotificationHistory> NotificationHistories => Set<NotificationHistory>();
    
    // Configuration Management
    public DbSet<CompanyConfiguration> CompanyConfigurations => Set<CompanyConfiguration>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<IncidentCategory> IncidentCategories => Set<IncidentCategory>();
    public DbSet<IncidentLocation> IncidentLocations => Set<IncidentLocation>();
    
    // PPE Management
    public DbSet<PPECategory> PPECategories => Set<PPECategory>();
    public DbSet<PPESize> PPESizes => Set<PPESize>();
    public DbSet<PPEStorageLocation> PPEStorageLocations => Set<PPEStorageLocation>();
    public DbSet<PPEItem> PPEItems => Set<PPEItem>();
    public DbSet<PPEAssignment> PPEAssignments => Set<PPEAssignment>();
    public DbSet<PPEInspection> PPEInspections => Set<PPEInspection>();
    public DbSet<PPERequest> PPERequests => Set<PPERequest>();
    public DbSet<PPERequestItem> PPERequestItems => Set<PPERequestItem>();
    public DbSet<PPEComplianceRequirement> PPEComplianceRequirements => Set<PPEComplianceRequirement>();
    
    // Hazard Management
    public DbSet<Hazard> Hazards => Set<Hazard>();
    public DbSet<HazardCategory> HazardCategories => Set<HazardCategory>();
    public DbSet<HazardType> HazardTypes => Set<HazardType>();
    public DbSet<HazardAttachment> HazardAttachments => Set<HazardAttachment>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
    public DbSet<HazardMitigationAction> HazardMitigationActions => Set<HazardMitigationAction>();
    public DbSet<HazardReassessment> HazardReassessments => Set<HazardReassessment>();
    public DbSet<HazardAuditLog> HazardAuditLogs => Set<HazardAuditLog>();
    
    // Health Management
    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();
    public DbSet<MedicalCondition> MedicalConditions => Set<MedicalCondition>();
    public DbSet<VaccinationRecord> VaccinationRecords => Set<VaccinationRecord>();
    public DbSet<HealthIncident> HealthIncidents => Set<HealthIncident>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    
    // Security Management
    public DbSet<SecurityIncident> SecurityIncidents => Set<SecurityIncident>();
    public DbSet<SecurityIncidentAttachment> SecurityIncidentAttachments => Set<SecurityIncidentAttachment>();
    public DbSet<SecurityIncidentInvolvedPerson> SecurityIncidentInvolvedPersons => Set<SecurityIncidentInvolvedPerson>();
    public DbSet<SecurityIncidentResponse> SecurityIncidentResponses => Set<SecurityIncidentResponse>();
    public DbSet<ThreatAssessment> ThreatAssessments => Set<ThreatAssessment>();
    public DbSet<SecurityControl> SecurityControls => Set<SecurityControl>();
    public DbSet<ThreatIndicator> ThreatIndicators => Set<ThreatIndicator>();
    public DbSet<Domain.Entities.Security.SecurityAuditLog> SecurityAuditLogs => Set<Domain.Entities.Security.SecurityAuditLog>();
    
    // Work Permit Management
    public DbSet<WorkPermit> WorkPermits => Set<WorkPermit>();
    public DbSet<WorkPermitAttachment> WorkPermitAttachments => Set<WorkPermitAttachment>();
    public DbSet<WorkPermitApproval> WorkPermitApprovals => Set<WorkPermitApproval>();
    public DbSet<WorkPermitHazard> WorkPermitHazards => Set<WorkPermitHazard>();
    public DbSet<WorkPermitPrecaution> WorkPermitPrecautions => Set<WorkPermitPrecaution>();
    public DbSet<WorkPermitSettings> WorkPermitSettings => Set<WorkPermitSettings>();
    public DbSet<WorkPermitSafetyVideo> WorkPermitSafetyVideos => Set<WorkPermitSafetyVideo>();
    
    // Inspection Management
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<InspectionItem> InspectionItems => Set<InspectionItem>();
    public DbSet<InspectionFinding> InspectionFindings => Set<InspectionFinding>();
    public DbSet<InspectionAttachment> InspectionAttachments => Set<InspectionAttachment>();
    public DbSet<InspectionComment> InspectionComments => Set<InspectionComment>();
    public DbSet<Domain.Entities.Inspections.FindingAttachment> FindingAttachments => Set<Domain.Entities.Inspections.FindingAttachment>();
    
    // Audit Management
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<AuditItem> AuditItems => Set<AuditItem>();
    public DbSet<AuditFinding> AuditFindings => Set<AuditFinding>();
    public DbSet<AuditAttachment> AuditAttachments => Set<AuditAttachment>();
    public DbSet<AuditComment> AuditComments => Set<AuditComment>();
    public DbSet<Domain.Entities.Audits.FindingAttachment> AuditFindingAttachments => Set<Domain.Entities.Audits.FindingAttachment>();
    
    // Training Management
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<TrainingParticipant> TrainingParticipants => Set<TrainingParticipant>();
    public DbSet<TrainingRequirement> TrainingRequirements => Set<TrainingRequirement>();
    public DbSet<TrainingAttachment> TrainingAttachments => Set<TrainingAttachment>();
    public DbSet<TrainingComment> TrainingComments => Set<TrainingComment>();
    public DbSet<TrainingCertification> TrainingCertifications => Set<TrainingCertification>();
    
    // License Management
    public DbSet<License> Licenses => Set<License>();
    public DbSet<LicenseAttachment> LicenseAttachments => Set<LicenseAttachment>();
    public DbSet<LicenseRenewal> LicenseRenewals => Set<LicenseRenewal>();
    public DbSet<LicenseCondition> LicenseConditions => Set<LicenseCondition>();
    public DbSet<LicenseAuditLog> LicenseAuditLogs => Set<LicenseAuditLog>();

    // Waste Management
    public DbSet<WasteReport> WasteReports => Set<WasteReport>();
    public DbSet<WasteAttachment> WasteAttachments => Set<WasteAttachment>();
    public DbSet<WasteType> WasteTypes => Set<WasteType>();
    public DbSet<DisposalProvider> DisposalProviders => Set<DisposalProvider>();
    public DbSet<WasteDisposalRecord> WasteDisposalRecords => Set<WasteDisposalRecord>();
    public DbSet<WasteComment> WasteComments => Set<WasteComment>();
    public DbSet<WasteCompliance> WasteCompliances => Set<WasteCompliance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure all DateTime properties to use UTC
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        property.ClrType == typeof(DateTime)
                            ? new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                                v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                            : new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : v,
                                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v));
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(e => e.CreatedBy).CurrentValue = _currentUserService.Email;
                    entry.Property(e => e.CreatedAt).CurrentValue = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Property(e => e.LastModifiedBy).CurrentValue = _currentUserService.Email;
                    entry.Property(e => e.LastModifiedAt).CurrentValue = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
