using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Queries;

public record GetLicensesQuery : IRequest<PagedList<LicenseDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public LicenseStatus? Status { get; init; }
    public LicenseType? Type { get; init; }
    public LicensePriority? Priority { get; init; }
    public RiskLevel? RiskLevel { get; init; }
    public string? Department { get; init; }
    public string? IssuingAuthority { get; init; }
    public bool? IsExpiring { get; init; } // Licenses expiring within 30 days
    public bool? IsExpired { get; init; }
    public bool? RenewalDue { get; init; }
    public DateTime? ExpiryDateFrom { get; init; }
    public DateTime? ExpiryDateTo { get; init; }
    public string SortBy { get; init; } = "CreatedAt";
    public string SortDirection { get; init; } = "desc";
}