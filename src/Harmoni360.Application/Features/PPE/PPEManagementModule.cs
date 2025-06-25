using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.PPE;

[Module(
    ModuleType.PPEManagement,
    "PPE Management",
    "Personal Protective Equipment inventory, distribution, and compliance tracking system",
    "fas fa-hard-hat",
    displayOrder: 50,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new[] { "RiskManagement", "TrainingManagement", "Reporting" }
)]
public class PPEManagementModule
{
    // This class serves as a marker for the module discovery system
    // Actual PPE management functionality is implemented in other classes within this namespace
}