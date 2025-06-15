using Microsoft.AspNetCore.Authorization;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.Constants;
using Harmoni360.Domain.Authorization;

namespace Harmoni360.Web.Authorization;

/// <summary>
/// Extension methods for configuring the authorization system with module-based permissions
/// </summary>
public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// Adds the module-based authorization system to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddModuleBasedAuthorization(this IServiceCollection services)
    {
        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, ModulePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, ModuleAccessHandler>();
        services.AddScoped<IAuthorizationHandler, CanPerformActionHandler>();

        // Configure authorization with module-based policies
        services.AddAuthorization(options =>
        {
            // Add module access policies
            AddModuleAccessPolicies(options);

            // Add module permission policies
            AddModulePermissionPolicies(options);

            // Add role-based policies
            AddRoleBasedPolicies(options);

            // Add convenience policies
            AddConveniencePolicies(options);
        });

        return services;
    }

    /// <summary>
    /// Adds policies for module access (any permission level)
    /// </summary>
    private static void AddModuleAccessPolicies(AuthorizationOptions options)
    {
        var modules = Enum.GetValues<ModuleType>();

        foreach (var module in modules)
        {
            var policyName = $"ModuleAccess.{module}";
            options.AddPolicy(policyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.Requirements.Add(new ModuleAccessRequirement(module));
            });
        }
    }

    /// <summary>
    /// Adds policies for specific module permissions
    /// </summary>
    private static void AddModulePermissionPolicies(AuthorizationOptions options)
    {
        var modules = Enum.GetValues<ModuleType>();
        var permissions = Enum.GetValues<PermissionType>();

        foreach (var module in modules)
        {
            foreach (var permission in permissions)
            {
                var policyName = $"ModulePermission.{module}.{permission}";
                options.AddPolicy(policyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new ModulePermissionRequirement(module, permission));
                });
            }
        }
    }

    /// <summary>
    /// Adds role-based policies
    /// </summary>
    private static void AddRoleBasedPolicies(AuthorizationOptions options)
    {
        // System administration policy (SuperAdmin, Developer only)
        options.AddPolicy("RequireSystemAdmin", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(AuthorizationConstants.Roles.SuperAdmin, AuthorizationConstants.Roles.Developer);
        });

        // Functional administration policy (SuperAdmin, Developer, Admin)
        options.AddPolicy("RequireFunctionalAdmin", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(AuthorizationConstants.Roles.SuperAdmin, AuthorizationConstants.Roles.Developer, AuthorizationConstants.Roles.Admin);
        });

        // Manager-level policy (Admin and all manager roles)
        options.AddPolicy("RequireManagerRole", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(
                AuthorizationConstants.Roles.SuperAdmin,
                AuthorizationConstants.Roles.Developer,
                AuthorizationConstants.Roles.Admin,
                AuthorizationConstants.Roles.IncidentManager,
                AuthorizationConstants.Roles.RiskManager,
                AuthorizationConstants.Roles.PPEManager,
                AuthorizationConstants.Roles.HealthMonitor
            );
        });

        // Basic user policy (any authenticated user)
        options.AddPolicy("RequireAuthenticatedUser", policy =>
        {
            policy.RequireAuthenticatedUser();
        });
    }

    /// <summary>
    /// Adds convenience policies for common scenarios
    /// </summary>
    private static void AddConveniencePolicies(AuthorizationOptions options)
    {
        // Can create records policy
        options.AddPolicy(AuthorizationConstants.Policies.CanCreateRecords, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new CanPerformActionRequirement(PermissionType.Create));
        });

        // Can update records policy
        options.AddPolicy(AuthorizationConstants.Policies.CanUpdateRecords, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new CanPerformActionRequirement(PermissionType.Update));
        });

        // Can delete records policy
        options.AddPolicy(AuthorizationConstants.Policies.CanDeleteRecords, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new CanPerformActionRequirement(PermissionType.Delete));
        });

        // Can export data policy
        options.AddPolicy(AuthorizationConstants.Policies.CanExportData, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new CanPerformActionRequirement(PermissionType.Export));
        });

        // Can configure system policy
        options.AddPolicy(AuthorizationConstants.Policies.CanConfigureSystem, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(AuthorizationConstants.Roles.SuperAdmin, AuthorizationConstants.Roles.Developer);
        });
    }
}

/// <summary>
/// Authorization requirement for module access (any permission)
/// </summary>
public class ModuleAccessRequirement : IAuthorizationRequirement
{
    public ModuleType Module { get; }

    public ModuleAccessRequirement(ModuleType module)
    {
        Module = module;
    }

    public override string ToString()
    {
        return $"ModuleAccess: {Module}";
    }
}

/// <summary>
/// Authorization requirement for checking if user can perform a specific action
/// </summary>
public class CanPerformActionRequirement : IAuthorizationRequirement
{
    public PermissionType Action { get; }

    public CanPerformActionRequirement(PermissionType action)
    {
        Action = action;
    }

    public override string ToString()
    {
        return $"CanPerform: {Action}";
    }
}

/// <summary>
/// Authorization handler for module access requirements
/// </summary>
public class ModuleAccessHandler : AuthorizationHandler<ModuleAccessRequirement>
{
    private readonly ILogger<ModuleAccessHandler> _logger;

    public ModuleAccessHandler(ILogger<ModuleAccessHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ModuleAccessRequirement requirement)
    {
        // Get user identity information
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";

        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            _logger.LogWarning("Module access failed for {UserName}: User not authenticated for module {Module}", 
                userName, requirement.Module);
            context.Fail();
            return Task.CompletedTask;
        }

        // Get user roles from claims
        var userRoles = context.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        
        if (!userRoles.Any())
        {
            _logger.LogWarning("Module access failed for {UserName} ({UserId}): No roles found for module {Module}", 
                userName, userId, requirement.Module);
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if any of the user's roles has access to the module
        bool hasAccess = false;

        foreach (var roleString in userRoles)
        {
            if (!Enum.TryParse<RoleType>(roleString, out var roleType))
            {
                _logger.LogWarning("Module access check for {UserName} ({UserId}): Invalid role '{Role}' found in claims", 
                    userName, userId, roleString);
                continue;
            }

            // Check if this role has any access to the module
            var accessibleModules = ModulePermissionMap.GetAccessibleModules(roleType);
            if (accessibleModules.Contains(requirement.Module))
            {
                hasAccess = true;
                _logger.LogDebug("Module access succeeded for {UserName} ({UserId}): Role '{Role}' has access to module {Module}", 
                    userName, userId, roleString, requirement.Module);
                break;
            }
        }

        if (hasAccess)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Module access failed for {UserName} ({UserId}): None of roles [{Roles}] have access to module {Module}", 
                userName, userId, string.Join(", ", userRoles), requirement.Module);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization handler for action permission requirements
/// </summary>
public class CanPerformActionHandler : AuthorizationHandler<CanPerformActionRequirement>
{
    private readonly ILogger<CanPerformActionHandler> _logger;

    public CanPerformActionHandler(ILogger<CanPerformActionHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanPerformActionRequirement requirement)
    {
        // This handler checks if the user has the specified action permission in ANY module they have access to
        // For more specific checks, use ModulePermissionRequirement instead

        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";

        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            _logger.LogWarning("Action permission failed for {UserName}: User not authenticated for action {Action}", 
                userName, requirement.Action);
            context.Fail();
            return Task.CompletedTask;
        }

        var userRoles = context.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        
        if (!userRoles.Any())
        {
            _logger.LogWarning("Action permission failed for {UserName} ({UserId}): No roles found for action {Action}", 
                userName, userId, requirement.Action);
            context.Fail();
            return Task.CompletedTask;
        }

        bool hasPermission = false;

        foreach (var roleString in userRoles)
        {
            if (!Enum.TryParse<RoleType>(roleString, out var roleType))
                continue;

            // Check if this role has the required permission in any module
            var accessibleModules = ModulePermissionMap.GetAccessibleModules(roleType);
            foreach (var module in accessibleModules)
            {
                if (ModulePermissionMap.HasPermission(roleType, module, requirement.Action))
                {
                    hasPermission = true;
                    _logger.LogDebug("Action permission succeeded for {UserName} ({UserId}): Role '{Role}' can perform {Action} in module {Module}", 
                        userName, userId, roleString, requirement.Action, module);
                    break;
                }
            }

            if (hasPermission) break;
        }

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Action permission failed for {UserName} ({UserId}): None of roles [{Roles}] can perform action {Action}", 
                userName, userId, string.Join(", ", userRoles), requirement.Action);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}