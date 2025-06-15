using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Configuration.Queries;

public class GetDepartmentsQuery : IRequest<IEnumerable<DepartmentDto>>
{
    public bool? IsActive { get; set; } = true;
}