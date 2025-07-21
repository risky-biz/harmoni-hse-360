using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities.Waste;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class WasteDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WasteDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public WasteDataSeeder(ApplicationDbContext context, ILogger<WasteDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Get seeding configuration
        var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
        var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                         (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);

        _logger.LogInformation("WasteDataSeeder - ForceReseed: {ForceReseed}", forceReseed);

        // Check if waste data already exists
        var hasTypesData = await _context.WasteTypes.AnyAsync();
        var hasProvidersData = await _context.DisposalProviders.AnyAsync();

        if (!forceReseed && (hasTypesData || hasProvidersData))
        {
            _logger.LogInformation("Waste data already exists and ForceReseed is false, skipping waste seeding");
            return;
        }

        _logger.LogInformation("Starting waste management data seeding...");

        // If re-seeding is enabled, clear existing data first
        if (forceReseed && (hasTypesData || hasProvidersData))
        {
            _logger.LogInformation("Clearing existing waste data for re-seeding...");
            _context.WasteTypes.RemoveRange(_context.WasteTypes);
            _context.DisposalProviders.RemoveRange(_context.DisposalProviders);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing waste data cleared");
        }

        await SeedWasteCategoriesAsync();
        await SeedWasteTypesAsync();
        await SeedDisposalProvidersAsync();

        await _context.SaveChangesAsync();
        _logger.LogInformation("Waste management data seeding completed");
    }

    private Task SeedWasteCategoriesAsync()
    {
        _logger.LogInformation("Seeding waste categories...");
        
        // Since WasteCategory in the current codebase is just an enum, 
        // we'll seed WasteType entities instead which represent the actual waste categories
        // This matches the current domain model implementation
        _logger.LogInformation("WasteCategory is an enum in current implementation - skipping category seeding");
        return Task.CompletedTask;
    }

    private async Task SeedWasteTypesAsync()
    {
        _logger.LogInformation("Seeding waste types...");

        const string systemUser = "System";
        
        var wasteTypes = new List<WasteType>
        {
            // Non-Hazardous Waste Types
            WasteType.Create("General Office Waste", "GOW", WasteClassification.NonHazardous, true, systemUser),
            WasteType.Create("Cafeteria Waste", "CAF", WasteClassification.NonHazardous, false, systemUser),
            WasteType.Create("Mixed Municipal Waste", "MMW", WasteClassification.NonHazardous, false, systemUser),

            // Hazardous Chemical Waste Types
            WasteType.Create("Laboratory Solvents", "LSOL", WasteClassification.HazardousChemical, false, systemUser),
            WasteType.Create("Acid Waste", "ACID", WasteClassification.HazardousChemical, false, systemUser),
            WasteType.Create("Base/Alkaline Waste", "BASE", WasteClassification.HazardousChemical, false, systemUser),
            WasteType.Create("Chemical Containers", "CHEM", WasteClassification.HazardousChemical, false, systemUser),

            // Biological/Medical Waste Types
            WasteType.Create("Infectious Waste", "INF", WasteClassification.HazardousBiological, false, systemUser),
            WasteType.Create("Sharps Waste", "SHRP", WasteClassification.HazardousBiological, false, systemUser),
            WasteType.Create("Pathological Waste", "PATH", WasteClassification.HazardousBiological, false, systemUser),

            // Recyclable Materials
            WasteType.Create("Paper and Cardboard", "PAP", WasteClassification.Recyclable, true, systemUser),
            WasteType.Create("Plastic Containers", "PLA", WasteClassification.Recyclable, true, systemUser),
            WasteType.Create("Metal Scrap", "MET", WasteClassification.Recyclable, true, systemUser),
            WasteType.Create("Glass Containers", "GLA", WasteClassification.Recyclable, true, systemUser),

            // Electronic Waste
            WasteType.Create("Computer Equipment", "COMP", WasteClassification.Electronic, true, systemUser),
            WasteType.Create("Mobile Devices", "MOB", WasteClassification.Electronic, true, systemUser),
            WasteType.Create("Monitors and Displays", "DISP", WasteClassification.Electronic, true, systemUser),
            WasteType.Create("Network Equipment", "NET", WasteClassification.Electronic, true, systemUser),

            // Construction Waste
            WasteType.Create("Concrete and Masonry", "CONC", WasteClassification.Construction, true, systemUser),
            WasteType.Create("Wood Waste", "WOOD", WasteClassification.Construction, true, systemUser),
            WasteType.Create("Metal Construction", "MCON", WasteClassification.Construction, true, systemUser),
            WasteType.Create("Drywall and Plaster", "DRY", WasteClassification.Construction, false, systemUser),

            // Organic Waste
            WasteType.Create("Food Scraps", "FOOD", WasteClassification.Organic, true, systemUser),
            WasteType.Create("Yard Waste", "YARD", WasteClassification.Organic, true, systemUser),
            WasteType.Create("Compostable Materials", "COMPOST", WasteClassification.Organic, true, systemUser),

            // Medical Waste
            WasteType.Create("Medical Equipment", "MEDEQ", WasteClassification.Medical, false, systemUser),
            WasteType.Create("Pharmaceutical Waste", "PHARM", WasteClassification.Medical, false, systemUser),

            // Universal Waste
            WasteType.Create("Batteries", "BAT", WasteClassification.Universal, false, systemUser),
            WasteType.Create("Fluorescent Bulbs", "FLUO", WasteClassification.Universal, false, systemUser),
            WasteType.Create("Pesticides", "PEST", WasteClassification.Universal, false, systemUser)
        };

        await _context.WasteTypes.AddRangeAsync(wasteTypes);
        _logger.LogInformation("Seeded {Count} waste types", wasteTypes.Count);
    }

    private async Task SeedDisposalProvidersAsync()
    {
        _logger.LogInformation("Seeding disposal providers...");

        const string systemUser = "System";
        
        var providers = new List<DisposalProvider>
        {
            DisposalProvider.Create("EcoWaste Solutions", "HW-2024-001", DateTime.UtcNow.AddYears(2), systemUser),
            DisposalProvider.Create("HazMat Disposal Specialists", "HW-2024-002", DateTime.UtcNow.AddYears(1), systemUser),
            DisposalProvider.Create("TechRecycle Pro", "ER-2024-003", DateTime.UtcNow.AddYears(3), systemUser),
            DisposalProvider.Create("Green Build Recyclers", "CR-2024-004", DateTime.UtcNow.AddYears(2), systemUser),
            DisposalProvider.Create("BioSafe Medical Waste", "MW-2024-005", DateTime.UtcNow.AddYears(1), systemUser),
            DisposalProvider.Create("Universal Waste Services", "UW-2024-006", DateTime.UtcNow.AddYears(2), systemUser),
            DisposalProvider.Create("Metro Recycling Center", "RC-2024-007", DateTime.UtcNow.AddYears(3), systemUser),
            DisposalProvider.Create("Industrial Waste Management", "IW-2024-008", DateTime.UtcNow.AddYears(2), systemUser)
        };

        await _context.DisposalProviders.AddRangeAsync(providers);
        _logger.LogInformation("Seeded {Count} disposal providers", providers.Count);
    }
}