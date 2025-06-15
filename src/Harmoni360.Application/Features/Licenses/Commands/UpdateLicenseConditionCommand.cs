using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record UpdateLicenseConditionCommand : IRequest<LicenseConditionDto>
{
    public int LicenseId { get; init; }
    public int ConditionId { get; init; }
    public string ConditionType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsMandatory { get; init; } = true;
    public DateTime? DueDate { get; init; }
    public LicenseConditionStatus Status { get; init; }
    public string ResponsiblePerson { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}