using MediatR;
using Harmoni360.Application.Features.Audits.DTOs;

namespace Harmoni360.Application.Features.Audits.Commands;

public record ArchiveAuditCommand : IRequest<AuditDto>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
}