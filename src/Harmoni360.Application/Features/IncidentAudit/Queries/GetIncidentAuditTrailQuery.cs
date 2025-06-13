using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.IncidentAudit.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.IncidentAudit.Queries;

public record GetIncidentAuditTrailQuery(int IncidentId) : IRequest<List<IncidentAuditLogDto>>;

public class GetIncidentAuditTrailQueryHandler : IRequestHandler<GetIncidentAuditTrailQuery, List<IncidentAuditLogDto>>
{
    private readonly IIncidentAuditService _auditService;

    public GetIncidentAuditTrailQueryHandler(IIncidentAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<List<IncidentAuditLogDto>> Handle(GetIncidentAuditTrailQuery request, CancellationToken cancellationToken)
    {
        var auditLogs = await _auditService.GetAuditTrailAsync(request.IncidentId);

        return auditLogs.Select(log => new IncidentAuditLogDto
        {
            Id = log.Id,
            IncidentId = log.IncidentId,
            Action = log.Action,
            FieldName = log.FieldName,
            OldValue = log.OldValue,
            NewValue = log.NewValue,
            ChangedBy = log.ChangedBy,
            ChangedAt = log.ChangedAt,
            ChangeDescription = log.ChangeDescription
        }).ToList();
    }
}