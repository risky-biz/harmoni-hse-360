using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class LicenseDataSeeder : IDataSeeder
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LicenseDataSeeder> _logger;
    private readonly Random _random = new();

    public LicenseDataSeeder(IApplicationDbContext context, ILogger<LicenseDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting License data seeding...");

            // Check if licenses already exist
            var existingLicenseCount = await _context.Licenses.CountAsync();
            if (existingLicenseCount > 0)
            {
                _logger.LogInformation("Licenses already exist ({Count} found). Skipping license seeding.", existingLicenseCount);
                return;
            }

            var licenses = new List<License>();

            // Generate 10 demo licenses
            for (int i = 1; i <= 10; i++)
            {
                var licenseType = GetRandomEnumValue<LicenseType>();
                var priority = GetRandomEnumValue<LicensePriority>();

                var issuedDate = DateTime.Now.AddDays(-_random.Next(30, 730));
                var expiryDate = issuedDate.AddDays(_random.Next(365, 1095));

                var license = License.Create(
                    title: $"{licenseType} License {i:D3}",
                    description: $"Demo {licenseType.ToString().ToLower()} license for testing purposes",
                    type: licenseType,
                    licenseNumber: $"LIC-{licenseType.ToString().ToUpper()}-{i:D4}",
                    issuingAuthority: GetIssuingAuthority(licenseType),
                    issuedDate: issuedDate,
                    expiryDate: expiryDate,
                    holderId: 1, // Default admin user ID
                    holderName: GetRandomHolderName(),
                    department: GetRandomDepartment(),
                    priority: priority
                );

                // Set random status
                SetRandomStatus(license);

                licenses.Add(license);
            }

            _context.Licenses.AddRange(licenses);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} licenses", licenses.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding license data");
            throw;
        }
    }

    private T GetRandomEnumValue<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(_random.Next(values.Length))!;
    }

    private string GetIssuingAuthority(LicenseType type)
    {
        return type switch
        {
            LicenseType.Environmental => "Environmental Protection Agency",
            LicenseType.Safety => "Occupational Safety and Health Administration",
            LicenseType.Health => "Department of Health Services",
            LicenseType.Construction => "Department of Building and Safety",
            LicenseType.Operating => "Industrial Operations Authority",
            LicenseType.Transport => "Department of Transportation",
            LicenseType.Waste => "Waste Management Authority",
            LicenseType.Chemical => "Chemical Safety Commission",
            LicenseType.Radiation => "Nuclear Regulatory Commission",
            LicenseType.Fire => "Fire Safety Authority",
            _ => "General Licensing Authority"
        };
    }

    private string GetRandomHolderName()
    {
        var names = new[] { "John Smith", "Sarah Johnson", "Michael Brown", "Emily Davis", "David Wilson" };
        return names[_random.Next(names.Length)];
    }

    private string GetRandomDepartment()
    {
        var departments = new[] { "Operations", "Safety & Compliance", "Environmental Affairs", "Engineering", "Production" };
        return departments[_random.Next(departments.Length)];
    }

    private void SetRandomStatus(License license)
    {
        var status = GetRandomEnumValue<LicenseStatus>();
        
        switch (status)
        {
            case LicenseStatus.Draft:
                // Default state, nothing to do
                break;
            case LicenseStatus.Submitted:
                license.Submit("System Seeder");
                break;
            case LicenseStatus.Approved:
                license.Submit("System Seeder");
                license.Approve("System Seeder", "Approved during data seeding");
                break;
            case LicenseStatus.Active:
                license.Submit("System Seeder");
                license.Approve("System Seeder", "Approved during data seeding");
                license.Activate("System Seeder");
                break;
            case LicenseStatus.Rejected:
                license.Submit("System Seeder");
                license.Reject("System Seeder", "Incomplete documentation");
                break;
            case LicenseStatus.Suspended:
                license.Submit("System Seeder");
                license.Approve("System Seeder", "Approved during data seeding");
                license.Activate("System Seeder");
                license.Suspend("System Seeder", "Compliance review required");
                break;
            case LicenseStatus.Revoked:
                license.Submit("System Seeder");
                license.Approve("System Seeder", "Approved during data seeding");
                license.Activate("System Seeder");
                license.Revoke("System Seeder", "Serious compliance violation");
                break;
        }
    }
}