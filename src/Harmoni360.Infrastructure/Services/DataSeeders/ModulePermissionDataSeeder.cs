using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class ModulePermissionDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ModulePermissionDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public ModulePermissionDataSeeder(ApplicationDbContext context, ILogger<ModulePermissionDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var modulePermissionCount = await _context.ModulePermissions.CountAsync();
        _logger.LogInformation("Current module permission count: {ModulePermissionCount}", modulePermissionCount);

        var reSeedModulePermissions = bool.Parse(_configuration["DataSeeding:ReSeedModulePermissions"] ?? "false");
        
        if (modulePermissionCount > 0 && !reSeedModulePermissions)
        {
            _logger.LogInformation("Module permissions already exist, skipping seeding");
            return;
        }
        
        if (reSeedModulePermissions && modulePermissionCount > 0)
        {
            _logger.LogInformation("ReSeedModulePermissions is enabled, clearing existing module permissions...");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"RoleModulePermissions\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ModulePermissions\"");
            _logger.LogInformation("Existing module permissions cleared");
        }

        _logger.LogInformation("Starting module permission seeding...");

        var modulePermissions = new List<ModulePermission>();
        var modules = Enum.GetValues<ModuleType>();
        var permissions = Enum.GetValues<PermissionType>();

        foreach (var module in modules)
        {
            foreach (var permission in permissions)
            {
                var name = $"{module}.{permission}";
                var description = $"Allows {permission.ToString().ToLower()} operations in the {module} module";

                var modulePermission = ModulePermission.Create(module, permission, name, description);
                modulePermissions.Add(modulePermission);
            }
        }

        await _context.ModulePermissions.AddRangeAsync(modulePermissions);
        _logger.LogInformation("Seeded {Count} module permissions", modulePermissions.Count);
    }
}