using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class PPEItemDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PPEItemDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public PPEItemDataSeeder(ApplicationDbContext context, ILogger<PPEItemDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Check if we should re-seed PPE items even if they exist
        var reSeedPPEItems = _configuration["DataSeeding:ReSeedPPEItems"] == "true";

        if (!reSeedPPEItems && await _context.PPEItems.AnyAsync())
        {
            _logger.LogInformation("PPE items already exist and ReSeedPPEItems is false, skipping PPE items seeding");
            return;
        }

        _logger.LogInformation("Starting PPE items seeding...");

        // If re-seeding is enabled, clear existing items first
        if (reSeedPPEItems && await _context.PPEItems.AnyAsync())
        {
            _logger.LogInformation("Clearing existing PPE items for re-seeding...");
            _context.PPEItems.RemoveRange(_context.PPEItems);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing PPE items cleared");
        }

        // Get reference data
        var categories = await _context.PPECategories.ToListAsync();
        var sizes = await _context.PPESizes.ToListAsync();
        var storageLocations = await _context.PPEStorageLocations.ToListAsync();

        if (!categories.Any() || !sizes.Any() || !storageLocations.Any())
        {
            _logger.LogWarning("Cannot seed PPE items because categories, sizes, or storage locations are missing");
            return;
        }

        // Create PPE items
        var ppeItems = new List<PPEItem>();
        var random = new Random();

        // Helper method to get random element
        T GetRandomElement<T>(List<T> list) => list[random.Next(list.Count)];

        // Seed various PPE items for each category
        foreach (var category in categories)
        {
            var itemCount = random.Next(5, 15); // 5-15 items per category
            
            for (int i = 1; i <= itemCount; i++)
            {
                var size = GetRandomElement(sizes);
                var storageLocation = GetRandomElement(storageLocations);
                
                var itemCode = $"{category.Code}-{i:D3}";
                var purchaseDate = DateTime.UtcNow.AddDays(-random.Next(30, 365));
                var cost = (decimal)(random.NextDouble() * 200 + 50); // $50-$250 range
                
                DateTime? expiryDate = null;
                if (category.RequiresExpiry && category.DefaultExpiryDays.HasValue)
                {
                    expiryDate = purchaseDate.AddDays(category.DefaultExpiryDays.Value);
                }

                var ppeItem = PPEItem.Create(
                    itemCode,
                    $"{category.Name} - Model {i}",
                    $"Standard {category.Name.ToLower()} for {category.Description.ToLower()}",
                    category.Id,
                    GetManufacturerForCategory(category.Type),
                    $"Model-{category.Code}-{i}",
                    purchaseDate,
                    cost,
                    "system",
                    sizeId: size.Id,
                    storageLocationId: storageLocation.Id,
                    location: storageLocation.Name,
                    color: GetRandomColor(),
                    expiryDate: expiryDate,
                    notes: $"Seeded item for {category.Name}"
                );

                // Randomly assign some conditions and statuses
                var conditionChance = random.NextDouble();
                if (conditionChance < 0.7) // 70% new/excellent
                {
                    // Keep as new
                }
                else if (conditionChance < 0.9) // 20% good/fair
                {
                    ppeItem.UpdateCondition(PPECondition.Good, "system");
                }
                else // 10% poor/damaged
                {
                    ppeItem.UpdateCondition(PPECondition.Fair, "system");
                }

                ppeItems.Add(ppeItem);
            }
        }

        await _context.PPEItems.AddRangeAsync(ppeItems);
        _logger.LogInformation("Seeded {Count} PPE items", ppeItems.Count);
    }

    private static string GetManufacturerForCategory(PPEType type) => type switch
    {
        PPEType.HeadProtection => GetRandomElement(new[] { "3M", "MSA", "Bullard", "Honeywell" }),
        PPEType.EyeProtection => GetRandomElement(new[] { "Uvex", "3M", "Oakley", "Pyramex" }),
        PPEType.HearingProtection => GetRandomElement(new[] { "3M", "Howard Leight", "Moldex", "Honeywell" }),
        PPEType.RespiratoryProtection => GetRandomElement(new[] { "3M", "Honeywell", "MSA", "Moldex" }),
        PPEType.HandProtection => GetRandomElement(new[] { "Ansell", "Showa", "HexArmor", "MCR Safety" }),
        PPEType.FootProtection => GetRandomElement(new[] { "Red Wing", "Timberland PRO", "Caterpillar", "Dr. Martens" }),
        PPEType.HighVisibility => GetRandomElement(new[] { "ML Kishigo", "Radians", "OccuNomix", "Portwest" }),
        PPEType.FallProtection => GetRandomElement(new[] { "Miller", "3M DBI-SALA", "Guardian", "MSA" }),
        PPEType.BodyProtection => GetRandomElement(new[] { "DuPont", "Lakeland", "Ansell", "Kimberly-Clark" }),
        PPEType.EmergencyEquipment => GetRandomElement(new[] { "Haws", "Guardian", "Speakman", "Bradley" }),
        _ => "Generic Manufacturer"
    };

    private static string? GetRandomColor()
    {
        var colors = new[] { "Blue", "Yellow", "Orange", "Green", "Red", "White", "Black", "Gray", null };
        var random = new Random();
        return colors[random.Next(colors.Length)];
    }

    private static T GetRandomElement<T>(T[] array)
    {
        var random = new Random();
        return array[random.Next(array.Length)];
    }
}