using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.ModuleConfiguration.DTOs;

public class ModuleDependencyDto
{
    public int Id { get; set; }
    public ModuleType ModuleType { get; set; }
    public string ModuleTypeName { get; set; } = string.Empty;
    public ModuleType DependsOnModuleType { get; set; }
    public string DependsOnModuleTypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? Description { get; set; }

    // Audit properties
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}