using Harmoni360.Application.Features.Inspections.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Harmoni360.Application.Features.Inspections.Commands;

public class UploadInspectionAttachmentCommand : IRequest<InspectionAttachmentDto>
{
    public int InspectionId { get; set; }
    public IFormFile File { get; set; } = null!;
    public string? Description { get; set; }
    public string? Category { get; set; }
}