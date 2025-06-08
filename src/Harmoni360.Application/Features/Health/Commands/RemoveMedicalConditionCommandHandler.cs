using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.Health.Commands;

public class RemoveMedicalConditionCommandHandler : IRequestHandler<RemoveMedicalConditionCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveMedicalConditionCommandHandler> _logger;
    private readonly ICacheService _cache;

    public RemoveMedicalConditionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<RemoveMedicalConditionCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(RemoveMedicalConditionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing medical condition ID: {ConditionId} by user {UserEmail}. Reason: {Reason}",
            request.Id, _currentUserService.Email, request.Reason ?? "Not specified");

        var medicalCondition = await _context.MedicalConditions
            .FirstOrDefaultAsync(mc => mc.Id == request.Id, cancellationToken);

        if (medicalCondition == null)
        {
            throw new ArgumentException($"Medical condition with ID {request.Id} not found", nameof(request.Id));
        }

        if (!medicalCondition.IsActive)
        {
            _logger.LogWarning("Attempted to remove already inactive medical condition: {ConditionId}", request.Id);
            return false; // Already removed
        }

        // Log removal of critical conditions
        if (medicalCondition.RequiresEmergencyAction)
        {
            _logger.LogWarning("CRITICAL MEDICAL CONDITION BEING REMOVED: {ConditionName} (ID: {ConditionId}) for health record {HealthRecordId}. Reason: {Reason}",
                medicalCondition.Name, request.Id, medicalCondition.HealthRecordId, request.Reason ?? "Not specified");
        }

        // Perform soft delete (deactivate)
        medicalCondition.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Medical condition removed successfully: {ConditionId}", medicalCondition.Id);

        // Invalidate health-related caches
        await InvalidateHealthCaches(medicalCondition.HealthRecordId);

        return true;
    }

    private async Task InvalidateHealthCaches(int healthRecordId)
    {
        await _cache.RemoveByTagAsync("health");
        await _cache.RemoveByTagAsync("health-records");
        await _cache.RemoveAsync($"health-record-{healthRecordId}");
        await _cache.RemoveByTagAsync("medical-conditions");
        _logger.LogInformation("Cache invalidated for medical condition removal from health record: {HealthRecordId}", healthRecordId);
    }
}