using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record ApproveLicenseCommand : IRequest<LicenseDto>
{
    public int Id { get; init; }
    public string ApprovalNotes { get; init; } = string.Empty;
    public DateTime? EffectiveDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
}