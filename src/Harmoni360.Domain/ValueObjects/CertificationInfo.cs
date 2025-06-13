using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.ValueObjects;

public class CertificationInfo : ValueObject
{
    public string CertificationNumber { get; private set; } = string.Empty;
    public string CertifyingBody { get; private set; } = string.Empty;
    public DateTime CertificationDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public string Standard { get; private set; } = string.Empty;

    protected CertificationInfo() { } // For EF Core

    private CertificationInfo(
        string certificationNumber,
        string certifyingBody,
        DateTime certificationDate,
        DateTime expiryDate,
        string standard)
    {
        CertificationNumber = certificationNumber;
        CertifyingBody = certifyingBody;
        CertificationDate = certificationDate;
        ExpiryDate = expiryDate;
        Standard = standard;
    }

    public static CertificationInfo Create(
        string certificationNumber,
        string certifyingBody,
        DateTime certificationDate,
        DateTime expiryDate,
        string standard)
    {
        if (string.IsNullOrWhiteSpace(certificationNumber))
            throw new ArgumentException("Certification number is required", nameof(certificationNumber));

        if (string.IsNullOrWhiteSpace(certifyingBody))
            throw new ArgumentException("Certifying body is required", nameof(certifyingBody));

        if (expiryDate <= certificationDate)
            throw new ArgumentException("Expiry date must be after certification date", nameof(expiryDate));

        return new CertificationInfo(certificationNumber, certifyingBody, certificationDate, expiryDate, standard);
    }

    public bool IsExpired => DateTime.UtcNow > ExpiryDate;

    public bool IsExpiringSoon(int daysWarning = 30) => 
        DateTime.UtcNow.AddDays(daysWarning) > ExpiryDate;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CertificationNumber;
        yield return CertifyingBody;
        yield return CertificationDate;
        yield return ExpiryDate;
        yield return Standard;
    }
}