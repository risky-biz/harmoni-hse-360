using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record RevokeLicenseCommand : IRequest<LicenseDto>
{
    public int Id { get; init; }
    public string RevocationReason { get; init; } = string.Empty;
    public DateTime? EffectiveDate { get; init; }
    public bool IsImmediate { get; init; } = true;
}