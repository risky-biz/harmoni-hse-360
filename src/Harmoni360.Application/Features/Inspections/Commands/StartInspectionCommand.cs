using MediatR;

namespace Harmoni360.Application.Features.Inspections.Commands;

public record StartInspectionCommand(int InspectionId) : IRequest<Unit>;