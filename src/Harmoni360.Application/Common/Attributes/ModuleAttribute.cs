using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Common.Attributes;

/// <summary>
/// Attribute to mark and configure HSSE modules for automatic discovery
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ModuleAttribute : Attribute
{
    public ModuleType ModuleType { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string IconClass { get; }
    public int DisplayOrder { get; }
    public bool IsEnabledByDefault { get; }
    public bool CanBeDisabled { get; }
    public string[] RequiredDependencies { get; }
    public string[] OptionalDependencies { get; }

    public ModuleAttribute(
        ModuleType moduleType,
        string displayName,
        string description,
        string iconClass = "fas fa-cog",
        int displayOrder = 100,
        bool isEnabledByDefault = true,
        bool canBeDisabled = true,
        string[]? requiredDependencies = null,
        string[]? optionalDependencies = null)
    {
        ModuleType = moduleType;
        DisplayName = displayName;
        Description = description;
        IconClass = iconClass;
        DisplayOrder = displayOrder;
        IsEnabledByDefault = isEnabledByDefault;
        CanBeDisabled = canBeDisabled;
        RequiredDependencies = requiredDependencies ?? Array.Empty<string>();
        OptionalDependencies = optionalDependencies ?? Array.Empty<string>();
    }
}