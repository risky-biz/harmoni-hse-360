using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.PPE.DTOs;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.PPE.Queries;

public class GetPPEItemsQueryHandler : IRequestHandler<GetPPEItemsQuery, GetPPEItemsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetPPEItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetPPEItemsResponse> Handle(GetPPEItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PPEItems
            .Include(i => i.Category)
            .Include(i => i.Size)
            .Include(i => i.StorageLocation)
            .Include(i => i.AssignedTo)
            .Include(i => i.Inspections)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(i => 
                i.ItemCode.ToLower().Contains(searchTerm) ||
                i.Name.ToLower().Contains(searchTerm) ||
                i.Description.ToLower().Contains(searchTerm) ||
                i.Manufacturer.ToLower().Contains(searchTerm) ||
                i.Model.ToLower().Contains(searchTerm));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<PPEStatus>(request.Status, out var status))
            {
                query = query.Where(i => i.Status == status);
            }
        }

        if (!string.IsNullOrEmpty(request.Condition))
        {
            if (Enum.TryParse<PPECondition>(request.Condition, out var condition))
            {
                query = query.Where(i => i.Condition == condition);
            }
        }

        if (!string.IsNullOrEmpty(request.Location))
        {
            query = query.Where(i => i.Location.ToLower().Contains(request.Location.ToLower()));
        }

        if (request.AssignedToId.HasValue)
        {
            query = query.Where(i => i.AssignedToId == request.AssignedToId.Value);
        }

        if (request.IsExpired.HasValue)
        {
            if (request.IsExpired.Value)
            {
                query = query.Where(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value < DateTime.UtcNow);
            }
            else
            {
                query = query.Where(i => !i.ExpiryDate.HasValue || i.ExpiryDate.Value >= DateTime.UtcNow);
            }
        }

        if (request.IsExpiringSoon.HasValue && request.IsExpiringSoon.Value)
        {
            var warningDate = DateTime.UtcNow.AddDays(30);
            query = query.Where(i => i.ExpiryDate.HasValue && 
                                   i.ExpiryDate.Value >= DateTime.UtcNow && 
                                   i.ExpiryDate.Value <= warningDate);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "itemcode" => request.SortDirection?.ToLower() == "desc" 
                ? query.OrderByDescending(i => i.ItemCode)
                : query.OrderBy(i => i.ItemCode),
            "name" => request.SortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Name)
                : query.OrderBy(i => i.Name),
            "category" => request.SortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Category.Name)
                : query.OrderBy(i => i.Category.Name),
            "status" => request.SortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            "condition" => request.SortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Condition)
                : query.OrderBy(i => i.Condition),
            "expiry" => request.SortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.ExpiryDate)
                : query.OrderBy(i => i.ExpiryDate),
            "created" => request.SortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt),
            _ => query.OrderBy(i => i.ItemCode)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new PPEItemSummaryDto
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                Name = i.Name,
                CategoryName = i.Category.Name,
                Manufacturer = i.Manufacturer,
                Model = i.Model,
                Size = i.Size != null ? i.Size.Name : string.Empty,
                Condition = i.Condition.ToString(),
                Status = i.Status.ToString(),
                Location = i.Location,
                AssignedToName = i.AssignedTo != null ? i.AssignedTo.Name : null,
                ExpiryDate = i.ExpiryDate,
                IsExpired = i.ExpiryDate.HasValue && i.ExpiryDate.Value < DateTime.UtcNow,
                IsExpiringSoon = i.ExpiryDate.HasValue && 
                               i.ExpiryDate.Value >= DateTime.UtcNow && 
                               i.ExpiryDate.Value <= DateTime.UtcNow.AddDays(30),
                IsMaintenanceDue = i.MaintenanceInfo != null && 
                                 i.MaintenanceInfo.NextMaintenanceDate.HasValue && 
                                 i.MaintenanceInfo.NextMaintenanceDate.Value <= DateTime.UtcNow,
                IsInspectionDue = i.Category.RequiresInspection && 
                                i.Category.InspectionIntervalDays.HasValue &&
                                (!i.Inspections.Any() || 
                                 i.Inspections.OrderByDescending(insp => insp.InspectionDate)
                                   .First().InspectionDate.AddDays(i.Category.InspectionIntervalDays.Value) <= DateTime.UtcNow)
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new GetPPEItemsResponse
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }
}