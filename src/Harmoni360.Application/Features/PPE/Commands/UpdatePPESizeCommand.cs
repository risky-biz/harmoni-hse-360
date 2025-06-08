using Harmoni360.Application.Features.PPE.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.PPE.Commands;

public class UpdatePPESizeCommand : IRequest<PPESizeDto>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}