using Harmoni360.Domain.Entities;

namespace Harmoni360.Domain.Interfaces;

public interface IIncidentRepository : IRepository<Incident>
{
    Task<Incident?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Incident>> GetByReporterIdAsync(int reporterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Incident>> GetBySeverityAsync(IncidentSeverity severity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Incident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Incident>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}