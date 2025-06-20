namespace Harmoni360.Application.Features.ModuleConfiguration.DTOs;

public class ModuleConfigurationDashboardDto
{
    public int TotalModules { get; set; }
    public int EnabledModules { get; set; }
    public int DisabledModules { get; set; }
    public int CriticalModules { get; set; }
    public int ModulesWithDependencies { get; set; }
    
    // Module status breakdown
    public List<ModuleStatusSummaryDto> ModuleStatusSummary { get; set; } = new();
    
    // Recent activity
    public List<ModuleConfigurationAuditLogDto> RecentActivity { get; set; } = new();
    
    // Warnings and issues
    public List<ModuleWarningDto> Warnings { get; set; } = new();
}

public class ModuleStatusSummaryDto
{
    public string ModuleName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool CanBeDisabled { get; set; }
    public int DependentModulesCount { get; set; }
    public int DependenciesCount { get; set; }
}

public class ModuleWarningDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string WarningType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}