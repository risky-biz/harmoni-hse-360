using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Incident> Incidents { get; }
    DbSet<IncidentAttachment> IncidentAttachments { get; }
    DbSet<IncidentInvolvedPerson> IncidentInvolvedPersons { get; }
    DbSet<CorrectiveAction> CorrectiveActions { get; }
    DbSet<IncidentAuditLog> IncidentAuditLogs { get; }
    
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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}