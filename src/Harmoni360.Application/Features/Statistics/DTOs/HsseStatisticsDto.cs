namespace Harmoni360.Application.Features.Statistics.DTOs;

public class HsseStatisticsDto
{
    public int TotalIncidents { get; set; }
    public int TotalHazards { get; set; }
    public int TotalSecurityIncidents { get; set; }
    public int TotalHealthIncidents { get; set; }

    public double Trir { get; set; }
    public double Ltifr { get; set; }
    public double SeverityRate { get; set; }
    public double ComplianceRate { get; set; }
}
