using MediatR;
using Microsoft.AspNetCore.Http;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public record UploadLicenseAttachmentCommand : IRequest<LicenseAttachmentDto>
{
    public int LicenseId { get; init; }
    public IFormFile File { get; init; } = null!;
    public LicenseAttachmentType AttachmentType { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool IsRequired { get; init; } = false;
    public DateTime? ValidUntil { get; init; }
}