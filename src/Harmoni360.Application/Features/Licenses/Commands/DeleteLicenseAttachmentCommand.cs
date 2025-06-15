using MediatR;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record DeleteLicenseAttachmentCommand : IRequest
{
    public int LicenseId { get; init; }
    public int AttachmentId { get; init; }
}