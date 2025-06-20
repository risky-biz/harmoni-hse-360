using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Reporting;

[Module(
    ModuleType.Reporting,
    "Reporting",
    "Comprehensive reporting system for analytics, compliance, and business intelligence",
    "fas fa-chart-bar",
    displayOrder: 150,
    isEnabledByDefault: true,
    canBeDisabled: true,
    requiredDependencies: new[] { "UserManagement" },
    optionalDependencies: new string[] { }
)]
public class ReportingModule
{
    // This class serves as a marker for the module discovery system
    // Actual reporting functionality is implemented in other classes within this namespace
}