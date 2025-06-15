using System.Collections.Generic;
using MediatR;
using Harmoni360.Application.Features.Hazards.DTOs;

namespace Harmoni360.Application.Features.Hazards.Queries
{
    public class GetHazardAuditTrailQuery : IRequest<List<HazardAuditLogDto>>
    {
        public int HazardId { get; set; }
    }
}