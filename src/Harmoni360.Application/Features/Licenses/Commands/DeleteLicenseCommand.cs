using MediatR;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record DeleteLicenseCommand : IRequest
{
    public int Id { get; init; }
}