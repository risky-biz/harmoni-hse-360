using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Queries;

public record GetLicenseByIdQuery : IRequest<LicenseDto?>
{
    public int Id { get; init; }
}