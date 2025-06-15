using MediatR;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Queries;

public record GetLicenseDashboardQuery : IRequest<LicenseDashboardDto>
{
    public int? DepartmentId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}