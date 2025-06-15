using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Waste;

public class DisposalProvider : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public DateTime LicenseExpiryDate { get; private set; }
    public ProviderStatus Status { get; private set; } = ProviderStatus.Active;
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected DisposalProvider() { }

    public static DisposalProvider Create(string name, string licenseNumber, DateTime licenseExpiryDate, string createdBy)
    {
        return new DisposalProvider
        {
            Name = name,
            LicenseNumber = licenseNumber,
            LicenseExpiryDate = licenseExpiryDate,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Update(string name, string licenseNumber, DateTime licenseExpiryDate)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        LicenseExpiryDate = licenseExpiryDate;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(ProviderStatus status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
    }
}
