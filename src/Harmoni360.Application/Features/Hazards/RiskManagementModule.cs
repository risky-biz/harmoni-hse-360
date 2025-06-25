using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Hazards;

[Module(
    ModuleType.RiskManagement,
    "Risk Management",
    "Hazard identification, risk assessment, and mitigation strategies for workplace safety",
    "fas fa-exclamation-triangle",
    displayOrder: 20,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new[] { "IncidentManagement", "Reporting" }
)]
public class RiskManagementModule
{
    // This class serves as a marker for the module discovery system
    // Actual risk management functionality is implemented in other classes within this namespace
}