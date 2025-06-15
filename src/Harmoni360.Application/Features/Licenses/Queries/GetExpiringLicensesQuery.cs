using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Queries;

public record GetExpiringLicensesQuery : IRequest<PagedList<LicenseDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int DaysAhead { get; init; } = 30; // Default to next 30 days
    public bool IncludeOverdue { get; init; } = true;
    public string? Department { get; init; }
    public string SortBy { get; init; } = "ExpiryDate";
    public string SortDirection { get; init; } = "asc";
}