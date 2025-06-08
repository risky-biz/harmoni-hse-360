using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
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
    public DbSet<HazardAttachment> HazardAttachments => Set<HazardAttachment>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
    public DbSet<HazardMitigationAction> HazardMitigationActions => Set<HazardMitigationAction>();
    public DbSet<HazardReassessment> HazardReassessments => Set<HazardReassessment>();
    
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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