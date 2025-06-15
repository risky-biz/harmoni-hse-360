using MediatR;
using Harmoni360.Application.Features.Audits.DTOs;

namespace Harmoni360.Application.Features.Audits.Commands;

public record StartAuditCommand : IRequest<AuditDto>
{
    public int Id { get; init; }
}