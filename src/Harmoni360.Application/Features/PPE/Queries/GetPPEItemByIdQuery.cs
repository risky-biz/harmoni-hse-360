using Harmoni360.Application.Features.PPE.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.PPE.Queries;

public record GetPPEItemByIdQuery : IRequest<PPEItemDto?>
{
    public int Id { get; init; }
}