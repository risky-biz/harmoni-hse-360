using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Infrastructure.Persistence.Repositories;

public class IncidentRepository : Repository<Incident>, IIncidentRepository
{
    public IncidentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Incident?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Reporter)
            .Include(i => i.Investigator)
            .Include(i => i.Attachments)
            .Include(i => i.InvolvedPersons)
                .ThenInclude(ip => ip.Person)
            .Include(i => i.CorrectiveActions)
                .ThenInclude(ca => ca.AssignedTo)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetByReporterIdAsync(int reporterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Reporter)
            .Where(i => i.ReporterId == reporterId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetBySeverityAsync(IncidentSeverity severity, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Reporter)
            .Where(i => i.Severity == severity)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Reporter)
            .Include(i => i.Investigator)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Reporter)
            .Where(i => i.IncidentDate >= startDate && i.IncidentDate <= endDate)
            .OrderByDescending(i => i.IncidentDate)
            .ToListAsync(cancellationToken);
    }
}