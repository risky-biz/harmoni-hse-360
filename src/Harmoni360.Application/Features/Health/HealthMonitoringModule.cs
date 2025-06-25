using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Health;

[Module(
    ModuleType.HealthMonitoring,
    "Health Monitoring",
    "Employee health records, medical surveillance, and wellness program management",
    "fas fa-heartbeat",
    displayOrder: 80,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new[] { "Reporting" }
)]
public class HealthMonitoringModule
{
    // This class serves as a marker for the module discovery system
    // Actual health monitoring functionality is implemented in other classes within this namespace
}