namespace Harmoni360.Application.Features.Statistics.DTOs;

public class HsseTrendPointDto
{
    public string Period { get; set; } = string.Empty; // "2024-01" format
    public string PeriodLabel { get; set; } = string.Empty; // "Jan 2024"
    public int IncidentCount { get; set; }
    public int HazardCount { get; set; }
    public int SecurityIncidentCount { get; set; }
    public int HealthIncidentCount { get; set; }
}
