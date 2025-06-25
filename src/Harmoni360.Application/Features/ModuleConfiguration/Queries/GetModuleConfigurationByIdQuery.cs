using Harmoni360.Application.Features.ModuleConfiguration.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.ModuleConfiguration.Queries;

public class GetModuleConfigurationByIdQuery : IRequest<ModuleConfigurationDto?>
{
    public ModuleType ModuleType { get; set; }
    public bool IncludeDependencies { get; set; } = true;
    public bool IncludeSubModules { get; set; } = true;
    public bool IncludeAuditProperties { get; set; } = true;
}

public class GetModuleConfigurationByIdQueryHandler : IRequestHandler<GetModuleConfigurationByIdQuery, ModuleConfigurationDto?>
{
    private readonly IApplicationDbContext _context;

    public GetModuleConfigurationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModuleConfigurationDto?> Handle(GetModuleConfigurationByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ModuleConfigurations
            .Where(mc => mc.ModuleType == request.ModuleType);

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

        var moduleConfiguration = await query.FirstOrDefaultAsync(cancellationToken);

        if (moduleConfiguration == null)
            return null;

        return new ModuleConfigurationDto
        {
            Id = moduleConfiguration.Id,
            ModuleType = moduleConfiguration.ModuleType,
            ModuleTypeName = moduleConfiguration.ModuleType.ToString(),
            IsEnabled = moduleConfiguration.IsEnabled,
            DisplayName = moduleConfiguration.DisplayName,
            Description = moduleConfiguration.Description,
            IconClass = moduleConfiguration.IconClass,
            DisplayOrder = moduleConfiguration.DisplayOrder,
            ParentModuleType = moduleConfiguration.ParentModuleType,
            ParentModuleName = moduleConfiguration.ParentModuleType?.ToString(),
            Settings = moduleConfiguration.Settings,
            CanBeDisabled = moduleConfiguration.CanBeDisabled(),
            DisableWarnings = moduleConfiguration.GetDisableWarnings(),
            SubModules = request.IncludeSubModules 
                ? moduleConfiguration.SubModules.Select(sm => new ModuleConfigurationDto
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
                ? moduleConfiguration.Dependencies.Select(d => new ModuleDependencyDto
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
                ? moduleConfiguration.DependentModules.Select(d => new ModuleDependencyDto
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
            CreatedAt = request.IncludeAuditProperties ? moduleConfiguration.CreatedAt : DateTime.MinValue,
            CreatedBy = request.IncludeAuditProperties ? moduleConfiguration.CreatedBy : string.Empty,
            LastModifiedAt = request.IncludeAuditProperties ? moduleConfiguration.LastModifiedAt : null,
            LastModifiedBy = request.IncludeAuditProperties ? moduleConfiguration.LastModifiedBy : null
        };
    }
}