using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record RenewLicenseCommand : IRequest<LicenseDto>
{
    public int Id { get; init; }
    public string RenewalNotes { get; init; } = string.Empty;
}