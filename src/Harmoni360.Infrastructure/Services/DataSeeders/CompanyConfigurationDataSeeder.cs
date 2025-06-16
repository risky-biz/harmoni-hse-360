using Harmoni360.Domain.Entities;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class CompanyConfigurationDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CompanyConfigurationDataSeeder> _logger;

    public CompanyConfigurationDataSeeder(
        ApplicationDbContext context, 
        ILogger<CompanyConfigurationDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check if any company configuration already exists
            var existingConfig = await _context.CompanyConfigurations
                .FirstOrDefaultAsync();

            if (existingConfig != null)
            {
                _logger.LogInformation("Company configuration already exists, skipping seeding");
                return;
            }

            _logger.LogInformation("Seeding default company configuration data...");

            // Create default company configuration based on British School Jakarta
            var defaultConfig = CompanyConfiguration.Create(
                companyName: "British School Jakarta",
                companyCode: "BSJ",
                description: "A premier international school in Jakarta, Indonesia, providing world-class education with a strong commitment to health, safety, security, and environmental excellence.",
                websiteUrl: "https://bsj.sch.id",
                primaryEmail: "info@bsj.sch.id");

            // Update contact information
            defaultConfig.UpdateContactInfo(
                primaryEmail: "info@bsj.sch.id",
                primaryPhone: "+62 21 745 1670",
                emergencyContactNumber: "+62 21 745 1671");

            // Update address information
            defaultConfig.UpdateAddress(
                address: "Benda Raya Street, Kemang",
                city: "Jakarta Selatan",
                state: "DKI Jakarta",
                postalCode: "12560",
                country: "Indonesia");

            // Update geographic coordinates (Jakarta, Indonesia - British School Jakarta area)
            defaultConfig.UpdateGeographicInfo(
                defaultLatitude: -6.1751,
                defaultLongitude: 106.8650);

            // Update branding colors (can be customized later)
            defaultConfig.UpdateBranding(
                primaryColor: "#1976d2",
                secondaryColor: "#424242",
                accentColor: "#ff5722");

            // Update compliance information
            defaultConfig.UpdateComplianceInfo(
                industryType: "Education",
                complianceStandards: "ISO 45001, ISO 14001, International School Standards",
                regulatoryAuthority: "Ministry of Education and Culture, Republic of Indonesia");

            // Update system settings
            defaultConfig.UpdateSystemSettings(
                timeZone: "Asia/Jakarta",
                dateFormat: "DD/MM/YYYY",
                currency: "IDR",
                language: "en-US");

            defaultConfig.CreatedBy = "System";

            _context.CompanyConfigurations.Add(defaultConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded default company configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding company configuration data");
            throw;
        }
    }
}