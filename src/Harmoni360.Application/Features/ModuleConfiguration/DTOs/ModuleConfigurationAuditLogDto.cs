using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.ModuleConfiguration.DTOs;

public class ModuleConfigurationAuditLogDto
{
    public int Id { get; set; }
    public ModuleType ModuleType { get; set; }
    public string ModuleTypeName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Context { get; set; }
}