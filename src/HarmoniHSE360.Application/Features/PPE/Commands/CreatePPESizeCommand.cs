using HarmoniHSE360.Application.Features.PPE.DTOs;
using MediatR;

namespace HarmoniHSE360.Application.Features.PPE.Commands;

public class CreatePPESizeCommand : IRequest<PPESizeDto>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}