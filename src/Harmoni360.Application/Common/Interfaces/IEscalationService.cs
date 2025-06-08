using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Common.Interfaces;

public interface IEscalationService
{
    Task ProcessEscalationRulesAsync(int incidentId, CancellationToken cancellationToken = default);
    Task CheckOverdueIncidentsAsync(CancellationToken cancellationToken = default);
    Task TriggerManualEscalationAsync(int incidentId, string reason, string escalatedBy, CancellationToken cancellationToken = default);
    Task<List<EscalationRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);
}

