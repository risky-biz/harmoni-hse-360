using System;

namespace Harmoni360.Application.Features.WasteReports.DTOs
{
    public class WasteAuditLogDto
    {
        public int Id { get; set; }
        public int WasteReportId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? ChangeDescription { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ComplianceNotes { get; set; }
        public bool IsCriticalAction { get; set; }
    }
}