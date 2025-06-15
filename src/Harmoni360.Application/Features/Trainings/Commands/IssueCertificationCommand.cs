using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record IssueCertificationCommand : IRequest<TrainingCertificationDto>
{
    public int TrainingId { get; init; }
    public int ParticipantId { get; init; }
    public CertificationType CertificationType { get; init; }
    public string CertifyingBody { get; init; } = string.Empty;
    public DateTime? ValidUntil { get; init; }
    public decimal? FinalScore { get; init; }
    public decimal? PassingScore { get; init; }
    public string IssuedBy { get; init; } = string.Empty;
    public string CertificateTitle { get; init; } = string.Empty;
    public bool IsK3Certificate { get; init; }
    public string K3CertificateType { get; init; } = string.Empty;
    public string K3LicenseClass { get; init; } = string.Empty;
    public string MinistryApprovalNumber { get; init; } = string.Empty;
}