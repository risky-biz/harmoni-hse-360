using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WasteReports;

[Module(
    ModuleType.WasteManagement,
    "Waste Management",
    "Environmental waste tracking, disposal management, and regulatory compliance reporting",
    "fas fa-recycle",
    displayOrder: 110,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new[] { "RiskManagement", "Reporting" }
)]
public class WasteManagementModule
{
    // This class serves as a marker for the module discovery system
    // Actual waste management functionality is implemented in other classes within this namespace
}