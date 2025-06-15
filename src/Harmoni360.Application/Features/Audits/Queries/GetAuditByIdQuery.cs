using MediatR;
using Harmoni360.Application.Features.Audits.DTOs;

namespace Harmoni360.Application.Features.Audits.Queries;

public record GetAuditByIdQuery : IRequest<AuditDto?>
{
    public int Id { get; init; }
}