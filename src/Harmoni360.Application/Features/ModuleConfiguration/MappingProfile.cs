using AutoMapper;
using Harmoni360.Application.Features.ModuleConfiguration.DTOs;

namespace Harmoni360.Application.Features.ModuleConfiguration;

public class ModuleConfigurationMappingProfile : Profile
{
    public ModuleConfigurationMappingProfile()
    {
        CreateMap<Harmoni360.Domain.Entities.ModuleConfiguration, ModuleConfigurationDto>()
            .ForMember(dest => dest.ModuleTypeName, opt => opt.MapFrom(src => src.ModuleType.ToString()))
            .ForMember(dest => dest.ParentModuleName, opt => opt.MapFrom(src => src.ParentModuleType.ToString()))
            .ForMember(dest => dest.CanBeDisabled, opt => opt.MapFrom(src => src.CanBeDisabled()))
            .ForMember(dest => dest.DisableWarnings, opt => opt.MapFrom(src => src.GetDisableWarnings()))
            .ForMember(dest => dest.SubModules, opt => opt.MapFrom(src => src.SubModules))
            .ForMember(dest => dest.Dependencies, opt => opt.MapFrom(src => src.Dependencies))
            .ForMember(dest => dest.DependentModules, opt => opt.MapFrom(src => src.DependentModules));

        CreateMap<Harmoni360.Domain.Entities.ModuleDependency, ModuleDependencyDto>()
            .ForMember(dest => dest.ModuleTypeName, opt => opt.MapFrom(src => src.ModuleType.ToString()))
            .ForMember(dest => dest.DependsOnModuleTypeName, opt => opt.MapFrom(src => src.DependsOnModuleType.ToString()));

        CreateMap<Harmoni360.Domain.Entities.ModuleConfigurationAuditLog, ModuleConfigurationAuditLogDto>()
            .ForMember(dest => dest.ModuleTypeName, opt => opt.MapFrom(src => src.ModuleType.ToString()))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
    }
}