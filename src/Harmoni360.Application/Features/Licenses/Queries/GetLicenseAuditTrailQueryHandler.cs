using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Licenses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Licenses.Queries
{
    public class GetLicenseAuditTrailQueryHandler : IRequestHandler<GetLicenseAuditTrailQuery, PagedList<LicenseAuditLogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetLicenseAuditTrailQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedList<LicenseAuditLogDto>> Handle(GetLicenseAuditTrailQuery request, CancellationToken cancellationToken)
        {
            var query = _context.LicenseAuditLogs
                .Where(lal => lal.LicenseId == request.LicenseId)
                .OrderByDescending(lal => lal.PerformedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var auditLogs = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(lal => new LicenseAuditLogDto
                {
                    Id = lal.Id,
                    Action = lal.Action.ToString(),
                    ActionDescription = lal.ActionDescription,
                    PerformedAt = lal.PerformedAt,
                    PerformedBy = lal.PerformedBy,
                    IpAddress = lal.IpAddress,
                    UserAgent = lal.UserAgent,
                    OldValues = lal.OldValues,
                    NewValues = lal.NewValues,
                    Comments = lal.Comments
                })
                .ToListAsync(cancellationToken);

            return new PagedList<LicenseAuditLogDto>(
                auditLogs,
                request.Page,
                request.PageSize,
                totalCount
            );
        }
    }
}