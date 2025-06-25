using MediatR;

namespace Harmoni360.Application.Features.Inspections.Commands;

public class DeleteInspectionAttachmentCommand : IRequest
{
    public int InspectionId { get; set; }
    public int AttachmentId { get; set; }
}