using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Dashboard;

/// <summary>
/// Dashboard Module Definition - Core system module
/// </summary>
[Module(
    ModuleType.Dashboard,
    "Dashboard",
    "Main dashboard and overview",
    "fas fa-tachometer-alt",
    displayOrder: 0,
    isEnabledByDefault: true,
    canBeDisabled: false // Core system module cannot be disabled
)]
public class DashboardModule
{
    // Core system module - always enabled
}