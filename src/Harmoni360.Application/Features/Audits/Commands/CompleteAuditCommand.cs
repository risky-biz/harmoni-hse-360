using MediatR;
using Harmoni360.Application.Features.Audits.DTOs;

namespace Harmoni360.Application.Features.Audits.Commands;

public record CompleteAuditCommand : IRequest<AuditDto>
{
    public int Id { get; init; }
    public string? Summary { get; init; }
    public string? Recommendations { get; init; }
}