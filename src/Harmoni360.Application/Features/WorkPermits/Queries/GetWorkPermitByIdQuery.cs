using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Queries
{
    public class GetWorkPermitByIdQuery : IRequest<WorkPermitDto?>
    {
        public int Id { get; set; }
    }
}