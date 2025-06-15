using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Queries
{
    public class GetOverdueWorkPermitsQuery : IRequest<PagedList<WorkPermitDto>>
    {
        public WorkPermitType? Type { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "PlannedEndDate";
        public bool SortDescending { get; set; } = false;
    }
}