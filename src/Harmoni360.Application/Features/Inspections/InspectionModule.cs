using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Inspections;

/// <summary>
/// Inspection Management Module Definition
/// </summary>
[Module(
    ModuleType.InspectionManagement,
    "Inspection Management",
    "Schedule and conduct safety inspections",
    "fas fa-search",
    displayOrder: 30,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { nameof(ModuleType.UserManagement) },
    optionalDependencies: new[] { nameof(ModuleType.AuditManagement), nameof(ModuleType.Reporting) }
)]
public class InspectionModule
{
    // Module configuration and features
}