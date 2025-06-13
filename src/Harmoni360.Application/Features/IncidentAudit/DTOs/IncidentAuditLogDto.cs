namespace Harmoni360.Application.Features.IncidentAudit.DTOs;

public class IncidentAuditLogDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? ChangeDescription { get; set; }
}