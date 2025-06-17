using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Queries
{
    public class GetLicenseAuditTrailQuery : IRequest<PagedList<LicenseAuditLogDto>>
    {
        public int LicenseId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}