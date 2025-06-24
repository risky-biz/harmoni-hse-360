using AutoMapper;
using MediatR;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WasteReports.Queries
{
    public class GetWasteAuditTrailQueryHandler : IRequestHandler<GetWasteAuditTrailQuery, IEnumerable<WasteAuditLogDto>>
    {
        private readonly IWasteAuditService _auditService;
        private readonly IMapper _mapper;

        public GetWasteAuditTrailQueryHandler(IWasteAuditService auditService, IMapper mapper)
        {
            _auditService = auditService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WasteAuditLogDto>> Handle(GetWasteAuditTrailQuery request, CancellationToken cancellationToken)
        {
            var auditLogs = await _auditService.GetAuditTrailAsync(request.WasteReportId);
            return _mapper.Map<IEnumerable<WasteAuditLogDto>>(auditLogs);
        }
    }
}