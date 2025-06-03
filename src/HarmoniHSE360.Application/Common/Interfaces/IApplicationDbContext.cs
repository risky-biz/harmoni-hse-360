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
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}