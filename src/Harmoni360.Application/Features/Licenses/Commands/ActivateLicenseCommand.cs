using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record ActivateLicenseCommand : IRequest<LicenseDto>
{
    public int Id { get; init; }
    public string? ActivationNotes { get; init; }
}