using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings;

[Module(
    ModuleType.TrainingManagement,
    "Training Management",
    "Employee training programs, scheduling, completion tracking, and competency management",
    "fas fa-graduation-cap",
    displayOrder: 100,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new[] { "LicenseManagement", "PPEManagement", "Reporting" }
)]
public class TrainingManagementModule
{
    // This class serves as a marker for the module discovery system
    // Actual training management functionality is implemented in other classes within this namespace
}