using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermitSettings.Queries;

public class GetSafetyVideoByIdQuery : IRequest<WorkPermitSafetyVideoDto?>
{
    public int VideoId { get; set; }

    public GetSafetyVideoByIdQuery(int videoId)
    {
        VideoId = videoId;
    }
}