using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class PPEDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PPEDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public PPEDataSeeder(ApplicationDbContext context, ILogger<PPEDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Check if we should re-seed PPE data even if it exists
        var reSeedPPEData = _configuration["DataSeeding:ReSeedPPEData"] == "true";

        if (!reSeedPPEData && (await _context.PPECategories.AnyAsync() || await _context.PPESizes.AnyAsync() || await _context.PPEStorageLocations.AnyAsync()))
        {
            _logger.LogInformation("PPE data already exists and ReSeedPPEData is false, skipping PPE data seeding");
            return;
        }

        _logger.LogInformation("Starting PPE data seeding...");

        // If re-seeding is enabled, clear existing data first
        if (reSeedPPEData)
        {
            if (await _context.PPECategories.AnyAsync())
            {
                _logger.LogInformation("Clearing existing PPE categories for re-seeding...");
                _context.PPECategories.RemoveRange(_context.PPECategories);
            }
            if (await _context.PPESizes.AnyAsync())
            {
                _logger.LogInformation("Clearing existing PPE sizes for re-seeding...");
                _context.PPESizes.RemoveRange(_context.PPESizes);
            }
            if (await _context.PPEStorageLocations.AnyAsync())
            {
                _logger.LogInformation("Clearing existing PPE storage locations for re-seeding...");
                _context.PPEStorageLocations.RemoveRange(_context.PPEStorageLocations);
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing PPE data cleared");
        }

        // Seed PPE Categories
        var categories = new List<PPECategory>
        {
            PPECategory.Create("Hard Hats", "HARDHAT", "Head protection equipment", PPEType.HeadProtection, "system", true, true, 90, false, null, "EN 397"),
            PPECategory.Create("Safety Glasses", "GLASSES", "Eye protection equipment", PPEType.EyeProtection, "system", false, true, 30, false, null, "EN 166"),
            PPECategory.Create("Hearing Protection", "HEARING", "Ear protection equipment", PPEType.HearingProtection, "system", false, true, 30, false, null, "EN 352"),
            PPECategory.Create("Respirators", "RESPIRATOR", "Respiratory protection equipment", PPEType.RespiratoryProtection, "system", true, true, 30, true, 365, "EN 149"),
            PPECategory.Create("Work Gloves", "GLOVES", "Hand protection equipment", PPEType.HandProtection, "system", false, true, 30, false, null, "EN 388"),
            PPECategory.Create("Safety Boots", "BOOTS", "Foot protection equipment", PPEType.FootProtection, "system", true, true, 180, false, null, "EN ISO 20345"),
            PPECategory.Create("Hi-Vis Vests", "HIVIS", "High visibility clothing", PPEType.HighVisibility, "system", false, true, 90, false, null, "EN ISO 20471"),
            PPECategory.Create("Fall Harness", "HARNESS", "Fall protection equipment", PPEType.FallProtection, "system", true, true, 180, false, null, "EN 361"),
            PPECategory.Create("Lab Coats", "LABCOAT", "Body protection for laboratories", PPEType.BodyProtection, "system", false, true, 30, false, null, "EN 14126"),
            PPECategory.Create("Emergency Shower", "SHOWER", "Emergency decontamination equipment", PPEType.EmergencyEquipment, "system", true, true, 30, false, null, "ANSI Z358.1")
        };

        await _context.PPECategories.AddRangeAsync(categories);
        _logger.LogInformation("Seeded {Count} PPE categories", categories.Count);

        // Seed PPE Sizes
        var sizes = new List<PPESize>
        {
            PPESize.Create("Extra Small", "XS", "Extra Small size", "system", sortOrder: 1),
            PPESize.Create("Small", "S", "Small size", "system", sortOrder: 2),
            PPESize.Create("Medium", "M", "Medium size", "system", sortOrder: 3),
            PPESize.Create("Large", "L", "Large size", "system", sortOrder: 4),
            PPESize.Create("Extra Large", "XL", "Extra Large size", "system", sortOrder: 5),
            PPESize.Create("Double Extra Large", "XXL", "Double Extra Large size", "system", sortOrder: 6),
            PPESize.Create("Triple Extra Large", "XXXL", "Triple Extra Large size", "system", sortOrder: 7),
            PPESize.Create("One Size", "OS", "One size fits all", "system", sortOrder: 8),
            PPESize.Create("6", "6", "Size 6", "system", sortOrder: 10),
            PPESize.Create("7", "7", "Size 7", "system", sortOrder: 11),
            PPESize.Create("8", "8", "Size 8", "system", sortOrder: 12),
            PPESize.Create("9", "9", "Size 9", "system", sortOrder: 13),
            PPESize.Create("10", "10", "Size 10", "system", sortOrder: 14),
            PPESize.Create("11", "11", "Size 11", "system", sortOrder: 15),
            PPESize.Create("12", "12", "Size 12", "system", sortOrder: 16)
        };

        await _context.PPESizes.AddRangeAsync(sizes);
        _logger.LogInformation("Seeded {Count} PPE sizes", sizes.Count);

        // Seed PPE Storage Locations
        var storageLocations = new List<PPEStorageLocation>
        {
            PPEStorageLocation.Create("Main Safety Office", "MSO", "system", "Primary safety equipment storage", "Building A, Ground Floor", "HSE Manager", "+62-21-1234567", 2000),
            PPEStorageLocation.Create("Chemistry Lab Storage", "CHEM", "system", "Chemistry laboratory PPE storage", "Science Building, 2nd Floor", "Lab Supervisor", "+62-21-1234568", 500),
            PPEStorageLocation.Create("Maintenance Workshop", "MAINT", "system", "Maintenance department storage", "Workshop Building", "Maintenance Supervisor", "+62-21-1234569", 1000),
            PPEStorageLocation.Create("PE Equipment Room", "PE", "system", "Physical education equipment storage", "Sports Complex", "PE Coordinator", "+62-21-1234570", 300),
            PPEStorageLocation.Create("Emergency Response Station", "ERS", "system", "Emergency response equipment", "Multiple Locations", "Security Chief", "+62-21-1234571", 200),
            PPEStorageLocation.Create("Warehouse A", "WH-A", "system", "Primary warehouse storage", "External Warehouse Complex", "Warehouse Manager", "+62-21-1234572", 5000),
            PPEStorageLocation.Create("Cafeteria Storage", "CAF", "system", "Food service PPE storage", "Cafeteria Building", "Food Service Manager", "+62-21-1234573", 150)
        };

        await _context.PPEStorageLocations.AddRangeAsync(storageLocations);
        _logger.LogInformation("Seeded {Count} PPE storage locations", storageLocations.Count);
    }
}