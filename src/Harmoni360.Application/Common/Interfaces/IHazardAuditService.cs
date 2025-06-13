using System.Threading.Tasks;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Common.Interfaces
{
    public interface IHazardAuditService
    {
        Task LogCreationAsync(Hazard hazard);
        Task LogUpdateAsync(int hazardId, string fieldName, object? oldValue, object? newValue, string? changeDescription = null);
        Task LogStatusChangeAsync(int hazardId, string oldStatus, string newStatus, string reason);
        Task LogRiskAssessmentAsync(int hazardId, string action, string details);
        Task LogMitigationActionAsync(int hazardId, string action, string details);
        Task LogAttachmentAsync(int hazardId, string action, string fileName);
        Task LogViewAsync(int hazardId);
        Task LogExportAsync(int hazardId, string format);
        Task LogActionAsync(int hazardId, string action, string? details = null);
    }
}