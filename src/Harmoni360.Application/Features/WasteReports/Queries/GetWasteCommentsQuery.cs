using MediatR;
using Harmoni360.Application.Features.WasteReports.Commands;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public record GetWasteCommentsQuery(int WasteReportId) : IRequest<List<WasteCommentDto>>;