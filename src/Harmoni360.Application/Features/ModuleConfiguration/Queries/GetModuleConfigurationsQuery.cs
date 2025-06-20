using Harmoni360.Application.Features.ModuleConfiguration.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.ModuleConfiguration.Queries;

public class GetModuleConfigurationsQuery : IRequest<IEnumerable<ModuleConfigurationDto>>
{
    public bool? IsEnabled { get; set; }
    public bool IncludeDependencies { get; set; } = true;
    public bool IncludeSubModules { get; set; } = true;
    public bool IncludeAuditProperties { get; set; } = false;
}

public class GetModuleConfigurationsQueryHandler : IRequestHandler<GetModuleConfigurationsQuery, IEnumerable<ModuleConfigurationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetModuleConfigurationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ModuleConfigurationDto>> Handle(GetModuleConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ModuleConfigurations.AsQueryable();

        // Filter by enabled status if specified
        if (request.IsEnabled.HasValue)
        {
            query = query.Where(mc => mc.IsEnabled == request.IsEnabled.Value);
        }

        // Include related data if requested
        if (request.IncludeDependencies)
        {
            query = query.Include(mc => mc.Dependencies)
                         .ThenInclude(d => d.DependsOnModule)
                         .Include(mc => mc.DependentModules)
                         .ThenInclude(d => d.Module);
        }

        if (request.IncludeSubModules)
        {
            query = query.Include(mc => mc.SubModules);
        }

        var moduleConfigurations = await query
            .OrderBy(mc => mc.DisplayOrder)
            .ThenBy(mc => mc.DisplayName)
            .ToListAsync(cancellationToken);

        return moduleConfigurations.Select(mc => new ModuleConfigurationDto
        {
            Id = mc.Id,
            ModuleType = mc.ModuleType,
            ModuleTypeName = mc.ModuleType.ToString(),
            IsEnabled = mc.IsEnabled,
            DisplayName = mc.DisplayName,
            Description = mc.Description,
            IconClass = mc.IconClass,
            DisplayOrder = mc.DisplayOrder,
            ParentModuleType = mc.ParentModuleType,
            ParentModuleName = mc.ParentModuleType?.ToString(),
            Settings = mc.Settings,
            CanBeDisabled = mc.CanBeDisabled(),
            DisableWarnings = mc.GetDisableWarnings(),
            SubModules = request.IncludeSubModules 
                ? mc.SubModules.Select(sm => new ModuleConfigurationDto
                {
                    Id = sm.Id,
                    ModuleType = sm.ModuleType,
                    ModuleTypeName = sm.ModuleType.ToString(),
                    IsEnabled = sm.IsEnabled,
                    DisplayName = sm.DisplayName,
                    Description = sm.Description,
                    IconClass = sm.IconClass,
                    DisplayOrder = sm.DisplayOrder,
                    ParentModuleType = sm.ParentModuleType,
                    ParentModuleName = sm.ParentModuleType?.ToString(),
                    Settings = sm.Settings,
                    CanBeDisabled = sm.CanBeDisabled(),
                    DisableWarnings = sm.GetDisableWarnings(),
                    CreatedAt = request.IncludeAuditProperties ? sm.CreatedAt : DateTime.MinValue,
                    CreatedBy = request.IncludeAuditProperties ? sm.CreatedBy : string.Empty,
                    LastModifiedAt = request.IncludeAuditProperties ? sm.LastModifiedAt : null,
                    LastModifiedBy = request.IncludeAuditProperties ? sm.LastModifiedBy : null
                }).ToList()
                : new List<ModuleConfigurationDto>(),
            Dependencies = request.IncludeDependencies 
                ? mc.Dependencies.Select(d => new ModuleDependencyDto
                {
                    Id = d.Id,
                    ModuleType = d.ModuleType,
                    ModuleTypeName = d.ModuleType.ToString(),
                    DependsOnModuleType = d.DependsOnModuleType,
                    DependsOnModuleTypeName = d.DependsOnModuleType.ToString(),
                    IsRequired = d.IsRequired,
                    Description = d.Description,
                    CreatedAt = request.IncludeAuditProperties ? d.CreatedAt : DateTime.MinValue,
                    CreatedBy = request.IncludeAuditProperties ? d.CreatedBy : string.Empty,
                    LastModifiedAt = request.IncludeAuditProperties ? d.LastModifiedAt : null,
                    LastModifiedBy = request.IncludeAuditProperties ? d.LastModifiedBy : null
                }).ToList()
                : new List<ModuleDependencyDto>(),
            DependentModules = request.IncludeDependencies 
                ? mc.DependentModules.Select(d => new ModuleDependencyDto
                {
                    Id = d.Id,
                    ModuleType = d.ModuleType,
                    ModuleTypeName = d.ModuleType.ToString(),
                    DependsOnModuleType = d.DependsOnModuleType,
                    DependsOnModuleTypeName = d.DependsOnModuleType.ToString(),
                    IsRequired = d.IsRequired,
                    Description = d.Description,
                    CreatedAt = request.IncludeAuditProperties ? d.CreatedAt : DateTime.MinValue,
                    CreatedBy = request.IncludeAuditProperties ? d.CreatedBy : string.Empty,
                    LastModifiedAt = request.IncludeAuditProperties ? d.LastModifiedAt : null,
                    LastModifiedBy = request.IncludeAuditProperties ? d.LastModifiedBy : null
                }).ToList()
                : new List<ModuleDependencyDto>(),
            CreatedAt = request.IncludeAuditProperties ? mc.CreatedAt : DateTime.MinValue,
            CreatedBy = request.IncludeAuditProperties ? mc.CreatedBy : string.Empty,
            LastModifiedAt = request.IncludeAuditProperties ? mc.LastModifiedAt : null,
            LastModifiedBy = request.IncludeAuditProperties ? mc.LastModifiedBy : null
        });
    }
}