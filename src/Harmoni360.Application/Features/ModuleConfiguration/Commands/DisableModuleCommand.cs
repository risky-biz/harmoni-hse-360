using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.ModuleConfiguration.Commands;

public class DisableModuleCommand : IRequest<bool>
{
    public ModuleType ModuleType { get; set; }
    public string? Context { get; set; }
    public bool ForceDisable { get; set; } = false;
}