using HarmoniHSE360.Application.Features.PPE.DTOs;
using MediatR;

namespace HarmoniHSE360.Application.Features.PPE.Queries;

public record GetPPEItemByIdQuery : IRequest<PPEItemDto?>
{
    public int Id { get; init; }
}