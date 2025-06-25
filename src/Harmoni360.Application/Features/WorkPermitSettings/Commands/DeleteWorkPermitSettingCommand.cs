using MediatR;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class DeleteWorkPermitSettingCommand : IRequest<Unit>
{
    public int Id { get; set; }

    public DeleteWorkPermitSettingCommand(int id)
    {
        Id = id;
    }
}