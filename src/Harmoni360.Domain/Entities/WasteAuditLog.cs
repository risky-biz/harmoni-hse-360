using System;
using Harmoni360.Domain.Common;
using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Domain.Entities
{
    public class WasteAuditLog : BaseEntity
    {
        public int WasteReportId { get; set; }
        public virtual WasteReport? WasteReport { get; set; }
        
        public string Action { get; set; } = string.Empty;
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? ChangeDescription { get; set; }
        
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        
        // Additional metadata for compliance
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ComplianceNotes { get; set; }
        public bool IsCriticalAction { get; set; }
    }
}