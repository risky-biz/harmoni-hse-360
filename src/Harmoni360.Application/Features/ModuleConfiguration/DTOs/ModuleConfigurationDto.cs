using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.ModuleConfiguration.DTOs;

public class ModuleConfigurationDto
{
    public int Id { get; set; }
    public ModuleType ModuleType { get; set; }
    public string ModuleTypeName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public ModuleType? ParentModuleType { get; set; }
    public string? ParentModuleName { get; set; }
    public string? Settings { get; set; }

    // Navigation properties
    public List<ModuleConfigurationDto> SubModules { get; set; } = new();
    public List<ModuleDependencyDto> Dependencies { get; set; } = new();
    public List<ModuleDependencyDto> DependentModules { get; set; } = new();

    // Business logic properties
    public bool CanBeDisabled { get; set; }
    public List<string> DisableWarnings { get; set; } = new();

    // Audit properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}