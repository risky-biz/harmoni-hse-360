using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record AddLicenseConditionCommand : IRequest<LicenseConditionDto>
{
    public int LicenseId { get; init; }
    public string ConditionType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsMandatory { get; init; } = true;
    public DateTime? DueDate { get; init; }
    public string ResponsiblePerson { get; init; } = string.Empty;
}