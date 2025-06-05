using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Application.Common.Interfaces;

public interface IIncidentAuditService
{
    Task LogFieldChangeAsync(int incidentId, string fieldName, string? oldValue, string? newValue, string? description = null);
    Task LogActionAsync(int incidentId, string action, string? description = null);
    Task LogIncidentCreatedAsync(int incidentId);
    Task LogIncidentViewedAsync(int incidentId);
    Task LogStatusChangeAsync(int incidentId, string oldStatus, string newStatus);
    Task LogSeverityChangeAsync(int incidentId, string oldSeverity, string newSeverity);
    Task LogAttachmentAddedAsync(int incidentId, string fileName);
    Task LogAttachmentRemovedAsync(int incidentId, string fileName);
    Task LogCorrectiveActionAddedAsync(int incidentId, string description);
    Task LogCorrectiveActionUpdatedAsync(int incidentId, string description);
    Task LogCorrectiveActionRemovedAsync(int incidentId, string description);
    Task<IEnumerable<IncidentAuditLog>> GetAuditTrailAsync(int incidentId);
}