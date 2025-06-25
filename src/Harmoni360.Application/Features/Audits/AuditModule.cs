using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Audits;

[Module(
    ModuleType.AuditManagement,
    "Audit Management",
    "Comprehensive audit planning, execution, and reporting system for organizational compliance and risk management",
    "fas fa-clipboard-check",
    displayOrder: 40,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new[] { "InspectionManagement", "Reporting" }
)]
public class AuditModule
{
    // This class serves as a marker for the module discovery system
    // Actual audit functionality is implemented in other classes within this namespace
}