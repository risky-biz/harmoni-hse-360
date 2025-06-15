using MediatR;

namespace Harmoni360.Application.Features.Incidents.Commands;

public class UpdateIncidentStatusCommand : IRequest
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
}