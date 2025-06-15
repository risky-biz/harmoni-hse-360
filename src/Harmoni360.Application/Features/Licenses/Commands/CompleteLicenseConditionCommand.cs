using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record CompleteLicenseConditionCommand : IRequest<LicenseConditionDto>
{
    public int LicenseId { get; init; }
    public int ConditionId { get; init; }
    public string ComplianceEvidence { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}