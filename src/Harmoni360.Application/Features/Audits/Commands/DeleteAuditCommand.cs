using MediatR;

namespace Harmoni360.Application.Features.Audits.Commands;

public record DeleteAuditCommand : IRequest
{
    public int Id { get; init; }
}