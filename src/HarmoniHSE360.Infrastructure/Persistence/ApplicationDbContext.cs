using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Common;
using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HarmoniHSE360.Infrastructure.Persistence;

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
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentAttachment> IncidentAttachments => Set<IncidentAttachment>();
    public DbSet<IncidentInvolvedPerson> IncidentInvolvedPersons => Set<IncidentInvolvedPerson>();
    public DbSet<CorrectiveAction> CorrectiveActions => Set<CorrectiveAction>();
    public DbSet<IncidentAuditLog> IncidentAuditLogs => Set<IncidentAuditLog>();
    public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();
    public DbSet<EscalationAction> EscalationActions => Set<EscalationAction>();
    public DbSet<EscalationHistory> EscalationHistories => Set<EscalationHistory>();
    public DbSet<NotificationHistory> NotificationHistories => Set<NotificationHistory>();

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