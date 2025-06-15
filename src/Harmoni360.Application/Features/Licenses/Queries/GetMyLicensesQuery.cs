using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Queries;

public record GetMyLicensesQuery : IRequest<PagedList<LicenseDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public LicenseStatus? Status { get; init; }
    public LicenseType? Type { get; init; }
    public bool? IsExpiring { get; init; }
    public bool? IsExpired { get; init; }
    public string SortBy { get; init; } = "CreatedAt";
    public string SortDirection { get; init; } = "desc";
}