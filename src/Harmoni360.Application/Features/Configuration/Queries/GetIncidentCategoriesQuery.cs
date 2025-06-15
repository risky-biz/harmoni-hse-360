using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Configuration.Queries;

public class GetIncidentCategoriesQuery : IRequest<IEnumerable<IncidentCategoryDto>>
{
    public bool? IsActive { get; set; } = true;
}