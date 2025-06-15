using System;
using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities
{
    public class HazardAuditLog : BaseEntity
    {
        public int HazardId { get; set; }
        public virtual Hazard? Hazard { get; set; }
        
        public string Action { get; set; } = string.Empty;
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? ChangeDescription { get; set; }
        
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        
        // Additional metadata
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}