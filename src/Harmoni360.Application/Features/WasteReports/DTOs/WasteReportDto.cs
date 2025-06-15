namespace Harmoni360.Application.Features.WasteReports.DTOs;

public class WasteReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int? ReporterId { get; set; }
    public string? ReporterName { get; set; }
    public int AttachmentsCount { get; set; }
}
