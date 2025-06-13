using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Configuration.Queries;

public class GetIncidentCategoriesQueryHandler : IRequestHandler<GetIncidentCategoriesQuery, IEnumerable<IncidentCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetIncidentCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IncidentCategoryDto>> Handle(GetIncidentCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.IncidentCategories.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new IncidentCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                Color = c.Color,
                Icon = c.Icon,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder,
                RequiresImmediateAction = c.RequiresImmediateAction,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                LastModifiedAt = c.LastModifiedAt,
                LastModifiedBy = c.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        return categories;
    }
}