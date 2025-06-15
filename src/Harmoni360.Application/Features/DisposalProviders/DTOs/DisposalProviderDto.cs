using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.DisposalProviders.DTOs;

public class DisposalProviderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiryDate { get; set; }
    public ProviderStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsExpired => LicenseExpiryDate < DateTime.UtcNow;
    public bool IsExpiringSoon => LicenseExpiryDate <= DateTime.UtcNow.AddDays(30);
}
