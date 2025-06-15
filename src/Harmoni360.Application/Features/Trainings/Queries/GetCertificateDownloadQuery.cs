using MediatR;

namespace Harmoni360.Application.Features.Trainings.Queries;

public record GetCertificateDownloadQuery : IRequest<CertificateFileDto?>
{
    public int CertificationId { get; init; }
}

public class CertificateFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public long FileSize { get; set; }
}