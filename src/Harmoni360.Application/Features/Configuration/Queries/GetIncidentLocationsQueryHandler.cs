using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Configuration.Queries;

public class GetIncidentLocationsQueryHandler : IRequestHandler<GetIncidentLocationsQuery, IEnumerable<IncidentLocationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetIncidentLocationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IncidentLocationDto>> Handle(GetIncidentLocationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.IncidentLocations.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(l => l.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(request.Building))
        {
            query = query.Where(l => l.Building == request.Building);
        }

        var locations = await query
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.Name)
            .Select(l => new IncidentLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Code = l.Code,
                Description = l.Description,
                Building = l.Building,
                Floor = l.Floor,
                Room = l.Room,
                Latitude = l.GeoLocation != null ? l.GeoLocation.Latitude : null,
                Longitude = l.GeoLocation != null ? l.GeoLocation.Longitude : null,
                IsActive = l.IsActive,
                DisplayOrder = l.DisplayOrder,
                IsHighRisk = l.IsHighRisk,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy,
                LastModifiedAt = l.LastModifiedAt,
                LastModifiedBy = l.LastModifiedBy,
                FullLocation = l.GetFullLocation()
            })
            .ToListAsync(cancellationToken);

        return locations;
    }
}