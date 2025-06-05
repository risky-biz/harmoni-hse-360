using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Infrastructure.Services;

public class IncidentAuditService : IIncidentAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public IncidentAuditService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public Task LogFieldChangeAsync(int incidentId, string fieldName, string? oldValue, string? newValue, string? description = null)
    {
        var auditLog = IncidentAuditLog.Create(
            incidentId,
            AuditActions.Updated,
            fieldName,
            oldValue,
            newValue,
            _currentUserService.Email ?? "System",
            description);

        _context.IncidentAuditLogs.Add(auditLog);
        // Changes will be saved by the calling service
        return Task.CompletedTask;
    }

    public Task LogActionAsync(int incidentId, string action, string? description = null)
    {
        var auditLog = IncidentAuditLog.CreateAction(
            incidentId,
            action,
            _currentUserService.Email ?? "System",
            description);

        _context.IncidentAuditLogs.Add(auditLog);
        // Changes will be saved by the calling service
        return Task.CompletedTask;
    }

    public async Task LogIncidentCreatedAsync(int incidentId)
    {
        await LogActionAsync(incidentId, AuditActions.Created, "Incident created");
    }

    public async Task LogIncidentViewedAsync(int incidentId)
    {
        await LogActionAsync(incidentId, AuditActions.Viewed, "Incident viewed");
    }

    public async Task LogStatusChangeAsync(int incidentId, string oldStatus, string newStatus)
    {
        await LogFieldChangeAsync(
            incidentId,
            "Status",
            oldStatus,
            newStatus,
            $"Status changed from {oldStatus} to {newStatus}");
    }

    public async Task LogSeverityChangeAsync(int incidentId, string oldSeverity, string newSeverity)
    {
        await LogFieldChangeAsync(
            incidentId,
            "Severity",
            oldSeverity,
            newSeverity,
            $"Severity changed from {oldSeverity} to {newSeverity}");
    }

    public async Task LogAttachmentAddedAsync(int incidentId, string fileName)
    {
        await LogActionAsync(
            incidentId,
            AuditActions.AttachmentAdded,
            $"Attachment '{fileName}' added");
    }

    public async Task LogAttachmentRemovedAsync(int incidentId, string fileName)
    {
        await LogActionAsync(
            incidentId,
            AuditActions.AttachmentRemoved,
            $"Attachment '{fileName}' removed");
    }

    public async Task LogCorrectiveActionAddedAsync(int incidentId, string description)
    {
        await LogActionAsync(
            incidentId,
            AuditActions.CorrectiveActionAdded,
            $"Corrective action added: {description}");
    }

    public async Task LogCorrectiveActionUpdatedAsync(int incidentId, string description)
    {
        await LogActionAsync(
            incidentId,
            AuditActions.CorrectiveActionUpdated,
            $"Corrective action updated: {description}");
    }

    public async Task LogCorrectiveActionRemovedAsync(int incidentId, string description)
    {
        await LogActionAsync(
            incidentId,
            AuditActions.CorrectiveActionRemoved,
            $"Corrective action removed: {description}");
    }

    public async Task<IEnumerable<IncidentAuditLog>> GetAuditTrailAsync(int incidentId)
    {
        return await _context.IncidentAuditLogs
            .Where(x => x.IncidentId == incidentId)
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync();
    }
}