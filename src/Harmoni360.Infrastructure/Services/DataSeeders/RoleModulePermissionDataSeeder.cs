using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Authorization;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class RoleModulePermissionDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RoleModulePermissionDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public RoleModulePermissionDataSeeder(ApplicationDbContext context, ILogger<RoleModulePermissionDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var roleModulePermissionCount = await _context.RoleModulePermissions.CountAsync();
        var forceReseed = Environment.GetEnvironmentVariable("HARMONI_FORCE_RESEED") == "true";
        _logger.LogInformation("Current role-module permission count: {RoleModulePermissionCount}, ForceReseed: {ForceReseed}", 
            roleModulePermissionCount, forceReseed);
        
        if (!forceReseed && roleModulePermissionCount > 0)
        {
            _logger.LogInformation("Role-module permissions already exist, skipping seeding");
            return;
        }
        
        if (forceReseed && roleModulePermissionCount > 0)
        {
            _logger.LogInformation("ForceReseed is enabled, clearing existing role-module permissions...");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"RoleModulePermissions\"");
            _logger.LogInformation("Existing role-module permissions cleared");
        }

        _logger.LogInformation("Starting role-module permission seeding...");

        // Get all roles and module permissions from database
        var roles = await _context.Roles.ToListAsync();
        var modulePermissions = await _context.ModulePermissions.ToListAsync();

        var roleModulePermissions = new List<RoleModulePermission>();

        // Get the role-module permission mapping from the authorization system
        var permissionMap = ModulePermissionMap.GetRoleModulePermissions();

        foreach (var role in roles)
        {
            // Parse role type from name
            if (!Enum.TryParse<RoleType>(role.Name, out var roleType))
            {
                _logger.LogWarning("Could not parse role type for role: {RoleName}", role.Name);
                continue;
            }

            // Check if this role type exists in our permission map
            if (!permissionMap.ContainsKey(roleType))
            {
                _logger.LogWarning("No permission mapping found for role type: {RoleType}", roleType);
                continue;
            }

            var rolePermissions = permissionMap[roleType];

            foreach (var moduleMapping in rolePermissions)
            {
                var module = moduleMapping.Key;
                var permissions = moduleMapping.Value;

                foreach (var permission in permissions)
                {
                    // Find the corresponding module permission in the database
                    var modulePermission = modulePermissions
                        .FirstOrDefault(mp => mp.Module == module && mp.Permission == permission);

                    if (modulePermission == null)
                    {
                        _logger.LogWarning("Module permission not found: {Module}.{Permission}", module, permission);
                        continue;
                    }

                    // Create the role-module-permission association
                    var roleModulePermission = RoleModulePermission.Create(
                        role.Id, 
                        modulePermission.Id, 
                        grantedByUserId: null, 
                        grantReason: "System seeded during initial setup"
                    );

                    roleModulePermissions.Add(roleModulePermission);
                }
            }
        }

        await _context.RoleModulePermissions.AddRangeAsync(roleModulePermissions);
        _logger.LogInformation("Seeded {Count} role-module permission associations", roleModulePermissions.Count);
    }
}