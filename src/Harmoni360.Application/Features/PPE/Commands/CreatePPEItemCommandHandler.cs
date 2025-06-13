using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Utilities;
using Harmoni360.Application.Features.PPE.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.PPE.Commands;

public class CreatePPEItemCommandHandler : IRequestHandler<CreatePPEItemCommand, PPEItemDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreatePPEItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PPEItemDto> Handle(CreatePPEItemCommand request, CancellationToken cancellationToken)
    {
        // Verify category exists
        var category = await _context.PPECategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);

        if (category == null)
        {
            throw new ArgumentException($"PPE Category with ID {request.CategoryId} not found or inactive");
        }

        // Check if item code is unique
        var existingItem = await _context.PPEItems
            .FirstOrDefaultAsync(i => i.ItemCode == request.ItemCode, cancellationToken);

        if (existingItem != null)
        {
            throw new ArgumentException($"PPE Item with code '{request.ItemCode}' already exists");
        }

        // Create certification info if provided
        CertificationInfo? certification = null;
        if (!string.IsNullOrEmpty(request.CertificationNumber) && 
            !string.IsNullOrEmpty(request.CertifyingBody) &&
            request.CertificationDate.HasValue &&
            request.CertificationExpiryDate.HasValue)
        {
            certification = CertificationInfo.Create(
                request.CertificationNumber,
                request.CertifyingBody,
                DateTimeUtilities.EnsureUtc(request.CertificationDate.Value),
                DateTimeUtilities.EnsureUtc(request.CertificationExpiryDate.Value),
                request.CertificationStandard ?? string.Empty);
        }

        // Create maintenance schedule if provided
        MaintenanceSchedule? maintenanceSchedule = null;
        if (request.MaintenanceIntervalDays.HasValue && request.MaintenanceIntervalDays.Value > 0)
        {
            maintenanceSchedule = MaintenanceSchedule.Create(
                request.MaintenanceIntervalDays.Value,
                DateTimeUtilities.EnsureUtc(request.LastMaintenanceDate),
                request.MaintenanceInstructions);
        }

        // Create PPE item
        var ppeItem = PPEItem.Create(
            request.ItemCode,
            request.Name,
            request.Description,
            request.CategoryId,
            request.Manufacturer,
            request.Model,
            DateTimeUtilities.EnsureUtc(request.PurchaseDate),
            request.Cost,
            _currentUserService.Email,
            sizeId: null, // TODO: Map from request.Size when we have Size entity
            storageLocationId: null, // TODO: Map from request.Location when we have StorageLocation entity
            location: request.Location,
            color: request.Color,
            expiryDate: DateTimeUtilities.EnsureUtc(request.ExpiryDate),
            certification: certification,
            maintenanceInfo: maintenanceSchedule,
            notes: request.Notes);

        _context.PPEItems.Add(ppeItem);
        await _context.SaveChangesAsync(cancellationToken);

        // Return DTO
        return new PPEItemDto
        {
            Id = ppeItem.Id,
            ItemCode = ppeItem.ItemCode,
            Name = ppeItem.Name,
            Description = ppeItem.Description,
            CategoryId = ppeItem.CategoryId,
            CategoryName = category.Name,
            CategoryType = category.Type.ToString(),
            Manufacturer = ppeItem.Manufacturer,
            Model = ppeItem.Model,
            Size = ppeItem.Size?.Name ?? string.Empty,
            Color = ppeItem.Color,
            Condition = ppeItem.Condition.ToString(),
            ExpiryDate = ppeItem.ExpiryDate,
            PurchaseDate = ppeItem.PurchaseDate,
            Cost = ppeItem.Cost,
            Location = ppeItem.Location,
            Status = ppeItem.Status.ToString(),
            Notes = ppeItem.Notes,
            CertificationNumber = ppeItem.Certification?.CertificationNumber,
            CertifyingBody = ppeItem.Certification?.CertifyingBody,
            CertificationDate = ppeItem.Certification?.CertificationDate,
            CertificationExpiryDate = ppeItem.Certification?.ExpiryDate,
            CertificationStandard = ppeItem.Certification?.Standard,
            MaintenanceIntervalDays = ppeItem.MaintenanceInfo?.IntervalDays,
            LastMaintenanceDate = ppeItem.MaintenanceInfo?.LastMaintenanceDate,
            NextMaintenanceDate = ppeItem.MaintenanceInfo?.NextMaintenanceDate,
            MaintenanceInstructions = ppeItem.MaintenanceInfo?.MaintenanceInstructions,
            IsExpired = ppeItem.IsExpired,
            IsExpiringSoon = ppeItem.IsExpiringSoon(),
            IsMaintenanceDue = ppeItem.IsMaintenanceDue,
            IsMaintenanceDueSoon = ppeItem.IsMaintenanceDueSoon(),
            IsCertificationExpired = ppeItem.IsCertificationExpired,
            IsCertificationExpiringSoon = ppeItem.IsCertificationExpiringSoon(),
            LastInspectionDate = ppeItem.LastInspectionDate,
            NextInspectionDue = ppeItem.NextInspectionDue,
            IsInspectionDue = ppeItem.IsInspectionDue,
            CreatedAt = ppeItem.CreatedAt,
            CreatedBy = ppeItem.CreatedBy,
            LastModifiedAt = ppeItem.LastModifiedAt,
            LastModifiedBy = ppeItem.LastModifiedBy
        };
    }
}