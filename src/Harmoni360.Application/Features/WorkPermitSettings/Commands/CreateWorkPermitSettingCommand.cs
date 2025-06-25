using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class CreateWorkPermitSettingCommand : IRequest<WorkPermitSettingDto>
{
    public bool RequireSafetyInduction { get; set; } = true;
    public bool EnableFormValidation { get; set; } = true;
    public bool AllowAttachments { get; set; } = true;
    public int MaxAttachmentSizeMB { get; set; } = 10;
    public string? FormInstructions { get; set; }
    public bool IsActive { get; set; } = true;
}