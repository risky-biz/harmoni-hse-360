using AutoMapper;
using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.WorkPermitSettings;

public class WorkPermitSettingsMappingProfile : Profile
{
    public WorkPermitSettingsMappingProfile()
    {
        CreateMap<Harmoni360.Domain.Entities.WorkPermitSettings, WorkPermitSettingDto>()
            .ForMember(dest => dest.SafetyVideos, opt => opt.MapFrom(src => src.SafetyVideos));

        CreateMap<WorkPermitSafetyVideo, WorkPermitSafetyVideoDto>();
    }
}