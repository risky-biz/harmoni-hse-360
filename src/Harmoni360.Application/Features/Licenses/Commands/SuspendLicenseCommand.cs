using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record SuspendLicenseCommand : IRequest<LicenseDto>
{
    public int Id { get; init; }
    public string SuspensionReason { get; init; } = string.Empty;
    public DateTime? EffectiveDate { get; init; }
}