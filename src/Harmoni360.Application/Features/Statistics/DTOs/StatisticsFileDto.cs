namespace Harmoni360.Application.Features.Statistics.DTOs;

public class StatisticsFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public long FileSize { get; set; }
}
