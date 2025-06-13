using System;

namespace Harmoni360.Application.Features.Hazards.DTOs
{
    public class HazardAuditLogDto
    {
        public int Id { get; set; }
        public int HazardId { get; set; }
        
        public string Action { get; set; } = string.Empty;
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? ChangeDescription { get; set; }
        
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}