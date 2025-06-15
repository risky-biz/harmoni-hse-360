using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.WorkPermits.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Queries
{
    public class GetMyWorkPermitsQuery : IRequest<PagedList<WorkPermitDto>>
    {
        public WorkPermitStatus? Status { get; set; }
        public WorkPermitType? Type { get; set; }
        public string Role { get; set; } = "Requester"; // Requester, Approver, Supervisor
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}