using MediatR;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class DeleteSafetyVideoCommand : IRequest<Unit>
{
    public int VideoId { get; set; }

    public DeleteSafetyVideoCommand(int videoId)
    {
        VideoId = videoId;
    }
}