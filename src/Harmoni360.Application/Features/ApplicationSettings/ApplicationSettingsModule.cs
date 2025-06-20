using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.ApplicationSettings;

[Module(
    ModuleType.ApplicationSettings,
    "Application Settings",
    "System configuration, company settings, and application customization options",
    "fas fa-cogs",
    displayOrder: 200,
    isEnabledByDefault: true,
    canBeDisabled: false,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new string[] { }
)]
public class ApplicationSettingsModule
{
    // This class serves as a marker for the module discovery system
    // Actual application settings functionality is implemented in other classes within this namespace
}