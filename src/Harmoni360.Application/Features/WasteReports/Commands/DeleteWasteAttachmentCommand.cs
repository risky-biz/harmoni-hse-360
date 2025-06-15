using MediatR;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record DeleteWasteAttachmentCommand(int AttachmentId) : IRequest;