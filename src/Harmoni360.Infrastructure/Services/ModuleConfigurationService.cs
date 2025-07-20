using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.ModuleConfiguration.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using AutoMapper;

namespace Harmoni360.Infrastructure.Services;

public class ModuleConfigurationService : IModuleConfigurationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public ModuleConfigurationService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<ModuleConfigurationDto?> GetModuleConfigurationAsync(ModuleType moduleType)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .Include(mc => mc.SubModules)
            .Include(mc => mc.Dependencies)
            .ThenInclude(d => d.DependsOnModule)
            .Include(mc => mc.DependentModules)
            .ThenInclude(d => d.Module)
            .FirstOrDefaultAsync(mc => mc.ModuleType == moduleType);

        return moduleConfiguration == null ? null : _mapper.Map<ModuleConfigurationDto>(moduleConfiguration);
    }

    public async Task<IEnumerable<ModuleConfigurationDto>> GetAllModuleConfigurationsAsync()
    {
        var moduleConfigurations = await _context.ModuleConfigurations
            .Include(mc => mc.SubModules)
            .Include(mc => mc.Dependencies)
            .ThenInclude(d => d.DependsOnModule)
            .Include(mc => mc.DependentModules)
            .ThenInclude(d => d.Module)
            .OrderBy(mc => mc.DisplayOrder)
            .ThenBy(mc => mc.DisplayName)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ModuleConfigurationDto>>(moduleConfigurations);
    }

    public async Task<IEnumerable<ModuleConfigurationDto>> GetEnabledModuleConfigurationsAsync()
    {
        var moduleConfigurations = await _context.ModuleConfigurations
            .Where(mc => mc.IsEnabled)
            .Include(mc => mc.SubModules.Where(sm => sm.IsEnabled))
            .Include(mc => mc.Dependencies)
            .ThenInclude(d => d.DependsOnModule)
            .OrderBy(mc => mc.DisplayOrder)
            .ThenBy(mc => mc.DisplayName)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ModuleConfigurationDto>>(moduleConfigurations);
    }

    public async Task<ModuleConfigurationDashboardDto> GetModuleConfigurationDashboardAsync()
    {
        var moduleConfigurations = await _context.ModuleConfigurations
            .Include(mc => mc.Dependencies)
            .Include(mc => mc.DependentModules)
            .ToListAsync();

        var totalModules = moduleConfigurations.Count;
        var enabledModules = moduleConfigurations.Count(mc => mc.IsEnabled);
        var disabledModules = totalModules - enabledModules;
        var criticalModules = moduleConfigurations.Count(mc => !mc.CanBeDisabled());
        var modulesWithDependencies = moduleConfigurations.Count(mc => mc.Dependencies.Any() || mc.DependentModules.Any());

        var moduleStatusSummary = moduleConfigurations.Select(mc => new ModuleStatusSummaryDto
        {
            ModuleName = mc.DisplayName,
            IsEnabled = mc.IsEnabled,
            CanBeDisabled = mc.CanBeDisabled(),
            DependentModulesCount = mc.DependentModules.Count,
            DependenciesCount = mc.Dependencies.Count
        }).ToList();

        var recentActivity = await _context.ModuleConfigurationAuditLogs
            .Include(log => log.User)
            .OrderByDescending(log => log.Timestamp)
            .Take(10)
            .Select(log => new ModuleConfigurationAuditLogDto
            {
                Id = log.Id,
                ModuleType = log.ModuleType,
                ModuleTypeName = log.ModuleType.ToString(),
                Action = log.Action,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                UserId = log.UserId,
                UserName = log.User.Name,
                UserEmail = log.User.Email,
                Timestamp = log.Timestamp,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Context = log.Context
            })
            .ToListAsync();

        var warnings = GenerateConfigurationWarnings(moduleConfigurations);

        return new ModuleConfigurationDashboardDto
        {
            TotalModules = totalModules,
            EnabledModules = enabledModules,
            DisabledModules = disabledModules,
            CriticalModules = criticalModules,
            ModulesWithDependencies = modulesWithDependencies,
            ModuleStatusSummary = moduleStatusSummary,
            RecentActivity = recentActivity,
            Warnings = warnings
        };
    }

    public async Task<bool> EnableModuleAsync(ModuleType moduleType)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .Include(mc => mc.Dependencies)
            .ThenInclude(d => d.DependsOnModule)
            .FirstOrDefaultAsync(mc => mc.ModuleType == moduleType);

        if (moduleConfiguration == null || moduleConfiguration.IsEnabled)
            return false;

        var oldState = moduleConfiguration.IsEnabled;
        moduleConfiguration.Enable();

        await LogModuleStateChangeAsync(moduleType, oldState, moduleConfiguration.IsEnabled, "Module enabled via service");
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DisableModuleAsync(ModuleType moduleType)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .Include(mc => mc.SubModules)
            .Include(mc => mc.DependentModules)
            .ThenInclude(d => d.Module)
            .FirstOrDefaultAsync(mc => mc.ModuleType == moduleType);

        if (moduleConfiguration == null || !moduleConfiguration.IsEnabled)
            return false;

        if (!moduleConfiguration.ValidateDisable())
            return false;

        var oldState = moduleConfiguration.IsEnabled;
        
        try
        {
            moduleConfiguration.Disable();
            await LogModuleStateChangeAsync(moduleType, oldState, moduleConfiguration.IsEnabled, "Module disabled via service");
            await _context.SaveChangesAsync();
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public async Task<bool> CanModuleBeDisabledAsync(ModuleType moduleType)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .Include(mc => mc.DependentModules)
            .ThenInclude(d => d.Module)
            .FirstOrDefaultAsync(mc => mc.ModuleType == moduleType);

        return moduleConfiguration?.CanBeDisabled() == true && moduleConfiguration.ValidateDisable();
    }

    public async Task<IEnumerable<string>> GetModuleDisableWarningsAsync(ModuleType moduleType)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .Include(mc => mc.SubModules)
            .Include(mc => mc.DependentModules)
            .ThenInclude(d => d.Module)
            .FirstOrDefaultAsync(mc => mc.ModuleType == moduleType);

        return moduleConfiguration?.GetDisableWarnings() ?? new List<string>();
    }

    public async Task<IEnumerable<ModuleDependencyDto>> GetModuleDependenciesAsync(ModuleType moduleType)
    {
        var dependencies = await _context.ModuleDependencies
            .Where(md => md.ModuleType == moduleType)
            .Include(md => md.DependsOnModule)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ModuleDependencyDto>>(dependencies);
    }

    public async Task<IEnumerable<ModuleDependencyDto>> GetDependentModulesAsync(ModuleType moduleType)
    {
        var dependentModules = await _context.ModuleDependencies
            .Where(md => md.DependsOnModuleType == moduleType)
            .Include(md => md.Module)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ModuleDependencyDto>>(dependentModules);
    }

    public async Task<bool> ValidateModuleDependenciesAsync(ModuleType moduleType)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .Include(mc => mc.Dependencies)
            .ThenInclude(d => d.DependsOnModule)
            .FirstOrDefaultAsync(mc => mc.ModuleType == moduleType);

        if (moduleConfiguration == null || !moduleConfiguration.IsEnabled)
            return true;

        // Check if all required dependencies are enabled
        return !moduleConfiguration.Dependencies
            .Where(d => d.IsRequired)
            .Any(d => !d.DependsOnModule.IsEnabled);
    }

    public async Task<bool> UpdateModuleSettingsAsync(ModuleType moduleType, string? settings)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .FirstOrDefaultAsync(mc => mc.ModuleType == moduleType);

        if (moduleConfiguration == null)
            return false;

        var oldSettings = moduleConfiguration.Settings;
        moduleConfiguration.Settings = settings;

        await LogModuleSettingsChangeAsync(moduleType, oldSettings, settings, "Settings updated via service");
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<string?> GetModuleSettingsAsync(ModuleType moduleType)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .FirstOrDefaultAsync(mc => mc.ModuleType == moduleType);

        return moduleConfiguration?.Settings;
    }

    public Task LogModuleStateChangeAsync(ModuleType moduleType, bool oldState, bool newState, string? context = null)
    {
        var auditLog = newState 
            ? ModuleConfigurationAuditLog.CreateEnabledLog(moduleType, _currentUserService.UserId, GetClientIpAddress(), GetUserAgent())
            : ModuleConfigurationAuditLog.CreateDisabledLog(moduleType, _currentUserService.UserId, GetClientIpAddress(), GetUserAgent());

        if (!string.IsNullOrEmpty(context))
            auditLog.Context = context;

        _context.ModuleConfigurationAuditLogs.Add(auditLog);
        // Note: SaveChanges should be called by the calling method
        return Task.CompletedTask;
    }

    public Task LogModuleSettingsChangeAsync(ModuleType moduleType, string? oldSettings, string? newSettings, string? context = null)
    {
        var auditLog = ModuleConfigurationAuditLog.CreateSettingsUpdatedLog(
            moduleType, oldSettings, newSettings, _currentUserService.UserId, GetClientIpAddress(), GetUserAgent());

        if (!string.IsNullOrEmpty(context))
            auditLog.Context = context;

        _context.ModuleConfigurationAuditLogs.Add(auditLog);
        // Note: SaveChanges should be called by the calling method
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ModuleConfigurationAuditLogDto>> GetModuleAuditTrailAsync(ModuleType moduleType)
    {
        var auditLogs = await _context.ModuleConfigurationAuditLogs
            .Where(log => log.ModuleType == moduleType)
            .Include(log => log.User)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ModuleConfigurationAuditLogDto>>(auditLogs);
    }

    public async Task<IEnumerable<ModuleConfigurationAuditLogDto>> GetRecentModuleActivityAsync(int count = 10)
    {
        var auditLogs = await _context.ModuleConfigurationAuditLogs
            .Include(log => log.User)
            .OrderByDescending(log => log.Timestamp)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ModuleConfigurationAuditLogDto>>(auditLogs);
    }

    public async Task<IEnumerable<ModuleConfigurationDto>> GetModuleHierarchyAsync()
    {
        var rootModules = await _context.ModuleConfigurations
            .Where(mc => mc.ParentModuleType == null)
            .Include(mc => mc.SubModules)
            .ThenInclude(sm => sm.SubModules)
            .OrderBy(mc => mc.DisplayOrder)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ModuleConfigurationDto>>(rootModules);
    }

    public async Task<IEnumerable<ModuleConfigurationDto>> GetChildModulesAsync(ModuleType parentModuleType)
    {
        var childModules = await _context.ModuleConfigurations
            .Where(mc => mc.ParentModuleType == parentModuleType)
            .OrderBy(mc => mc.DisplayOrder)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ModuleConfigurationDto>>(childModules);
    }

    private string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
            ipAddress = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
            ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        return ipAddress;
    }

    private string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
    }

    private List<ModuleWarningDto> GenerateConfigurationWarnings(List<Domain.Entities.ModuleConfiguration> moduleConfigurations)
    {
        var warnings = new List<ModuleWarningDto>();

        foreach (var module in moduleConfigurations)
        {
            // Check for disabled modules with active dependents
            if (!module.IsEnabled && module.DependentModules.Any(dm => dm.IsRequired && dm.Module.IsEnabled))
            {
                warnings.Add(new ModuleWarningDto
                {
                    ModuleName = module.DisplayName,
                    WarningType = "DependencyViolation",
                    Message = $"Disabled module {module.DisplayName} has active dependent modules",
                    Severity = "High"
                });
            }

            // Check for enabled modules with disabled required dependencies
            if (module.IsEnabled && module.Dependencies.Any(d => d.IsRequired && !d.DependsOnModule.IsEnabled))
            {
                warnings.Add(new ModuleWarningDto
                {
                    ModuleName = module.DisplayName,
                    WarningType = "MissingDependency",
                    Message = $"Enabled module {module.DisplayName} has disabled required dependencies",
                    Severity = "High"
                });
            }

            // Check for modules with many dependencies (complexity warning)
            if (module.Dependencies.Count > 5)
            {
                warnings.Add(new ModuleWarningDto
                {
                    ModuleName = module.DisplayName,
                    WarningType = "HighComplexity",
                    Message = $"Module {module.DisplayName} has many dependencies ({module.Dependencies.Count})",
                    Severity = "Medium"
                });
            }
        }

        return warnings;
    }
}