using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.PPE.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.PPE.Queries;

public class GetPPECategoriesQueryHandler : IRequestHandler<GetPPECategoriesQuery, List<PPECategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPPECategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PPECategoryDto>> Handle(GetPPECategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PPECategories.AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var categories = await query
            .OrderBy(c => c.Name)
            .Select(c => new PPECategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type.ToString(),
                RequiresCertification = c.RequiresCertification,
                RequiresInspection = c.RequiresInspection,
                InspectionIntervalDays = c.InspectionIntervalDays,
                RequiresExpiry = c.RequiresExpiry,
                DefaultExpiryDays = c.DefaultExpiryDays,
                ComplianceStandard = c.ComplianceStandard,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        return categories;
    }
}