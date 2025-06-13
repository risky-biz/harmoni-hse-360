using Harmoni360.Application.Features.PPE.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.PPE.Commands;

public record CreatePPEItemCommand : IRequest<PPEItemDto>
{
    public string ItemCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string Manufacturer { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public string? Color { get; init; }
    public DateTime PurchaseDate { get; init; }
    public decimal Cost { get; init; }
    public string Location { get; init; } = string.Empty;
    public DateTime? ExpiryDate { get; init; }
    public string? Notes { get; init; }
    
    // Certification Info
    public string? CertificationNumber { get; init; }
    public string? CertifyingBody { get; init; }
    public DateTime? CertificationDate { get; init; }
    public DateTime? CertificationExpiryDate { get; init; }
    public string? CertificationStandard { get; init; }
    
    // Maintenance Info
    public int? MaintenanceIntervalDays { get; init; }
    public DateTime? LastMaintenanceDate { get; init; }
    public string? MaintenanceInstructions { get; init; }
}