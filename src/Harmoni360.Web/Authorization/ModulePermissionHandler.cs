using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Authorization;
using Harmoni360.Domain.Constants;

namespace Harmoni360.Web.Authorization;

/// <summary>
/// Authorization handler for module-based permissions.
/// Evaluates whether a user has the required permission for a specific module.
/// </summary>
public class ModulePermissionHandler : AuthorizationHandler<ModulePermissionRequirement>
{
    private readonly ILogger<ModulePermissionHandler> _logger;

    public ModulePermissionHandler(ILogger<ModulePermissionHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ModulePermissionRequirement requirement)
    {
        // Get user identity information
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            _logger.LogWarning("Authorization failed for {UserName}: User not authenticated for {Requirement}", 
                userName, requirement);
            context.Fail();
            return Task.CompletedTask;
        }

        // Get user roles from claims
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        if (!userRoles.Any())
        {
            _logger.LogWarning("Authorization failed for {UserName} ({UserId}): No roles found for {Requirement}", 
                userName, userId, requirement);
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if any of the user's roles has the required permission
        bool hasPermission = false;
        var checkedRoles = new List<string>();

        foreach (var roleString in userRoles)
        {
            checkedRoles.Add(roleString);
            
            // Try to parse the role string to RoleType enum
            if (!Enum.TryParse<RoleType>(roleString, out var roleType))
            {
                _logger.LogWarning("Authorization check for {UserName} ({UserId}): Invalid role '{Role}' found in claims", 
                    userName, userId, roleString);
                continue;
            }

            // Check if this role has the required permission for the module
            if (ModulePermissionMap.HasPermission(roleType, requirement.Module, requirement.Permission))
            {
                hasPermission = true;
                _logger.LogDebug("Authorization succeeded for {UserName} ({UserId}): Role '{Role}' has {Requirement}", 
                    userName, userId, roleString, requirement);
                break;
            }
        }

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Authorization failed for {UserName} ({UserId}): None of roles [{Roles}] have {Requirement}", 
                userName, userId, string.Join(", ", checkedRoles), requirement);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization handler specifically for role-based requirements.
/// Checks if the user has any of the specified roles.
/// </summary>
public class RoleRequirementHandler : AuthorizationHandler<RolesAuthorizationRequirement>
{
    private readonly ILogger<RoleRequirementHandler> _logger;

    public RoleRequirementHandler(ILogger<RoleRequirementHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RolesAuthorizationRequirement requirement)
    {
        // Get user identity information
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            _logger.LogWarning("Role authorization failed for {UserName}: User not authenticated for roles [{RequiredRoles}]", 
                userName, string.Join(", ", requirement.AllowedRoles));
            context.Fail();
            return Task.CompletedTask;
        }

        // Get user roles from claims
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        if (!userRoles.Any())
        {
            _logger.LogWarning("Role authorization failed for {UserName} ({UserId}): No roles found, required roles [{RequiredRoles}]", 
                userName, userId, string.Join(", ", requirement.AllowedRoles));
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if user has any of the required roles
        bool hasRequiredRole = userRoles.Any(userRole => requirement.AllowedRoles.Contains(userRole));

        if (hasRequiredRole)
        {
            var matchedRoles = userRoles.Where(userRole => requirement.AllowedRoles.Contains(userRole));
            _logger.LogDebug("Role authorization succeeded for {UserName} ({UserId}): Has roles [{MatchedRoles}] from required [{RequiredRoles}]", 
                userName, userId, string.Join(", ", matchedRoles), string.Join(", ", requirement.AllowedRoles));
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Role authorization failed for {UserName} ({UserId}): Has roles [{UserRoles}] but requires [{RequiredRoles}]", 
                userName, userId, string.Join(", ", userRoles), string.Join(", ", requirement.AllowedRoles));
            context.Fail();
        }

        return Task.CompletedTask;
    }
}