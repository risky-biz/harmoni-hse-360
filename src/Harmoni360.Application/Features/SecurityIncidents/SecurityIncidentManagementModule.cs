using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.SecurityIncidents;

[Module(
    ModuleType.SecurityIncidentManagement,
    "Security Incident Management",
    "Physical and information security incident reporting, investigation, and response management",
    "fas fa-shield-alt",
    displayOrder: 90,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new[] { "IncidentManagement", "Reporting" }
)]
public class SecurityIncidentManagementModule
{
    // This class serves as a marker for the module discovery system
    // Actual security incident management functionality is implemented in other classes within this namespace
}