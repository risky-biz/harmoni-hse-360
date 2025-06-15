using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record RejectLicenseCommand : IRequest<LicenseDto>
{
    public int Id { get; init; }
    public string RejectionReason { get; init; } = string.Empty;
    public string RejectionNotes { get; init; } = string.Empty;
}