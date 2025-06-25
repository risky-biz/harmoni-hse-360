using Harmoni360.Application.Features.ModuleConfiguration.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.ModuleConfiguration.Queries;

public class GetModuleConfigurationDashboardQuery : IRequest<ModuleConfigurationDashboardDto>
{
    public int RecentActivityCount { get; set; } = 10;
}

public class GetModuleConfigurationDashboardQueryHandler : IRequestHandler<GetModuleConfigurationDashboardQuery, ModuleConfigurationDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetModuleConfigurationDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModuleConfigurationDashboardDto> Handle(GetModuleConfigurationDashboardQuery request, CancellationToken cancellationToken)
    {
        // Get module configurations with dependencies
        var moduleConfigurations = await _context.ModuleConfigurations
            .Include(mc => mc.Dependencies)
            .Include(mc => mc.DependentModules)
            .ToListAsync(cancellationToken);

        // Calculate summary statistics
        var totalModules = moduleConfigurations.Count;
        var enabledModules = moduleConfigurations.Where(mc => mc.IsEnabled).Count();
        var disabledModules = totalModules - enabledModules;
        var criticalModules = moduleConfigurations.Where(mc => !mc.CanBeDisabled()).Count();
        var modulesWithDependencies = moduleConfigurations.Where(mc => mc.Dependencies.Any() || mc.DependentModules.Any()).Count();

        // Create module status summary
        var moduleStatusSummary = moduleConfigurations.Select(mc => new ModuleStatusSummaryDto
        {
            ModuleName = mc.DisplayName,
            IsEnabled = mc.IsEnabled,
            CanBeDisabled = mc.CanBeDisabled(),
            DependentModulesCount = mc.DependentModules.Count,
            DependenciesCount = mc.Dependencies.Count
        }).ToList();

        // Get recent activity
        var recentActivity = await _context.ModuleConfigurationAuditLogs
            .Include(log => log.User)
            .OrderByDescending(log => log.Timestamp)
            .Take(request.RecentActivityCount)
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
            .ToListAsync(cancellationToken);

        // Generate warnings
        var warnings = new List<ModuleWarningDto>();
        
        foreach (var module in moduleConfigurations)
        {
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
        }

        return new ModuleConfigurationDashboardDto
        {
            TotalModules = totalModules,
            EnabledModules = enabledModules,
            DisabledModules = disabledModules,
            CriticalModules = criticalModules,
            ModulesWithDependencies = modulesWithDependencies,
            ModuleStatusSummary = moduleStatusSummary,
            RecentActivity = recentActivity.ToList(),
            Warnings = warnings
        };
    }
}