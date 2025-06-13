using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.PPE.DTOs;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.PPE.Queries;

public class GetPPEItemByIdQueryHandler : IRequestHandler<GetPPEItemByIdQuery, PPEItemDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPPEItemByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PPEItemDto?> Handle(GetPPEItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.PPEItems
            .Include(i => i.Category)
            .Include(i => i.Size)
            .Include(i => i.StorageLocation)
            .Include(i => i.AssignedTo)
            .Include(i => i.Inspections)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item == null)
            return null;

        var now = DateTime.UtcNow;

        return new PPEItemDto
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            Name = item.Name,
            Description = item.Description,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            CategoryType = item.Category.Type.ToString(),
            Manufacturer = item.Manufacturer,
            Model = item.Model,
            Size = item.Size?.Name ?? string.Empty,
            Color = item.Color,
            Condition = item.Condition.ToString(),
            ExpiryDate = item.ExpiryDate,
            PurchaseDate = item.PurchaseDate,
            Cost = item.Cost,
            Location = item.Location,
            AssignedToId = item.AssignedToId,
            AssignedToName = item.AssignedTo?.Name,
            AssignedDate = item.AssignedDate,
            Status = item.Status.ToString(),
            Notes = item.Notes,

            // Certification Info
            CertificationNumber = item.Certification?.CertificationNumber,
            CertifyingBody = item.Certification?.CertifyingBody,
            CertificationDate = item.Certification?.CertificationDate,
            CertificationExpiryDate = item.Certification?.ExpiryDate,
            CertificationStandard = item.Certification?.Standard,

            // Maintenance Info
            MaintenanceIntervalDays = item.MaintenanceInfo?.IntervalDays,
            LastMaintenanceDate = item.MaintenanceInfo?.LastMaintenanceDate,
            NextMaintenanceDate = item.MaintenanceInfo?.NextMaintenanceDate,
            MaintenanceInstructions = item.MaintenanceInfo?.MaintenanceInstructions,

            // Computed Properties
            IsExpired = item.ExpiryDate.HasValue && item.ExpiryDate.Value < now,
            IsExpiringSoon = item.ExpiryDate.HasValue && 
                           item.ExpiryDate.Value >= now && 
                           item.ExpiryDate.Value <= now.AddDays(30),
            IsMaintenanceDue = item.MaintenanceInfo != null && 
                             item.MaintenanceInfo.NextMaintenanceDate.HasValue && 
                             item.MaintenanceInfo.NextMaintenanceDate.Value <= now,
            IsMaintenanceDueSoon = item.MaintenanceInfo != null && 
                                 item.MaintenanceInfo.NextMaintenanceDate.HasValue && 
                                 item.MaintenanceInfo.NextMaintenanceDate.Value > now &&
                                 item.MaintenanceInfo.NextMaintenanceDate.Value <= now.AddDays(7),
            IsCertificationExpired = item.Certification != null && item.Certification.ExpiryDate < now,
            IsCertificationExpiringSoon = item.Certification != null && 
                                        item.Certification.ExpiryDate >= now && 
                                        item.Certification.ExpiryDate <= now.AddDays(30),
            LastInspectionDate = item.Inspections.Any() 
                ? item.Inspections.OrderByDescending(i => i.InspectionDate).First().InspectionDate
                : null,
            NextInspectionDue = item.Category.RequiresInspection && item.Category.InspectionIntervalDays.HasValue
                ? (item.Inspections.Any() 
                    ? item.Inspections.OrderByDescending(i => i.InspectionDate)
                        .First().InspectionDate.AddDays(item.Category.InspectionIntervalDays.Value)
                    : item.PurchaseDate.AddDays(item.Category.InspectionIntervalDays.Value))
                : null,
            IsInspectionDue = item.Category.RequiresInspection && 
                            item.Category.InspectionIntervalDays.HasValue &&
                            (!item.Inspections.Any() || 
                             item.Inspections.OrderByDescending(insp => insp.InspectionDate)
                               .First().InspectionDate.AddDays(item.Category.InspectionIntervalDays.Value) <= now),

            // Audit Info
            CreatedAt = item.CreatedAt,
            CreatedBy = item.CreatedBy,
            LastModifiedAt = item.LastModifiedAt,
            LastModifiedBy = item.LastModifiedBy
        };
    }
}