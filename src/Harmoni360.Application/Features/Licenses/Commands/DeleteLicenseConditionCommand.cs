using MediatR;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record DeleteLicenseConditionCommand : IRequest<Unit>
{
    public int LicenseId { get; init; }
    public int ConditionId { get; init; }
}