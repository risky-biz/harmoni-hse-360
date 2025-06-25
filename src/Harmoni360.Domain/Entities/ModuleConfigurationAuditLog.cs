using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

/// <summary>
/// Entity for auditing module configuration changes
/// </summary>
public class ModuleConfigurationAuditLog : BaseEntity
{
    /// <summary>
    /// The module that was modified
    /// </summary>
    public ModuleType ModuleType { get; set; }
    
    /// <summary>
    /// The action that was performed (Enabled, Disabled, SettingsUpdated, etc.)
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// The previous value before the change (JSON)
    /// </summary>
    public string? OldValue { get; set; }
    
    /// <summary>
    /// The new value after the change (JSON)
    /// </summary>
    public string? NewValue { get; set; }
    
    /// <summary>
    /// ID of the user who made the change
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Timestamp when the change occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// IP address of the user who made the change
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string of the browser/client
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Additional context or reason for the change
    /// </summary>
    public string? Context { get; set; }
    
    // Navigation property
    
    /// <summary>
    /// The user who made the change
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Creates an audit log entry for enabling a module
    /// </summary>
    /// <param name="moduleType">The module that was enabled</param>
    /// <param name="userId">ID of the user who enabled it</param>
    /// <param name="ipAddress">IP address of the user</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>New audit log entry</returns>
    public static ModuleConfigurationAuditLog CreateEnabledLog(
        ModuleType moduleType, 
        int userId, 
        string? ipAddress = null, 
        string? userAgent = null)
    {
        return new ModuleConfigurationAuditLog
        {
            ModuleType = moduleType,
            Action = "Enabled",
            OldValue = "false",
            NewValue = "true",
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Context = $"Module {moduleType} was enabled"
        };
    }
    
    /// <summary>
    /// Creates an audit log entry for disabling a module
    /// </summary>
    /// <param name="moduleType">The module that was disabled</param>
    /// <param name="userId">ID of the user who disabled it</param>
    /// <param name="ipAddress">IP address of the user</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>New audit log entry</returns>
    public static ModuleConfigurationAuditLog CreateDisabledLog(
        ModuleType moduleType, 
        int userId, 
        string? ipAddress = null, 
        string? userAgent = null)
    {
        return new ModuleConfigurationAuditLog
        {
            ModuleType = moduleType,
            Action = "Disabled",
            OldValue = "true",
            NewValue = "false",
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Context = $"Module {moduleType} was disabled"
        };
    }
    
    /// <summary>
    /// Creates an audit log entry for updating module settings
    /// </summary>
    /// <param name="moduleType">The module whose settings were updated</param>
    /// <param name="oldSettings">Previous settings (JSON)</param>
    /// <param name="newSettings">New settings (JSON)</param>
    /// <param name="userId">ID of the user who made the change</param>
    /// <param name="ipAddress">IP address of the user</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>New audit log entry</returns>
    public static ModuleConfigurationAuditLog CreateSettingsUpdatedLog(
        ModuleType moduleType,
        string? oldSettings,
        string? newSettings,
        int userId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new ModuleConfigurationAuditLog
        {
            ModuleType = moduleType,
            Action = "SettingsUpdated",
            OldValue = oldSettings,
            NewValue = newSettings,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Context = $"Settings for module {moduleType} were updated"
        };
    }
    
    /// <summary>
    /// Gets a human-readable summary of the audit log entry
    /// </summary>
    /// <returns>Summary string</returns>
    public string GetSummary()
    {
        return $"{Action} - {ModuleType} at {Timestamp:yyyy-MM-dd HH:mm:ss} UTC";
    }
}