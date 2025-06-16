using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Application.Features.Statistics.Utils;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Harmoni360.Application.Features.Statistics.Queries;

public class ExportHsseStatisticsQueryHandler : IRequestHandler<ExportHsseStatisticsQuery, StatisticsFileDto>
{
    private readonly IMediator _mediator;

    public ExportHsseStatisticsQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<StatisticsFileDto> Handle(ExportHsseStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _mediator.Send(new GetHsseStatisticsQuery
        {
            Module = request.Module,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        }, cancellationToken);

        stats.Trir = HsseKpiCalculator.CalculateTrir(stats.TotalIncidents, request.HoursWorked);
        stats.Ltifr = HsseKpiCalculator.CalculateLtifr(request.LostTimeInjuries, request.HoursWorked);
        stats.SeverityRate = HsseKpiCalculator.CalculateSeverityRate(request.DaysLost, request.HoursWorked);
        stats.ComplianceRate = HsseKpiCalculator.CalculateComplianceRate(request.CompliantRecords, request.TotalRecords);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.Content().Column(stack =>
                {
                    stack.Item().AlignCenter().Text("HSSE Statistics Report").FontSize(20).Bold();
                    stack.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd}");
                    stack.Item().Text($"Module: {request.Module?.ToString() ?? "All"}");
                    if (request.StartDate.HasValue || request.EndDate.HasValue)
                    {
                        var start = request.StartDate?.ToString("yyyy-MM-dd") ?? "-";
                        var end = request.EndDate?.ToString("yyyy-MM-dd") ?? "-";
                        stack.Item().Text($"Date Range: {start} - {end}");
                    }
                    stack.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Metric");
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Value");
                        });
                        table.Cell().Padding(5).Text("Total Incidents");
                        table.Cell().Padding(5).Text(stats.TotalIncidents.ToString());
                        table.Cell().Padding(5).Text("Total Hazards");
                        table.Cell().Padding(5).Text(stats.TotalHazards.ToString());
                        table.Cell().Padding(5).Text("Security Incidents");
                        table.Cell().Padding(5).Text(stats.TotalSecurityIncidents.ToString());
                        table.Cell().Padding(5).Text("Health Incidents");
                        table.Cell().Padding(5).Text(stats.TotalHealthIncidents.ToString());
                        table.Cell().Padding(5).Text("TRIR");
                        table.Cell().Padding(5).Text(stats.Trir.ToString("F2"));
                        table.Cell().Padding(5).Text("LTIFR");
                        table.Cell().Padding(5).Text(stats.Ltifr.ToString("F2"));
                        table.Cell().Padding(5).Text("Severity Rate");
                        table.Cell().Padding(5).Text(stats.SeverityRate.ToString("F2"));
                        table.Cell().Padding(5).Text("Compliance Rate %");
                        table.Cell().Padding(5).Text(stats.ComplianceRate.ToString("F2"));
                    });
                });
            });
        });

        var pdf = doc.GeneratePdf();
        return new StatisticsFileDto
        {
            FileContent = pdf,
            FileName = $"hsse_statistics_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf",
            FileSize = pdf.Length,
            ContentType = "application/pdf"
        };
    }
}
