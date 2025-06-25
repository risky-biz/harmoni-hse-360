using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.UserManagement;

/// <summary>
/// User Management Module Definition - Core system module
/// </summary>
[Module(
    ModuleType.UserManagement,
    "User Management",
    "User accounts, roles, and permissions",
    "fas fa-users",
    displayOrder: 1,
    isEnabledByDefault: true,
    canBeDisabled: false // Core system module cannot be disabled
)]
public class UserManagementModule
{
    // Core system module - always enabled
}