using System.Threading.Tasks;
using System.Collections.Generic;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Application.Common.Interfaces
{
    public interface IWasteAuditService
    {
        Task LogWasteReportCreatedAsync(WasteReport wasteReport);
        Task LogWasteReportUpdatedAsync(int wasteReportId, string fieldName, object? oldValue, object? newValue, string? changeDescription = null);
        Task LogDisposalStatusChangeAsync(int wasteReportId, string oldStatus, string newStatus, string reason);
        Task LogDisposalRecordCreatedAsync(int wasteReportId, string details);
        Task LogComplianceCheckAsync(int wasteReportId, string action, string details, bool isCritical = false);
        Task LogAttachmentAsync(int wasteReportId, string action, string fileName);
        Task LogViewAsync(int wasteReportId);
        Task LogExportAsync(int wasteReportId, string format);
        Task LogCommentAsync(int wasteReportId, string action, string? comment = null);
        Task LogActionAsync(int wasteReportId, string action, string? details = null, bool isCritical = false);
        Task<IEnumerable<WasteAuditLog>> GetAuditTrailAsync(int wasteReportId);
        Task<IEnumerable<WasteAuditLog>> GetComplianceAuditTrailAsync(DateTime fromDate, DateTime toDate);
    }
}