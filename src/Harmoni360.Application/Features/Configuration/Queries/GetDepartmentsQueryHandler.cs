using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Configuration.Queries;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, IEnumerable<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Departments.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(d => d.IsActive == request.IsActive.Value);
        }

        var departments = await query
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Name)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                HeadOfDepartment = d.HeadOfDepartment,
                Contact = d.Contact,
                Location = d.Location,
                IsActive = d.IsActive,
                DisplayOrder = d.DisplayOrder,
                CreatedAt = d.CreatedAt,
                CreatedBy = d.CreatedBy,
                LastModifiedAt = d.LastModifiedAt,
                LastModifiedBy = d.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        return departments;
    }
}