using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HarmoniHSE360.Application.Common.Interfaces;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class DeleteIncidentCommandHandler : IRequestHandler<DeleteIncidentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteIncidentCommandHandler> _logger;

    public DeleteIncidentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cache,
        ILogger<DeleteIncidentCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteIncidentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting incident {IncidentId} by user {UserEmail}",
            request.Id, _currentUserService.Email);

        var incident = await _context.Incidents
            .Include(i => i.Attachments)
            .Include(i => i.InvolvedPersons)
            .Include(i => i.CorrectiveActions)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (incident == null)
        {
            _logger.LogWarning("Incident {IncidentId} not found", request.Id);
            return false;
        }

        // TODO: Add authorization check here
        // For now, any authenticated user can delete any incident

        // Remove the incident (cascading delete will handle related entities)
        _context.Incidents.Remove(incident);
        await _context.SaveChangesAsync(cancellationToken);

        // Clear incident cache after successful deletion
        await InvalidateIncidentCaches(incident.ReporterId?.ToString() ?? "unknown");

        _logger.LogInformation("Incident {IncidentId} deleted successfully", request.Id);

        return true;
    }

    private async Task InvalidateIncidentCaches(string reporterId)
    {
        await _cache.RemoveByTagAsync("incidents");
        _logger.LogInformation("Cache invalidated for incident deletion, reporter: {ReporterId}", reporterId);
    }
}