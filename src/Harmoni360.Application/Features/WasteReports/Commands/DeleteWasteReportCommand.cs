using MediatR;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record DeleteWasteReportCommand(int Id) : IRequest<Unit>;