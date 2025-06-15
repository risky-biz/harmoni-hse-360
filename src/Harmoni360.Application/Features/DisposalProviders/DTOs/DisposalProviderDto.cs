namespace Harmoni360.Application.Features.DisposalProviders.DTOs;

public class DisposalProviderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiryDate { get; set; }
}
