using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses;

[Module(
    ModuleType.LicenseManagement,
    "License Management",
    "Professional licenses, certifications, and compliance tracking with automated renewal reminders",
    "fas fa-certificate",
    displayOrder: 70,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new[] { "TrainingManagement", "Reporting" }
)]
public class LicenseManagementModule
{
    // This class serves as a marker for the module discovery system
    // Actual license management functionality is implemented in other classes within this namespace
}