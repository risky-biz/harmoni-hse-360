using Harmoni360.Application.Features.WasteReports.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public record GetWasteReportByIdQuery(int Id) : IRequest<WasteReportDto>;