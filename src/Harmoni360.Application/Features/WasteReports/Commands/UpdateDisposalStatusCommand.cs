using Harmoni360.Domain.Entities.Waste;
using MediatR;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record UpdateDisposalStatusCommand(int Id, WasteDisposalStatus Status) : IRequest;
