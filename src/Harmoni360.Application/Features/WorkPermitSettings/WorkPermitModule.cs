using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WorkPermitSettings;

/// <summary>
/// Work Permit Management Module Definition
/// </summary>
[Module(
    ModuleType.WorkPermitManagement,
    "Work Permit Management",
    "Issue and track work permits for high-risk activities",
    "fas fa-clipboard-list",
    displayOrder: 60,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { nameof(ModuleType.UserManagement), nameof(ModuleType.RiskManagement) },
    optionalDependencies: new[] { nameof(ModuleType.TrainingManagement), nameof(ModuleType.PPEManagement) }
)]
public class WorkPermitModule
{
    // Module marker class - can contain module-specific configuration
}