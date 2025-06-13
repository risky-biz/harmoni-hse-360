using System;
using System.Linq;
using System.Threading.Tasks;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Infrastructure.Services
{
    public class HazardAuditService : IHazardAuditService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HazardAuditService(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogCreationAsync(Hazard hazard)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazard.Id,
                Action = "Created",
                FieldName = "System",
                ChangeDescription = $"Hazard \"{hazard.Title}\" created",
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogUpdateAsync(int hazardId, string fieldName, object? oldValue, object? newValue, string? changeDescription = null)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazardId,
                Action = "Updated",
                FieldName = fieldName,
                OldValue = oldValue?.ToString(),
                NewValue = newValue?.ToString(),
                ChangeDescription = changeDescription,
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogStatusChangeAsync(int hazardId, string oldStatus, string newStatus, string reason)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazardId,
                Action = "Status Changed",
                FieldName = "Status",
                OldValue = oldStatus,
                NewValue = newStatus,
                ChangeDescription = $"Status changed from {oldStatus} to {newStatus}{(!string.IsNullOrEmpty(reason) ? $". Reason: {reason}" : "")}",
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogRiskAssessmentAsync(int hazardId, string action, string details)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazardId,
                Action = action,
                FieldName = "Risk Assessment",
                ChangeDescription = details,
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogMitigationActionAsync(int hazardId, string action, string details)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazardId,
                Action = action,
                FieldName = "Mitigation Action",
                ChangeDescription = details,
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogAttachmentAsync(int hazardId, string action, string fileName)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazardId,
                Action = action,
                FieldName = "Attachment",
                ChangeDescription = $"{action} file: {fileName}",
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogViewAsync(int hazardId)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazardId,
                Action = "Viewed",
                FieldName = "System",
                ChangeDescription = "Hazard details viewed",
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogExportAsync(int hazardId, string format)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazardId,
                Action = "Exported",
                FieldName = "System",
                ChangeDescription = $"Hazard exported as {format}",
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogActionAsync(int hazardId, string action, string? details = null)
        {
            var auditLog = new HazardAuditLog
            {
                HazardId = hazardId,
                Action = action,
                FieldName = "System",
                ChangeDescription = details ?? action,
                ChangedBy = await GetCurrentUserNameAsync(),
                ChangedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _context.HazardAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        private async Task<string> GetCurrentUserNameAsync()
        {
            var userId = _currentUserService.UserId;
            if (userId == 0)
                return "System";

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user != null ? user.Name : "Unknown User";
        }

        private string? GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return null;

                // Check for proxied IP address
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',').First().Trim();
                }

                return httpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string? GetUserAgent()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}