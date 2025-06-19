using MediatR;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class UpdateWorkPermitSettingCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public bool RequireSafetyInduction { get; set; }
    public bool EnableFormValidation { get; set; }
    public bool AllowAttachments { get; set; }
    public int MaxAttachmentSizeMB { get; set; }
    public string? FormInstructions { get; set; }
    public bool IsActive { get; set; }
}