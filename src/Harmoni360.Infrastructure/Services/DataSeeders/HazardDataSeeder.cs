using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class HazardDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HazardDataSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly IApplicationModeService _applicationModeService;

    public HazardDataSeeder(
        ApplicationDbContext context, 
        ILogger<HazardDataSeeder> logger, 
        IConfiguration configuration,
        IApplicationModeService applicationModeService)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _applicationModeService = applicationModeService;
    }

    public async Task SeedAsync()
    {
        var isDemoMode = _applicationModeService.IsDemoMode;
        _logger.LogInformation("Starting hazard seeding in {Mode} mode...", isDemoMode ? "Demo" : "Production");

        // Note: Hazard categories and types are now seeded by ConfigurationDataSeeder in Essential phase

        // Get seeding configuration
        var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
        var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                         (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);
        var existingHazardCount = await _context.Hazards.CountAsync();
        
        _logger.LogInformation("ForceReseed setting: {ForceReseed}, Existing hazards: {Count}", forceReseed, existingHazardCount);

        if (!forceReseed && existingHazardCount > 0)
        {
            _logger.LogInformation("Hazards already exist ({Count}) and ForceReseed is false, skipping hazard instance seeding", existingHazardCount);
            return;
        }

        // If re-seeding is enabled, clear existing hazards first
        if (forceReseed && existingHazardCount > 0)
        {
            _logger.LogInformation("Clearing {Count} existing hazards for re-seeding...", existingHazardCount);
            
            // Clear related data first to avoid foreign key constraints
            var mitigationActions = await _context.HazardMitigationActions.ToListAsync();
            if (mitigationActions.Any())
            {
                _context.HazardMitigationActions.RemoveRange(mitigationActions);
                _logger.LogInformation("Removed {Count} mitigation actions", mitigationActions.Count);
            }
            
            var riskAssessments = await _context.RiskAssessments.ToListAsync();
            if (riskAssessments.Any())
            {
                _context.RiskAssessments.RemoveRange(riskAssessments);
                _logger.LogInformation("Removed {Count} risk assessments", riskAssessments.Count);
            }
            
            var hazardAttachments = await _context.HazardAttachments.ToListAsync();
            if (hazardAttachments.Any())
            {
                _context.HazardAttachments.RemoveRange(hazardAttachments);
                _logger.LogInformation("Removed {Count} hazard attachments", hazardAttachments.Count);
            }
            
            // Now remove hazards
            var hazards = await _context.Hazards.ToListAsync();
            _context.Hazards.RemoveRange(hazards);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleared {Count} existing hazards successfully", hazards.Count);
        }

        var random = new Random(42); // Fixed seed for consistent demo data
        var departments = new[] { "Chemistry", "Physics", "Biology", "PE", "Maintenance", "Administration", "IT", "Security", "Cafeteria", "Library" };
        
        // Get users for assignments
        var users = await _context.Users.Take(10).ToListAsync();
        if (!users.Any())
        {
            _logger.LogWarning("No users found for hazard seeding. Skipping hazard seeding.");
            return;
        }

        // Determine seeding strategy based on mode
        if (isDemoMode)
        {
            await SeedComprehensiveDemoData(users, departments, random);
        }
        else
        {
            await SeedEssentialConfigurationData(users, departments, random);
        }
    }

    private async Task SeedComprehensiveDemoData(List<User> users, string[] departments, Random random)
    {
        _logger.LogInformation("Seeding comprehensive demo data for Risk Management module...");

        // Get category and type mappings
        var categories = await _context.HazardCategories.ToListAsync();
        var types = await _context.HazardTypes.ToListAsync();
        
        _logger.LogInformation("Found {CategoryCount} categories and {TypeCount} types for seeding", categories.Count, types.Count);
        
        if (!categories.Any())
        {
            _logger.LogWarning("No hazard categories found. Cannot seed hazards without categories.");
            return;
        }
        
        if (!types.Any())
        {
            _logger.LogWarning("No hazard types found. Cannot seed hazards without types.");
            return;
        }
        
        // Comprehensive hazard templates for demo mode
        var hazardTemplates = GetComprehensiveHazardTemplates(categories, types);
        
        var hazards = new List<Hazard>();
        // Generate realistic date spread
        var createdDates = new List<DateTime>();
        for (int i = 0; i < hazardTemplates.Length; i++)
        {
            var daysBack = random.Next(1, 365);
            createdDates.Add(DateTime.UtcNow.AddDays(-daysBack));
        }
        createdDates = createdDates.OrderBy(d => d).ToList();

        for (int i = 0; i < hazardTemplates.Length; i++)
        {
            var template = hazardTemplates[i];
            var user = users[random.Next(users.Count)];
            var department = departments[random.Next(departments.Length)];
            var createdDate = createdDates[i];
            
            // Create realistic geo-locations for 80% of hazards
            GeoLocation? location = null;
            if (random.NextDouble() > 0.2)
            {
                // Generate random campus coordinates (simplified)
                var lat = -6.2088 + (random.NextDouble() - 0.5) * 0.01; // Jakarta area with small variation
                var lng = 106.8456 + (random.NextDouble() - 0.5) * 0.01;
                location = GeoLocation.Create(lat, lng);
            }

            var hazard = Hazard.Create(
                template.Title,
                template.Description,
                template.CategoryId,
                template.TypeId,
                template.Location ?? department,
                template.Severity,
                user.Id,
                department,
                location
            );

            // Set realistic creation date
            var hazardType = typeof(Hazard);
            var createdAtProperty = hazardType.GetProperty("CreatedAt");
            createdAtProperty?.SetValue(hazard, createdDate);

            // Update status for some hazards based on age and severity to ensure all statuses are represented
            var daysOld = (DateTime.UtcNow - createdDate).TotalDays;
            var hazardIndex = hazards.Count; // Use current position to ensure distribution
            
            // Ensure we have examples of all statuses by using a more systematic approach
            if (hazardIndex % 7 == 1) // Every 7th hazard gets UnderAssessment
            {
                hazard.UpdateStatus(HazardStatus.UnderAssessment, "system");
            }
            else if (hazardIndex % 7 == 2) // Every 7th hazard gets ActionRequired
            {
                hazard.UpdateStatus(HazardStatus.ActionRequired, "system");
            }
            else if (hazardIndex % 7 == 3) // Every 7th hazard gets Mitigating
            {
                hazard.UpdateStatus(HazardStatus.Mitigating, "system");
            }
            else if (hazardIndex % 7 == 4) // Every 7th hazard gets Monitoring
            {
                hazard.UpdateStatus(HazardStatus.Monitoring, "system");
            }
            else if (hazardIndex % 7 == 5) // Every 7th hazard gets Resolved
            {
                hazard.UpdateStatus(HazardStatus.Resolved, "system");
            }
            else if (hazardIndex % 7 == 6) // Every 7th hazard gets Closed
            {
                hazard.UpdateStatus(HazardStatus.Closed, "system");
            }
            // else: Keep default Reported status (hazardIndex % 7 == 0)

            hazards.Add(hazard);
        }

        await _context.Hazards.AddRangeAsync(hazards);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} comprehensive demo hazards", hazards.Count);
        
        // Log category distribution for verification
        var categoryDistribution = hazards
            .Where(h => h.CategoryId.HasValue)
            .GroupBy(h => h.CategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());
        _logger.LogInformation("Hazard distribution by CategoryId: {Distribution}", 
            string.Join(", ", categoryDistribution.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
        
        var hazardsWithoutCategory = hazards.Count(h => !h.CategoryId.HasValue);
        if (hazardsWithoutCategory > 0)
        {
            _logger.LogWarning("{Count} hazards were created without a category", hazardsWithoutCategory);
        }

        // Create comprehensive risk assessments with realistic demo data
        await CreateDemoRiskAssessments(hazards, users, random);
        
        // Create extensive mitigation actions (simplified for now)
        // TODO: Implement CreateDemoMitigationActions method
        
        // Create hazard reassessments for some critical hazards (simplified for now)
        // TODO: Implement CreateDemoReassessments method

        // Verify seeding results
        var finalHazardCount = await _context.Hazards.CountAsync();
        var hazardsWithCategories = await _context.Hazards.CountAsync(h => h.CategoryId != null);
        var hazardsWithTypes = await _context.Hazards.CountAsync(h => h.TypeId != null);
        
        _logger.LogInformation("Comprehensive demo data seeding completed for Risk Management module");
        _logger.LogInformation("Final hazard count: {Total}, With categories: {WithCat}, With types: {WithType}", 
            finalHazardCount, hazardsWithCategories, hazardsWithTypes);
        
        // Log a few sample hazards to verify categories
        var sampleHazards = await _context.Hazards
            .Include(h => h.Category)
            .Include(h => h.Type)
            .Take(5)
            .ToListAsync();
            
        foreach (var hazard in sampleHazards)
        {
            _logger.LogInformation("Sample hazard: {Title}, Category: {Category}, Type: {Type}", 
                hazard.Title, 
                hazard.Category?.Name ?? "NULL", 
                hazard.Type?.Name ?? "NULL");
        }
    }

    private async Task SeedEssentialConfigurationData(List<User> users, string[] departments, Random random)
    {
        _logger.LogInformation("Seeding essential configuration data for Risk Management module...");

        // Get category and type mappings
        var categories = await _context.HazardCategories.ToListAsync();
        var types = await _context.HazardTypes.ToListAsync();

        // Essential hazard templates for production mode
        var essentialTemplates = GetEssentialHazardTemplates(categories, types);
        
        var hazards = new List<Hazard>();

        foreach (var template in essentialTemplates)
        {
            var user = users[0]; // Use first user for essential data
            var department = departments[0]; // Use first department

            var hazard = Hazard.Create(
                template.Title,
                template.Description,
                template.CategoryId,
                template.TypeId,
                department,
                template.Severity,
                user.Id,
                department
            );

            hazards.Add(hazard);
        }

        await _context.Hazards.AddRangeAsync(hazards);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} essential configuration hazards", hazards.Count);

        // Create basic risk assessments (simplified for now)
        // TODO: Implement CreateEssentialRiskAssessments method

        _logger.LogInformation("Essential configuration data seeding completed for Risk Management module");
    }

    private dynamic[] GetComprehensiveHazardTemplates(List<Domain.Entities.HazardCategory> categories, List<Domain.Entities.HazardType> types)
    {
        // Helper method to safely get category by code
        int? GetCategoryId(string code)
        {
            var cat = categories.FirstOrDefault(c => c.Code == code);
            if (cat == null)
            {
                _logger.LogWarning("Could not find hazard category with code: {Code}", code);
                return null;
            }
            return cat.Id;
        }

        // Helper method to safely get type by code
        int? GetTypeId(string code)
        {
            var type = types.FirstOrDefault(t => t.Code == code);
            if (type == null)
            {
                _logger.LogWarning("Could not find hazard type with code: {Code}", code);
                return null;
            }
            return type.Id;
        }

        // Get category and type IDs by codes/names with safety checks
        var physicalCat = GetCategoryId("PHYS");
        var chemicalCat = GetCategoryId("CHEM");
        var biologicalCat = GetCategoryId("BIO");
        var ergonomicCat = GetCategoryId("ERGO");
        var psychologicalCat = GetCategoryId("PSYCH");
        var environmentalCat = GetCategoryId("ENV");
        var fireCat = GetCategoryId("FIRE");
        var electricalCat = GetCategoryId("ELEC");
        var mechanicalCat = GetCategoryId("MECH");
        var radiationCat = GetCategoryId("RAD");

        var slipType = GetTypeId("SLIP");
        var tripType = GetTypeId("TRIP");
        var fallType = GetTypeId("FALL");
        var cutType = GetTypeId("CUT");
        var burnType = GetTypeId("BURN");
        var exposureType = GetTypeId("EXPO");
        var spillType = GetTypeId("SPILL");
        var collisionType = GetTypeId("COLL");
        var entrapmentType = GetTypeId("ENTR");
        var explosionType = GetTypeId("EXPL");
        var fireType = GetTypeId("FIRE_T");
        var otherType = GetTypeId("OTHER");

        return new dynamic[]
        {
            // Physical Hazards - All Severities
            new { Title = "Slippery floors in chemistry laboratory", Description = "Chemical spills and water on laboratory floors create significant slip hazards for students and staff", CategoryId = physicalCat, TypeId = slipType, Severity = HazardSeverity.Moderate, Location = "Chemistry Lab Block A" },
            new { Title = "Uneven pavement in main walkway", Description = "Cracked and uneven concrete pavement poses trip hazards, especially during rainy season", CategoryId = physicalCat, TypeId = tripType, Severity = HazardSeverity.Minor, Location = "Main Campus Walkway" },
            new { Title = "Broken handrail on staircase", Description = "Loose handrail on main building staircase poses fall risk for students and visitors", CategoryId = physicalCat, TypeId = fallType, Severity = HazardSeverity.Major, Location = "Main Building Staircase" },
            new { Title = "Structural collapse risk in old building", Description = "Critical structural damage discovered in foundation of heritage building", CategoryId = physicalCat, TypeId = fallType, Severity = HazardSeverity.Catastrophic, Location = "Heritage Building East Wing" },
            new { Title = "Minor floor scratches in hallway", Description = "Small scratches on polished floor surface causing minimal risk", CategoryId = physicalCat, TypeId = slipType, Severity = HazardSeverity.Negligible, Location = "Main Hallway" },
            
            // Chemical Hazards - All Severities
            new { Title = "Improper chemical storage in lab", Description = "Incompatible chemicals stored together in laboratory storage cabinet", CategoryId = chemicalCat, TypeId = exposureType, Severity = HazardSeverity.Major, Location = "Chemistry Laboratory" },
            new { Title = "Leaking chemical containers", Description = "Several chemical containers showing signs of deterioration and minor leaks", CategoryId = chemicalCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Chemical Storage Room" },
            new { Title = "Missing safety data sheets", Description = "Safety data sheets missing for newly acquired laboratory chemicals", CategoryId = chemicalCat, TypeId = exposureType, Severity = HazardSeverity.Minor, Location = "Chemistry Prep Room" },
            new { Title = "Major chemical leak emergency", Description = "Significant chemical leak requiring immediate evacuation and specialized cleanup", CategoryId = chemicalCat, TypeId = spillType, Severity = HazardSeverity.Catastrophic, Location = "Chemical Storage Facility" },
            new { Title = "Empty chemical bottle disposal", Description = "Properly cleaned empty bottles ready for standard disposal", CategoryId = chemicalCat, TypeId = otherType, Severity = HazardSeverity.Negligible, Location = "Chemistry Prep Room" },
            
            // Biological Hazards - All Severities
            new { Title = "Mold growth in basement storage", Description = "Visible mold growth in basement storage area due to moisture issues", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Basement Storage" },
            new { Title = "Food contamination risk in cafeteria", Description = "Refrigeration units not maintaining consistent temperatures for food safety", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Major, Location = "School Cafeteria" },
            new { Title = "Infectious disease outbreak risk", Description = "Multiple confirmed cases requiring immediate quarantine protocols", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Catastrophic, Location = "School Campus Wide" },
            new { Title = "Dust accumulation on shelves", Description = "Normal dust buildup requiring routine cleaning", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Negligible, Location = "Library Storage" },
            new { Title = "Bacterial growth in water fountain", Description = "Detected bacterial growth requiring sanitization", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Minor, Location = "Main Corridor Water Fountain" },
            
            // Fire Hazards - All Severities  
            new { Title = "Blocked fire exit in library", Description = "Emergency fire exit blocked by stored furniture and equipment", CategoryId = fireCat, TypeId = fireType, Severity = HazardSeverity.Major, Location = "School Library" },
            new { Title = "Faulty fire extinguisher in kitchen", Description = "Fire extinguisher in kitchen area showing pressure gauge in red zone", CategoryId = fireCat, TypeId = fireType, Severity = HazardSeverity.Major, Location = "Cafeteria Kitchen" },
            new { Title = "Gas leak near laboratory burners", Description = "Detected gas leak creating imminent explosion risk", CategoryId = fireCat, TypeId = explosionType, Severity = HazardSeverity.Catastrophic, Location = "Chemistry Laboratory" },
            new { Title = "Cigarette butt in trash bin", Description = "Improperly disposed cigarette butt with minimal fire risk", CategoryId = fireCat, TypeId = fireType, Severity = HazardSeverity.Minor, Location = "Outdoor Smoking Area" },
            new { Title = "Burnt out light bulb", Description = "Non-functioning light bulb with no safety risk", CategoryId = fireCat, TypeId = otherType, Severity = HazardSeverity.Negligible, Location = "Storage Closet" },
            
            // Electrical Hazards - All Severities
            new { Title = "Exposed electrical cables in corridor", Description = "Electrical cables running along corridor floor pose electrocution and trip hazards", CategoryId = electricalCat, TypeId = otherType, Severity = HazardSeverity.Major, Location = "Science Block Corridor" },
            new { Title = "Overloaded power outlets in computer lab", Description = "Multiple high-power devices connected to single outlet creating fire risk", CategoryId = electricalCat, TypeId = otherType, Severity = HazardSeverity.Moderate, Location = "Computer Laboratory" },
            new { Title = "Damaged electrical panel", Description = "Main electrical panel showing signs of severe damage and sparking", CategoryId = electricalCat, TypeId = otherType, Severity = HazardSeverity.Catastrophic, Location = "Main Electrical Room" },
            new { Title = "Loose light switch cover", Description = "Light switch cover slightly loose but fully functional", CategoryId = electricalCat, TypeId = otherType, Severity = HazardSeverity.Negligible, Location = "Classroom 201" },
            new { Title = "Flickering fluorescent light", Description = "Fluorescent light occasionally flickering, may need tube replacement", CategoryId = electricalCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "Teacher's Lounge" },
            
            // Mechanical Hazards - All Severities
            new { Title = "Loose playground equipment bolts", Description = "Several playground equipment pieces showing loose bolts and connections", CategoryId = mechanicalCat, TypeId = fallType, Severity = HazardSeverity.Major, Location = "Primary Playground" },
            new { Title = "Sharp edges on metal furniture", Description = "Metal tables and chairs with sharp edges pose cutting hazards", CategoryId = mechanicalCat, TypeId = cutType, Severity = HazardSeverity.Minor, Location = "Outdoor Seating Area" },
            new { Title = "Elevator mechanical failure", Description = "Critical elevator mechanism failure creating entrapment risk", CategoryId = mechanicalCat, TypeId = entrapmentType, Severity = HazardSeverity.Catastrophic, Location = "Main Building Elevator" },
            new { Title = "Squeaky door hinge", Description = "Door hinge making noise but functioning normally", CategoryId = mechanicalCat, TypeId = otherType, Severity = HazardSeverity.Negligible, Location = "Library Entrance" },
            new { Title = "Loose desk drawer handle", Description = "Desk drawer handle slightly loose but secure", CategoryId = mechanicalCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "Administrative Office" },
            
            // Environmental Hazards - All Severities
            new { Title = "Air quality issues in art classroom", Description = "Poor ventilation in art classroom during use of solvents and paints", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Art Classroom" },
            new { Title = "Excessive heat in computer lab", Description = "Computer laboratory experiencing overheating due to inadequate air conditioning", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Computer Laboratory" },
            new { Title = "Severe air pollution from nearby construction", Description = "Dangerous air quality levels requiring immediate campus closure", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Catastrophic, Location = "Entire Campus" },
            new { Title = "Mild temperature fluctuation in classroom", Description = "Classroom temperature slightly above optimal range", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Negligible, Location = "Classroom 105" },
            new { Title = "Outdated air filter in HVAC system", Description = "Air filter needs replacement for optimal air quality", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Minor, Location = "HVAC Maintenance Room" },
            
            // Ergonomic Hazards - All Severities
            new { Title = "Poor posture from uncomfortable chairs", Description = "Student chairs causing back strain and poor posture", CategoryId = ergonomicCat, TypeId = otherType, Severity = HazardSeverity.Moderate, Location = "Classroom 301" },
            new { Title = "Repetitive strain from computer use", Description = "Students experiencing wrist pain from extended computer use", CategoryId = ergonomicCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "Computer Laboratory" },
            new { Title = "Severe back injury risk from heavy lifting", Description = "Staff required to lift equipment exceeding safe weight limits", CategoryId = ergonomicCat, TypeId = otherType, Severity = HazardSeverity.Major, Location = "Maintenance Storage" },
            new { Title = "Workplace stress epidemic", Description = "Campus-wide stress levels creating severe health risks", CategoryId = psychologicalCat, TypeId = otherType, Severity = HazardSeverity.Catastrophic, Location = "Campus Wide" },
            new { Title = "Slightly uncomfortable desk height", Description = "Desk height marginally below optimal ergonomic position", CategoryId = ergonomicCat, TypeId = otherType, Severity = HazardSeverity.Negligible, Location = "Administration Office" },
            
            // Psychological Hazards - All Severities
            new { Title = "Workplace bullying incident", Description = "Reported workplace harassment creating hostile environment", CategoryId = psychologicalCat, TypeId = otherType, Severity = HazardSeverity.Major, Location = "Staff Break Room" },
            new { Title = "High stress during exam period", Description = "Elevated stress levels among students during examination period", CategoryId = psychologicalCat, TypeId = otherType, Severity = HazardSeverity.Moderate, Location = "Examination Halls" },
            new { Title = "Mental health crisis", Description = "Multiple serious mental health incidents requiring immediate intervention", CategoryId = psychologicalCat, TypeId = otherType, Severity = HazardSeverity.Catastrophic, Location = "Student Counseling Center" },
            new { Title = "Minor workload increase", Description = "Slight increase in daily tasks causing minimal stress", CategoryId = psychologicalCat, TypeId = otherType, Severity = HazardSeverity.Negligible, Location = "Teacher Preparation Room" },
            new { Title = "Occasional noisy distractions", Description = "Periodic noise disruptions affecting concentration", CategoryId = psychologicalCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "Study Hall" },
            
            // Radiation Hazards - All Severities
            new { Title = "X-ray machine radiation leak", Description = "Medical X-ray equipment showing radiation leakage above safe limits", CategoryId = radiationCat, TypeId = exposureType, Severity = HazardSeverity.Catastrophic, Location = "Medical Center" },
            new { Title = "UV exposure in science lab", Description = "UV lamps without proper shielding exposing students to harmful radiation", CategoryId = radiationCat, TypeId = exposureType, Severity = HazardSeverity.Major, Location = "Biology Laboratory" },
            new { Title = "Microwave radiation from WiFi", Description = "High-density WiFi installation causing elevated electromagnetic exposure", CategoryId = radiationCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Computer Laboratory" },
            new { Title = "Outdated radiation monitoring badge", Description = "Personal radiation monitoring equipment needing calibration", CategoryId = radiationCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "Physics Laboratory" },
            new { Title = "Computer screen blue light", Description = "Standard computer monitor blue light emission within normal ranges", CategoryId = radiationCat, TypeId = exposureType, Severity = HazardSeverity.Negligible, Location = "Administrative Office" },
            
            // Additional Physical Hazards for better coverage
            new { Title = "Wet floor without warning signs", Description = "Recently mopped floor area lacks proper warning signage", CategoryId = physicalCat, TypeId = slipType, Severity = HazardSeverity.Moderate, Location = "Main Entrance" },
            new { Title = "Damaged ceiling tile falling", Description = "Loose ceiling tiles pose head injury risk", CategoryId = physicalCat, TypeId = fallType, Severity = HazardSeverity.Major, Location = "Classroom 102" },
            new { Title = "Knife safety in cooking class", Description = "Sharp kitchen knives requiring proper handling protocols", CategoryId = physicalCat, TypeId = cutType, Severity = HazardSeverity.Moderate, Location = "Culinary Arts Kitchen" },
            new { Title = "Hot surfaces in laboratory", Description = "Laboratory hot plates and heating equipment without guards", CategoryId = physicalCat, TypeId = burnType, Severity = HazardSeverity.Moderate, Location = "Chemistry Laboratory" },
            new { Title = "Falling books from high shelves", Description = "Unstable book storage on tall library shelves", CategoryId = physicalCat, TypeId = fallType, Severity = HazardSeverity.Minor, Location = "Library Reference Section" },
            
            // Additional Chemical Hazards
            new { Title = "Acid spill containment failure", Description = "Acid storage containment system showing signs of deterioration", CategoryId = chemicalCat, TypeId = spillType, Severity = HazardSeverity.Major, Location = "Chemical Storage Room" },
            new { Title = "Paint fume exposure", Description = "Art classroom paint activities without adequate ventilation", CategoryId = chemicalCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Art Classroom" },
            new { Title = "Cleaning chemical mixing", Description = "Risk of dangerous chemical reactions from improper cleaning product storage", CategoryId = chemicalCat, TypeId = exposureType, Severity = HazardSeverity.Major, Location = "Custodial Storage" },
            new { Title = "Hand sanitizer alcohol vapors", Description = "High concentration of alcohol-based sanitizers in enclosed space", CategoryId = chemicalCat, TypeId = exposureType, Severity = HazardSeverity.Minor, Location = "School Entrance" },
            
            // Additional Biological Hazards
            new { Title = "Contaminated drinking water", Description = "Water quality tests showing bacterial contamination", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Major, Location = "Water Fountains Campus-wide" },
            new { Title = "Pest infestation in cafeteria", Description = "Evidence of rodent activity in food preparation areas", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Major, Location = "Cafeteria Kitchen" },
            new { Title = "Mold in air conditioning vents", Description = "Visible mold growth in HVAC system affecting air quality", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Classroom HVAC System" },
            new { Title = "Bird droppings on outdoor equipment", Description = "Accumulated bird waste on playground equipment", CategoryId = biologicalCat, TypeId = exposureType, Severity = HazardSeverity.Minor, Location = "Outdoor Playground" },
            
            // Additional Fire/Explosion Hazards
            new { Title = "Overloaded electrical circuits", Description = "Electrical panels showing signs of overload and overheating", CategoryId = fireCat, TypeId = fireType, Severity = HazardSeverity.Major, Location = "Main Electrical Room" },
            new { Title = "Combustible material storage", Description = "Flammable materials stored too close to heat sources", CategoryId = fireCat, TypeId = fireType, Severity = HazardSeverity.Moderate, Location = "Maintenance Storage" },
            new { Title = "Propane tank leak", Description = "Laboratory propane tanks showing signs of gas leakage", CategoryId = fireCat, TypeId = explosionType, Severity = HazardSeverity.Major, Location = "Science Laboratory" },
            new { Title = "Candle safety in ceremonies", Description = "Open flame use during school ceremonies requiring fire safety protocols", CategoryId = fireCat, TypeId = fireType, Severity = HazardSeverity.Minor, Location = "School Auditorium" },
            
            // Additional Environmental Hazards
            new { Title = "Noise pollution from construction", Description = "Nearby construction creating excessive noise levels", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Classrooms Facing Construction" },
            new { Title = "Poor lighting in study areas", Description = "Inadequate lighting causing eye strain and reduced visibility", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Minor, Location = "Library Study Carrels" },
            new { Title = "Extreme weather exposure", Description = "Outdoor activities during severe weather conditions", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Major, Location = "Sports Fields" },
            new { Title = "Dust from renovation work", Description = "Construction dust affecting indoor air quality", CategoryId = environmentalCat, TypeId = exposureType, Severity = HazardSeverity.Moderate, Location = "Renovation Zone" },
            
            // Additional scenarios to ensure comprehensive status coverage
            new { Title = "Security camera blind spot", Description = "Areas of campus not covered by security monitoring", CategoryId = physicalCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "Parking Lot Section C" },
            new { Title = "Emergency exit inspection needed", Description = "Quarterly emergency exit functionality check due", CategoryId = fireCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "All Buildings" },
            new { Title = "First aid kit expiration", Description = "Medical supplies in first aid stations past expiration date", CategoryId = biologicalCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "Nurse's Office" },
            new { Title = "Lab safety equipment calibration", Description = "Safety showers and eyewash stations requiring annual testing", CategoryId = chemicalCat, TypeId = otherType, Severity = HazardSeverity.Moderate, Location = "Science Laboratories" },
            new { Title = "Fire drill coordination needed", Description = "Monthly fire drill planning and execution", CategoryId = fireCat, TypeId = otherType, Severity = HazardSeverity.Minor, Location = "Campus Wide" }
        };
    }

    private dynamic[] GetEssentialHazardTemplates(List<Domain.Entities.HazardCategory> categories, List<Domain.Entities.HazardType> types)
    {
        // Get basic category and type IDs for essential data
        var physicalCat = categories.First(c => c.Code == "PHYS").Id;
        var chemicalCat = categories.First(c => c.Code == "CHEM").Id;
        var fireCat = categories.First(c => c.Code == "FIRE").Id;

        var slipType = types.First(t => t.Code == "SLIP").Id;
        var exposureType = types.First(t => t.Code == "EXPO").Id;
        var fireType = types.First(t => t.Code == "FIRE_T").Id;

        return new dynamic[]
        {
            new { Title = "Sample slip hazard", Description = "Basic slip hazard for system configuration", CategoryId = physicalCat, TypeId = slipType, Severity = HazardSeverity.Moderate },
            new { Title = "Sample chemical exposure", Description = "Basic chemical exposure for system configuration", CategoryId = chemicalCat, TypeId = exposureType, Severity = HazardSeverity.Major },
            new { Title = "Sample fire hazard", Description = "Basic fire hazard for system configuration", CategoryId = fireCat, TypeId = fireType, Severity = HazardSeverity.Major }
        };
    }

    /// <summary>
    /// Creates comprehensive demo risk assessments following industry standards (ISO 31000, 5x5 matrix)
    /// </summary>
    private async Task CreateDemoRiskAssessments(List<Hazard> hazards, List<User> users, Random random)
    {
        _logger.LogInformation("Creating comprehensive demo risk assessments using industry-standard methodology...");

        var riskAssessments = new List<RiskAssessment>();

        // Risk assessment templates based on industry standards
        var assessmentTemplates = GetRiskAssessmentTemplates();

        // Create risk assessments for approximately 75% of hazards (realistic coverage)
        var hazardsToAssess = hazards.Take((int)(hazards.Count * 0.75)).ToList();

        foreach (var hazard in hazardsToAssess)
        {
            // Select appropriate template based on hazard severity
            var template = SelectRiskAssessmentTemplate(hazard.Severity, assessmentTemplates, random);
            
            // Select random assessor (risk managers, safety officers, supervisors)
            var assessor = users[random.Next(users.Count)];
            
            // Calculate assessment date (within last 90 days)
            var assessmentDate = DateTime.UtcNow.AddDays(-random.Next(1, 90));
            
            // Create risk assessment using industry-standard 5x5 matrix
            var riskAssessment = RiskAssessment.Create(
                hazardId: hazard.Id,
                type: template.Type,
                assessorId: assessor.Id,
                probabilityScore: template.ProbabilityScore,
                severityScore: template.SeverityScore,
                potentialConsequences: template.PotentialConsequences,
                existingControls: template.ExistingControls,
                recommendedActions: template.RecommendedActions,
                additionalNotes: template.AdditionalNotes
            );

            // Set the assessment date manually since Create doesn't accept it
            // This is acceptable for demo data seeding
            var field = typeof(RiskAssessment).GetField("_assessmentDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                // If reflection doesn't work, use a property if available
                var property = typeof(RiskAssessment).GetProperty("AssessmentDate");
                if (property?.CanWrite == true)
                {
                    property.SetValue(riskAssessment, assessmentDate);
                }
            }
            else
            {
                field.SetValue(riskAssessment, assessmentDate);
            }

            // Apply approval workflow for some assessments (60% approval rate)
            if (random.NextDouble() < 0.6)
            {
                var approver = users[random.Next(users.Count)];
                var approvalNotes = GetApprovalNotes(riskAssessment.RiskLevel, random);
                
                riskAssessment.Approve(approver.Id, approvalNotes);
            }

            riskAssessments.Add(riskAssessment);
        }

        // Add some historical assessments for the same hazards (showing assessment evolution)
        var criticalHazards = hazards.Where(h => h.Severity == HazardSeverity.Major || h.Severity == HazardSeverity.Catastrophic).Take(10);
        
        foreach (var hazard in criticalHazards)
        {
            // Create an older assessment to show progression
            var oldAssessment = await CreateHistoricalRiskAssessment(hazard, users, random);
            if (oldAssessment != null)
            {
                riskAssessments.Add(oldAssessment);
            }
        }

        await _context.RiskAssessments.AddRangeAsync(riskAssessments);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created {Count} demo risk assessments", riskAssessments.Count);
        
        // Log statistics
        var approvedCount = riskAssessments.Count(ra => ra.IsApproved);
        var riskLevelDistribution = riskAssessments
            .GroupBy(ra => ra.RiskLevel)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());
            
        _logger.LogInformation("Risk assessment statistics - Approved: {Approved}/{Total}, Risk levels: {Distribution}", 
            approvedCount, riskAssessments.Count, 
            string.Join(", ", riskLevelDistribution.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
    }

    /// <summary>
    /// Risk assessment templates based on industry best practices and ISO 31000 standards
    /// </summary>
    private List<RiskAssessmentTemplate> GetRiskAssessmentTemplates()
    {
        return new List<RiskAssessmentTemplate>
        {
            // Low Risk Templates (1-4 on 5x5 matrix)
            new RiskAssessmentTemplate
            {
                Type = RiskAssessmentType.General,
                ProbabilityScore = 1, // Very Unlikely
                SeverityScore = 2, // Minor
                PotentialConsequences = "Minor injuries requiring basic first aid treatment. Minimal disruption to operations. Low financial impact under $1,000.",
                ExistingControls = "Standard safety procedures in place. Regular maintenance schedule. Basic PPE available. Staff awareness training completed.",
                RecommendedActions = "Continue monitoring. Maintain existing controls. Review during scheduled inspections. Update training materials as needed.",
                ReviewMonths = 12,
                AdditionalNotes = "Risk level acceptable under ALARP principle. Continue current management approach with periodic review."
            },
            
            // Medium Risk Templates (5-9 on 5x5 matrix)
            new RiskAssessmentTemplate
            {
                Type = RiskAssessmentType.HIRA,
                ProbabilityScore = 3, // Possible
                SeverityScore = 3, // Moderate
                PotentialConsequences = "Moderate injuries requiring medical attention. Potential for work absence up to 7 days. Financial impact $1,000-$10,000. Some operational disruption.",
                ExistingControls = "Enhanced safety procedures implemented. Regular equipment inspections. Intermediate PPE required. Supervisor oversight during high-risk activities.",
                RecommendedActions = "Implement additional engineering controls. Enhance training programs. Increase inspection frequency. Consider upgraded PPE. Develop specific procedures for high-risk scenarios.",
                ReviewMonths = 6,
                AdditionalNotes = "Monitor for trends. Consider cost-benefit analysis for additional controls. Ensure controls are maintained and effective."
            },
            
            // High Risk Templates (10-16 on 5x5 matrix)
            new RiskAssessmentTemplate
            {
                Type = RiskAssessmentType.JSA,
                ProbabilityScore = 4, // Likely
                SeverityScore = 4, // Major
                PotentialConsequences = "Serious injuries requiring hospitalization. Potential for permanent disability. Extended work absence. Financial impact $10,000-$100,000. Significant operational disruption.",
                ExistingControls = "Comprehensive safety management system. Detailed job safety analysis. Specialized PPE mandatory. Permit-to-work system. Emergency response procedures.",
                RecommendedActions = "Immediate implementation of additional controls. Consider elimination or substitution of hazard. Implement administrative controls. Enhance emergency response. Increase supervision levels.",
                ReviewMonths = 3,
                AdditionalNotes = "Requires immediate attention. Consider work stoppage until additional controls implemented. Senior management approval required for continued operations."
            },
            
            // Critical Risk Templates (17-25 on 5x5 matrix)
            new RiskAssessmentTemplate
            {
                Type = RiskAssessmentType.Environmental,
                ProbabilityScore = 5, // Almost Certain
                SeverityScore = 5, // Catastrophic
                PotentialConsequences = "Fatality or multiple serious injuries. Permanent total disability. Major environmental impact. Financial impact exceeding $100,000. Complete operational shutdown.",
                ExistingControls = "Emergency protocols activated. Work area isolated. Specialized emergency equipment deployed. Expert consultation obtained. Regulatory authorities notified.",
                RecommendedActions = "IMMEDIATE ACTION REQUIRED. Stop work until risks eliminated. Implement hierarchy of controls. Expert assessment mandatory. Regulatory compliance verification. Management of change procedures.",
                ReviewMonths = 1,
                AdditionalNotes = "CRITICAL RISK - Unacceptable risk level. Work must not proceed until risk reduced to acceptable level. Requires board-level approval for any continued operations."
            },
            
            // Fire Safety Specific
            new RiskAssessmentTemplate
            {
                Type = RiskAssessmentType.Fire,
                ProbabilityScore = 2, // Unlikely
                SeverityScore = 4, // Major
                PotentialConsequences = "Potential for fire/explosion with serious injuries. Property damage. Emergency evacuation required. Business continuity impact.",
                ExistingControls = "Fire detection and suppression systems. Emergency evacuation procedures. Fire safety training. Hot work permits. Hazardous material controls.",
                RecommendedActions = "Enhance fire prevention measures. Upgrade detection systems. Increase inspection frequency. Review evacuation procedures. Consider sprinkler system upgrades.",
                ReviewMonths = 6,
                AdditionalNotes = "Fire risk requires special attention due to potential for rapid escalation. Ensure emergency response readiness."
            }
        };
    }

    private RiskAssessmentTemplate SelectRiskAssessmentTemplate(HazardSeverity hazardSeverity, List<RiskAssessmentTemplate> templates, Random random)
    {
        // Select template based on hazard severity to ensure realistic risk assessment alignment
        return hazardSeverity switch
        {
            HazardSeverity.Negligible => templates[0], // Low risk template
            HazardSeverity.Minor => random.NextDouble() < 0.7 ? templates[0] : templates[1], // Mostly low, some medium
            HazardSeverity.Moderate => templates[1], // Medium risk template
            HazardSeverity.Major => random.NextDouble() < 0.6 ? templates[2] : templates[1], // Mostly high, some medium
            HazardSeverity.Catastrophic => templates[3], // Critical risk template
            _ => templates[1] // Default to medium risk
        };
    }

    private async Task<RiskAssessment?> CreateHistoricalRiskAssessment(Hazard hazard, List<User> users, Random random)
    {
        try
        {
            var assessor = users[random.Next(users.Count)];
            var oldAssessmentDate = DateTime.UtcNow.AddDays(-random.Next(180, 365)); // 6-12 months ago
            
            // Create a historical assessment with higher risk scores to show improvement over time
            var historicalAssessment = RiskAssessment.Create(
                hazardId: hazard.Id,
                type: RiskAssessmentType.General,
                assessorId: assessor.Id,
                probabilityScore: Math.Min(5, random.Next(3, 6)), // Higher probability in the past
                severityScore: Math.Min(5, random.Next(3, 6)), // Higher severity in the past
                potentialConsequences: "Historical assessment showing higher risk before controls were implemented.",
                existingControls: "Basic controls in place. Limited safety measures. Reactive approach to risk management.",
                recommendedActions: "Implement comprehensive risk controls. Develop proactive safety measures. Enhance training programs.",
                additionalNotes: "Initial assessment - risks have since been reduced through implementation of recommended controls."
            );

            // Set the historical assessment date manually
            var property = typeof(RiskAssessment).GetProperty("AssessmentDate");
            if (property?.CanWrite == true)
            {
                property.SetValue(historicalAssessment, oldAssessmentDate);
            }

            // Mark as inactive since this is a historical assessment
            historicalAssessment.Deactivate("Superseded by updated risk assessment");

            return historicalAssessment;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create historical risk assessment for hazard {HazardId}", hazard.Id);
            return null;
        }
    }

    private string GetApprovalNotes(RiskAssessmentLevel riskLevel, Random random)
    {
        var approvalNotes = riskLevel switch
        {
            RiskAssessmentLevel.VeryLow => new[] 
            {
                "Risk assessment approved. Current controls are adequate. Continue monitoring.",
                "Acceptable risk level. No additional actions required at this time.",
                "Assessment methodology appropriate. Risk level within acceptable parameters."
            },
            RiskAssessmentLevel.Low => new[]
            {
                "Risk assessment approved with minor recommendations noted for future review.",
                "Controls adequate for current risk level. Consider enhancements during next review cycle.",
                "Approved. Monitor effectiveness of existing controls."
            },
            RiskAssessmentLevel.Medium => new[]
            {
                "Risk assessment approved. Recommend implementing suggested additional controls within 30 days.",
                "Conditional approval. Progress on recommended actions to be reviewed monthly.",
                "Approved with requirement for enhanced monitoring and control implementation."
            },
            RiskAssessmentLevel.High => new[]
            {
                "Risk assessment approved. Immediate implementation of recommended controls required.",
                "Approved with mandatory implementation timeline. Senior management oversight required.",
                "Critical controls must be implemented before continued operations approved."
            },
            RiskAssessmentLevel.Critical => new[]
            {
                "Emergency approval pending immediate risk reduction measures.",
                "Operations suspended until critical controls implemented and verified.",
                "Board-level approval required. Comprehensive action plan mandatory."
            },
            _ => new[] { "Risk assessment reviewed and approved by authorized personnel." }
        };

        return approvalNotes[random.Next(approvalNotes.Length)];
    }

    /// <summary>
    /// Template for creating standardized risk assessments
    /// </summary>
    private class RiskAssessmentTemplate
    {
        public RiskAssessmentType Type { get; set; }
        public int ProbabilityScore { get; set; }
        public int SeverityScore { get; set; }
        public string PotentialConsequences { get; set; } = string.Empty;
        public string ExistingControls { get; set; } = string.Empty;
        public string RecommendedActions { get; set; } = string.Empty;
        public int ReviewMonths { get; set; }
        public string AdditionalNotes { get; set; } = string.Empty;
    }

}
