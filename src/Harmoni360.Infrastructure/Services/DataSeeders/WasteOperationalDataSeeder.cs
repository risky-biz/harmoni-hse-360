using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Waste;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class WasteOperationalDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WasteOperationalDataSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _random = new();

    public WasteOperationalDataSeeder(ApplicationDbContext context, ILogger<WasteOperationalDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting Waste Management operational data seeding...");

            // Get required data
            var users = await _context.Users.ToListAsync();
            var wasteTypes = await _context.WasteTypes.ToListAsync();
            var disposalProviders = await _context.DisposalProviders.ToListAsync();
            var departments = new[] { "Operations", "Maintenance", "Laboratory", "Administration", "Production", "Warehouse" };

            if (!users.Any())
            {
                _logger.LogWarning("Cannot seed Waste operational data - missing users");
                return;
            }

            // Get seeding configuration
            var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
            var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                             (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);

            _logger.LogInformation("WasteOperationalDataSeeder - ForceReseed: {ForceReseed}", forceReseed);

            // Clear existing operational data if force reseed
            if (forceReseed)
            {
                await ClearExistingDataAsync();
            }

            // Check if data already exists
            if (!forceReseed && await _context.WasteReports.AnyAsync())
            {
                _logger.LogInformation("Waste operational data already exists, skipping seeding");
                return;
            }

            // Create operational data
            await SeedWasteReportsAsync(users, departments);
            await SeedWasteDisposalRecordsAsync(users, disposalProviders);
            await SeedWasteComplianceAsync(departments);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Waste Management operational data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding Waste operational data");
            throw;
        }
    }

    private async Task ClearExistingDataAsync()
    {
        _logger.LogInformation("Clearing existing Waste operational data...");
        
        _context.WasteAttachments.RemoveRange(_context.WasteAttachments);
        _context.WasteComments.RemoveRange(_context.WasteComments);
        _context.WasteCompliances.RemoveRange(_context.WasteCompliances);
        _context.WasteDisposalRecords.RemoveRange(_context.WasteDisposalRecords);
        _context.WasteReports.RemoveRange(_context.WasteReports);
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Existing Waste operational data cleared");
    }

    private async Task SeedWasteReportsAsync(List<User> users, string[] departments)
    {
        _logger.LogInformation("Seeding waste reports...");

        var reports = new List<WasteReport>();
        var startDate = DateTime.UtcNow.AddYears(-2);

        // Create 200-300 waste reports over 2 years
        for (int i = 0; i < _random.Next(200, 301); i++)
        {
            var reporter = users[_random.Next(users.Count)];
            var department = departments[_random.Next(departments.Length)];
            var generatedDate = startDate.AddDays(_random.Next(0, 730));

            // Use proper enum values for WasteCategory
            var categories = new[] { WasteCategory.Hazardous, WasteCategory.NonHazardous, WasteCategory.Recyclable };
            var category = categories[_random.Next(categories.Length)];
            var title = GetWasteTitle(category, department);
            var description = GetWasteDescription(category, department);
            var location = GetRandomLocation();

            var report = WasteReport.Create(
                title: title,
                description: description,
                category: category,
                generatedDate: generatedDate,
                location: location,
                reporterId: reporter.Id,
                createdBy: reporter.Name
            );

            // Set random disposal status for some reports to simulate real data
            if (_random.NextDouble() < 0.3) // 30% chance of being in progress or disposed
            {
                var statuses = new[] { WasteDisposalStatus.InProgress, WasteDisposalStatus.Disposed };
                var randomStatus = statuses[_random.Next(statuses.Length)];
                report.UpdateDisposalStatus(randomStatus);
            }
            // Default status is Pending, so 70% will remain pending

            reports.Add(report);
        }

        _context.WasteReports.AddRange(reports);
        await _context.SaveChangesAsync(); // Save reports first to get IDs

        // Add comments to some reports
        await AddWasteCommentsAsync(reports, users);

        _logger.LogInformation($"Seeded {reports.Count} waste reports");
    }

    private async Task SeedWasteDisposalRecordsAsync(List<User> users, List<DisposalProvider> providers)
    {
        _logger.LogInformation("Seeding waste disposal records...");

        if (!providers.Any())
        {
            _logger.LogWarning("No disposal providers found, skipping disposal records");
            return;
        }

        var reports = await _context.WasteReports.ToListAsync();
        var disposalRecords = new List<WasteDisposalRecord>();

        foreach (var report in reports.Take(150)) // Dispose 150 reports
        {
            var provider = providers[_random.Next(providers.Count)];
            var disposalDate = report.GeneratedDate.AddDays(_random.Next(7, 30));
            var quantity = (decimal)(_random.NextDouble() * 1000); // 0-1000 kg
            var unit = GetRandomUnit();

            var record = WasteDisposalRecord.Create(
                wasteReportId: report.Id,
                disposalProviderId: provider.Id,
                disposalDate: disposalDate,
                quantity: quantity,
                unit: unit,
                createdBy: "system"
            );

            disposalRecords.Add(record);
        }

        _context.WasteDisposalRecords.AddRange(disposalRecords);
        _logger.LogInformation($"Seeded {disposalRecords.Count} waste disposal records");
    }

    private async Task SeedWasteComplianceAsync(string[] departments)
    {
        _logger.LogInformation("Seeding waste compliance records...");

        var compliances = new List<WasteCompliance>();
        var startDate = DateTime.UtcNow.AddMonths(-12);

        // Create compliance records for different regulatory bodies
        var regulatoryBodies = new[]
        {
            ("EPA", "40CFR", "Resource Conservation and Recovery Act"),
            ("OSHA", "1910.120", "Hazardous Waste Operations Standard"),
            ("DOT", "49CFR", "Hazardous Materials Transportation"),
            ("Local Authority", "WASTE-001", "Municipal Waste Management")
        };

        foreach (var (body, code, name) in regulatoryBodies)
        {
            var compliance = WasteCompliance.Create(
                body: body,
                code: code,
                name: name,
                createdBy: "system"
            );

            compliances.Add(compliance);
        }

        _context.WasteCompliances.AddRange(compliances);
        _logger.LogInformation($"Seeded {compliances.Count} waste compliance records");
    }

    private async Task AddWasteCommentsAsync(List<WasteReport> reports, List<User> users)
    {
        var comments = new List<WasteComment>();

        foreach (var report in reports.Take(50)) // Add comments to 50 reports
        {
            var commentCount = _random.Next(1, 4);
            for (int i = 0; i < commentCount; i++)
            {
                var commenter = users[_random.Next(users.Count)];
                // Use proper enum values for CommentType
                var commentTypes = Enum.GetValues<CommentType>();
                var commentType = commentTypes[_random.Next(commentTypes.Length)];

                var comment = WasteComment.Create(
                    wasteReportId: report.Id,
                    commentedById: commenter.Id,
                    comment: GetRandomComment(commentType),
                    type: commentType,
                    createdBy: commenter.Name
                );

                comments.Add(comment);
            }
        }

        _context.WasteComments.AddRange(comments);
        _logger.LogInformation($"Added {comments.Count} waste comments");
    }

    private string GetRandomLocation()
    {
        var locations = new[]
        {
            "Production Floor A", "Production Floor B", "Chemical Storage", "Warehouse 1",
            "Warehouse 2", "Laboratory", "Maintenance Shop", "Office Building",
            "Loading Dock", "Treatment Plant", "Storage Yard", "Processing Unit"
        };
        return locations[_random.Next(locations.Length)];
    }

    private UnitOfMeasure GetRandomUnit()
    {
        var units = Enum.GetValues<UnitOfMeasure>();
        return units[_random.Next(units.Length)];
    }

    private string GetWasteTitle(WasteCategory category, string department)
    {
        return category switch
        {
            WasteCategory.Hazardous => $"Hazardous Waste from {department}",
            WasteCategory.Recyclable => $"Recyclable Materials from {department}",
            WasteCategory.NonHazardous => $"Non-Hazardous Waste from {department}",
            _ => $"General Waste from {department}"
        };
    }

    private string GetWasteDescription(WasteCategory category, string department)
    {
        var descriptions = category switch
        {
            WasteCategory.Hazardous => new[]
            {
                $"Hazardous waste from {department} requiring special handling and disposal procedures",
                $"Chemical waste containing hazardous materials - requires certified disposal",
                $"Contaminated materials with hazardous properties from {department} operations"
            },
            WasteCategory.Recyclable => new[]
            {
                $"Recyclable materials from {department} suitable for reprocessing",
                $"Clean materials from {department} for recycling program",
                $"Separated recyclables ready for recycling facility from {department}"
            },
            WasteCategory.NonHazardous => new[]
            {
                $"Non-hazardous waste from {department} for standard disposal",
                $"General office waste from {department} operations",
                $"Regular waste requiring standard processing from {department}"
            },
            _ => new[]
            {
                $"General waste from {department} for standard disposal",
                $"Mixed waste from {department} operations",
                $"Regular waste requiring standard processing from {department}"
            }
        };
        return descriptions[_random.Next(descriptions.Length)];
    }

    private string GetRandomComment(CommentType commentType)
    {
        return commentType switch
        {
            CommentType.General => "Waste properly segregated and labeled according to procedures.",
            CommentType.StatusUpdate => "Status updated - waste collection scheduled for next week.",
            CommentType.ComplianceNote => "All regulatory requirements confirmed and documented.",
            CommentType.DisposalUpdate => "Disposal provider confirmed pickup and processing.",
            CommentType.Correction => "Corrected waste classification based on additional testing.",
            _ => "Standard waste processing comment."
        };
    }
}