using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.Health.Commands;

public class DeactivateHealthRecordCommandHandler : IRequestHandler<DeactivateHealthRecordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeactivateHealthRecordCommandHandler> _logger;
    private readonly ICacheService _cache;

    public DeactivateHealthRecordCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<DeactivateHealthRecordCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(DeactivateHealthRecordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating health record ID: {HealthRecordId} by user {UserEmail}. Reason: {Reason}",
            request.Id, _currentUserService.Email, request.Reason ?? "Not specified");

        var healthRecord = await _context.HealthRecords
            .FirstOrDefaultAsync(hr => hr.Id == request.Id, cancellationToken);

        if (healthRecord == null)
        {
            throw new ArgumentException($"Health record with ID {request.Id} not found", nameof(request.Id));
        }

        if (!healthRecord.IsActive)
        {
            _logger.LogWarning("Attempted to deactivate already inactive health record: {HealthRecordId}", request.Id);
            return false; // Already deactivated
        }

        // Check for active health incidents before deactivation
        var activeHealthIncidents = await _context.HealthIncidents
            .Where(hi => hi.HealthRecordId == request.Id && !hi.IsResolved)
            .CountAsync(cancellationToken);

        if (activeHealthIncidents > 0)
        {
            throw new InvalidOperationException(
                $"Cannot deactivate health record. There are {activeHealthIncidents} unresolved health incidents.");
        }

        // Perform soft delete
        healthRecord.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Health record deactivated successfully: {HealthRecordId}", healthRecord.Id);

        // Invalidate health-related caches
        await InvalidateHealthCaches(healthRecord.Id);

        return true;
    }

    private async Task InvalidateHealthCaches(int healthRecordId)
    {
        await _cache.RemoveByTagAsync("health");
        await _cache.RemoveByTagAsync("health-records");
        await _cache.RemoveAsync($"health-record-{healthRecordId}");
        _logger.LogInformation("Cache invalidated for health record deactivation: {HealthRecordId}", healthRecordId);
    }
}