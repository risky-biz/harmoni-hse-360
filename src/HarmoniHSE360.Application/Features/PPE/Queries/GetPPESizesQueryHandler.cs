using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.PPE.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Application.Features.PPE.Queries;

public class GetPPESizesQueryHandler : IRequestHandler<GetPPESizesQuery, List<PPESizeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPPESizesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PPESizeDto>> Handle(GetPPESizesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PPESizes.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(searchTerm) ||
                s.Code.ToLower().Contains(searchTerm) ||
                (s.Description != null && s.Description.ToLower().Contains(searchTerm))
            );
        }

        var sizes = await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => new PPESizeDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Description = s.Description,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                LastModifiedAt = s.LastModifiedAt,
                LastModifiedBy = s.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        return sizes;
    }
}