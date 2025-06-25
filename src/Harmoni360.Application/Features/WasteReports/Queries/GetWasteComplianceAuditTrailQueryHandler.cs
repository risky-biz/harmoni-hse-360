using AutoMapper;
using MediatR;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;

namespace Harmoni360.Application.Features.WasteReports.Queries
{
    public class GetWasteComplianceAuditTrailQueryHandler : IRequestHandler<GetWasteComplianceAuditTrailQuery, IEnumerable<WasteAuditLogDto>>
    {
        private readonly IWasteAuditService _auditService;
        private readonly IMapper _mapper;

        public GetWasteComplianceAuditTrailQueryHandler(IWasteAuditService auditService, IMapper mapper)
        {
            _auditService = auditService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WasteAuditLogDto>> Handle(GetWasteComplianceAuditTrailQuery request, CancellationToken cancellationToken)
        {
            var auditLogs = await _auditService.GetComplianceAuditTrailAsync(request.FromDate, request.ToDate);
            return _mapper.Map<IEnumerable<WasteAuditLogDto>>(auditLogs);
        }
    }
}