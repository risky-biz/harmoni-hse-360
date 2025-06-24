using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Waste;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Infrastructure.Services
{
    public class WasteAuditService : IWasteAuditService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WasteAuditService(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogWasteReportCreatedAsync(WasteReport wasteReport)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReport.Id,
                Action = "Created",
                FieldName = "System",
                ChangeDescription = $"Waste report \"{wasteReport.Title}\" created",
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = false
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogWasteReportUpdatedAsync(int wasteReportId, string fieldName, object? oldValue, object? newValue, string? changeDescription = null)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = "Updated",
                FieldName = fieldName,
                OldValue = oldValue?.ToString(),
                NewValue = newValue?.ToString(),
                ChangeDescription = changeDescription ?? $"Field {fieldName} updated",
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = IsCriticalField(fieldName)
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogDisposalStatusChangeAsync(int wasteReportId, string oldStatus, string newStatus, string reason)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = "Disposal Status Changed",
                FieldName = "DisposalStatus",
                OldValue = oldStatus,
                NewValue = newStatus,
                ChangeDescription = $"Disposal status changed from {oldStatus} to {newStatus}. Reason: {reason}",
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = true,
                ComplianceNotes = reason
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogDisposalRecordCreatedAsync(int wasteReportId, string details)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = "Disposal Record Created",
                FieldName = "DisposalRecord",
                ChangeDescription = details,
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = true,
                ComplianceNotes = "Disposal record for regulatory compliance"
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogComplianceCheckAsync(int wasteReportId, string action, string details, bool isCritical = false)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = $"Compliance {action}",
                FieldName = "Compliance",
                ChangeDescription = details,
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = isCritical,
                ComplianceNotes = details
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogAttachmentAsync(int wasteReportId, string action, string fileName)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = $"Attachment {action}",
                FieldName = "Attachment",
                ChangeDescription = $"File {fileName} was {action.ToLower()}",
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = false
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogViewAsync(int wasteReportId)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = "Viewed",
                FieldName = "System",
                ChangeDescription = "Waste report viewed",
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = false
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogExportAsync(int wasteReportId, string format)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = "Exported",
                FieldName = "System",
                ChangeDescription = $"Waste report exported to {format}",
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = false
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogCommentAsync(int wasteReportId, string action, string? comment = null)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = $"Comment {action}",
                FieldName = "Comment",
                ChangeDescription = comment != null ? $"Comment: {comment}" : $"Comment {action.ToLower()}",
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = false
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogActionAsync(int wasteReportId, string action, string? details = null, bool isCritical = false)
        {
            var auditLog = new WasteAuditLog
            {
                WasteReportId = wasteReportId,
                Action = action,
                FieldName = "System",
                ChangeDescription = details ?? action,
                ChangedBy = GetCurrentUserName(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                IsCriticalAction = isCritical,
                ComplianceNotes = isCritical ? details : null
            };

            _context.WasteAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WasteAuditLog>> GetAuditTrailAsync(int wasteReportId)
        {
            return await _context.WasteAuditLogs
                .Where(log => log.WasteReportId == wasteReportId)
                .OrderByDescending(log => log.ChangedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<WasteAuditLog>> GetComplianceAuditTrailAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.WasteAuditLogs
                .Where(log => log.ChangedAt >= fromDate && log.ChangedAt <= toDate && 
                             (log.IsCriticalAction || !string.IsNullOrEmpty(log.ComplianceNotes)))
                .OrderByDescending(log => log.ChangedAt)
                .ToListAsync();
        }

        private string GetCurrentUserName()
        {
            return _currentUserService.IsAuthenticated ? _currentUserService.Name : "System";
        }

        private string? GetClientIpAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            return ipAddress;
        }

        private string? GetUserAgent()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Request.Headers["User-Agent"].FirstOrDefault();
        }

        private static bool IsCriticalField(string fieldName)
        {
            var criticalFields = new[] 
            { 
                "Category", "Classification", "DisposalStatus", "DisposalMethod", 
                "Quantity", "Unit", "HazardLevel", "ApprovedBy", "DisposedBy"
            };
            return criticalFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);
        }
    }
}