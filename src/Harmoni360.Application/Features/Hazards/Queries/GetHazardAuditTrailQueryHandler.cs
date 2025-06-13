using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Hazards.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Hazards.Queries
{
    public class GetHazardAuditTrailQueryHandler : IRequestHandler<GetHazardAuditTrailQuery, List<HazardAuditLogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetHazardAuditTrailQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<HazardAuditLogDto>> Handle(GetHazardAuditTrailQuery request, CancellationToken cancellationToken)
        {
            var auditLogs = await _context.HazardAuditLogs
                .Where(hal => hal.HazardId == request.HazardId)
                .OrderByDescending(hal => hal.ChangedAt)
                .Select(hal => new HazardAuditLogDto
                {
                    Id = hal.Id,
                    HazardId = hal.HazardId,
                    Action = hal.Action,
                    FieldName = hal.FieldName,
                    OldValue = hal.OldValue,
                    NewValue = hal.NewValue,
                    ChangeDescription = hal.ChangeDescription,
                    ChangedBy = hal.ChangedBy,
                    ChangedAt = hal.ChangedAt
                })
                .ToListAsync(cancellationToken);

            return auditLogs;
        }
    }
}