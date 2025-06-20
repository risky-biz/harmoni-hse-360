using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.IncidentManagement;

/// <summary>
/// Incident Management Module Definition
/// </summary>
[Module(
    ModuleType.IncidentManagement,
    "Incident Management",
    "Report, track, and manage workplace incidents",
    "fas fa-exclamation-triangle",
    displayOrder: 10,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { nameof(ModuleType.UserManagement) },
    optionalDependencies: new[] { nameof(ModuleType.RiskManagement), nameof(ModuleType.Reporting) }
)]
public class IncidentModule
{
    // This class serves as a marker for the module and could contain
    // module-specific configuration, initialization logic, or feature flags
}