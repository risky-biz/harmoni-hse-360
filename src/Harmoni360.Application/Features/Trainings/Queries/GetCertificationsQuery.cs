using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Queries;

public record GetCertificationsQuery : IRequest<PagedList<TrainingCertificationDto>>
{
    public int? UserId { get; init; }
    public int? TrainingId { get; init; }
    public CertificationType? CertificationType { get; init; }
    public bool? IsValid { get; init; }
    public bool? IsExpired { get; init; }
    public bool? IsK3Certificate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SearchTerm { get; init; } = string.Empty;
    public string SortBy { get; init; } = "IssuedDate";
    public bool SortDescending { get; init; } = true;
}