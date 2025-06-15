using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services;

public class ConfigurationDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConfigurationDataSeeder> _logger;
    private readonly bool _isDemoMode;

    public ConfigurationDataSeeder(
        ApplicationDbContext context,
        ILogger<ConfigurationDataSeeder> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _isDemoMode = bool.TryParse(configuration["Application:DemoMode"], out var demoMode) && demoMode;
    }

    public async Task SeedAsync()
    {
        await SeedAsync(false);
    }

    public async Task SeedAsync(bool forceReseed)
    {
        _logger.LogInformation("Starting configuration data seeding for ALL modules in {Mode} mode", _isDemoMode ? "Demo" : "Production");

        try
        {
            await SeedDepartmentsAsync(forceReseed);
            await SeedIncidentCategoriesAsync(forceReseed);
            await SeedIncidentLocationsAsync(forceReseed);
            await SeedHazardConfigurationAsync(forceReseed);
            await SeedPPEConfigurationAsync(forceReseed);

            _logger.LogInformation("Configuration data seeding completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration data seeding");
            throw;
        }
    }

    private async Task SeedDepartmentsAsync(bool forceReseed = false)
    {
        var existingCount = await _context.Departments.CountAsync();
        
        _logger.LogInformation("SeedDepartments - ExistingCount: {Count}, ForceReseed: {ForceReseed}", 
            existingCount, forceReseed);
        
        // Skip seeding only if data already exists AND forceReseed is not enabled
        if (existingCount > 0 && !forceReseed)
        {
            _logger.LogInformation("Departments already exist ({Count} records), skipping seeding", existingCount);
            return;
        }
        
        _logger.LogInformation("Starting departments seeding...");

        var departments = new List<Department>
        {
            Department.Create("Academic", "ACAD", "Academic departments including teaching staff and educational support", "Dr. Sarah Johnson", "sarah.johnson@bsj.edu", "Academic Block A", 1, true),
            Department.Create("Operations", "OPS", "Facilities management, maintenance, and operational support", "Michael Chen", "michael.chen@bsj.edu", "Administration Building", 2, true),
            Department.Create("Security", "SEC", "Campus security, safety coordination, and emergency response", "James Rodriguez", "james.rodriguez@bsj.edu", "Security Office", 3, true),
            Department.Create("Information Technology", "IT", "Technology infrastructure, systems administration, and user support", "Priya Sharma", "priya.sharma@bsj.edu", "IT Center", 4, true),
            Department.Create("Human Resources", "HR", "Staff recruitment, training, compliance, and employee relations", "Lisa Wang", "lisa.wang@bsj.edu", "Administration Building", 5, true),
            Department.Create("Health Services", "HEALTH", "Medical services, health monitoring, and wellness programs", "Dr. Ahmad Hassan", "ahmad.hassan@bsj.edu", "Health Center", 6, true),
            Department.Create("Food Services", "FOOD", "Cafeteria operations, food safety, and nutrition programs", "Roberto Silva", "roberto.silva@bsj.edu", "Cafeteria", 7, true),
            Department.Create("Student Services", "STU", "Student support, activities, and welfare coordination", "Emma Thompson", "emma.thompson@bsj.edu", "Student Center", 8, true),
            Department.Create("Maintenance", "MAINT", "Building maintenance, repairs, and preventive maintenance programs", "David Kim", "david.kim@bsj.edu", "Maintenance Workshop", 9, true),
            Department.Create("Finance", "FIN", "Financial operations, procurement, and budget management", "Maria Gonzalez", "maria.gonzalez@bsj.edu", "Finance Office", 10, true)
        };

        _context.Departments.AddRange(departments);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} departments", departments.Count);
    }

    private async Task SeedIncidentCategoriesAsync(bool forceReseed = false)
    {
        var existingCount = await _context.IncidentCategories.CountAsync();
        
        _logger.LogInformation("SeedIncidentCategories - ExistingCount: {Count}, ForceReseed: {ForceReseed}", 
            existingCount, forceReseed);
        
        // Skip seeding only if data already exists AND forceReseed is not enabled
        if (existingCount > 0 && !forceReseed)
        {
            _logger.LogInformation("Incident categories already exist ({Count} records), skipping seeding", existingCount);
            return;
        }
        
        _logger.LogInformation("Starting incident categories seeding...");

        var categories = new List<IncidentCategory>
        {
            IncidentCategory.Create("Slip, Trip, Fall", "STF", "Incidents involving slips, trips, or falls", "#dc3545", "fa-person-falling", 1, true),
            IncidentCategory.Create("Equipment Malfunction", "EQUIP", "Incidents related to equipment failures or malfunctions", "#fd7e14", "fa-gear", 2, false),
            IncidentCategory.Create("Chemical Exposure", "CHEM", "Incidents involving exposure to hazardous chemicals", "#dc3545", "fa-flask", 3, true),
            IncidentCategory.Create("Fire/Explosion", "FIRE", "Fire-related incidents and explosions", "#dc3545", "fa-fire", 4, true),
            IncidentCategory.Create("Medical Emergency", "MED", "Medical emergencies and health-related incidents", "#dc3545", "fa-briefcase-medical", 5, true),
            IncidentCategory.Create("Security Breach", "SEC", "Security incidents and unauthorized access", "#6f42c1", "fa-shield-halved", 6, true),
            IncidentCategory.Create("Vehicle Accident", "VEH", "Vehicle-related accidents and incidents", "#e83e8c", "fa-car-crash", 7, true),
            IncidentCategory.Create("Workplace Violence", "VIO", "Incidents involving violence or threats", "#dc3545", "fa-hand-fist", 8, true),
            IncidentCategory.Create("Environmental", "ENV", "Environmental incidents and spills", "#198754", "fa-leaf", 9, false),
            IncidentCategory.Create("Electrical", "ELEC", "Electrical incidents and power-related issues", "#ffc107", "fa-bolt", 10, true),
            IncidentCategory.Create("Structural", "STRUCT", "Building and structural incidents", "#6c757d", "fa-building", 11, false),
            IncidentCategory.Create("Food Safety", "FOOD", "Food safety and contamination incidents", "#fd7e14", "fa-utensils", 12, true),
            IncidentCategory.Create("Data Breach", "DATA", "Information security and data protection incidents", "#6f42c1", "fa-database", 13, true),
            IncidentCategory.Create("Near Miss", "NEAR", "Near miss incidents without actual injury or damage", "#20c997", "fa-exclamation-triangle", 14, false),
            IncidentCategory.Create("Property Damage", "PROP", "Incidents resulting in property damage", "#6c757d", "fa-hammer", 15, false)
        };

        _context.IncidentCategories.AddRange(categories);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} incident categories", categories.Count);
    }

    private async Task SeedIncidentLocationsAsync(bool forceReseed = false)
    {
        var existingCount = await _context.IncidentLocations.CountAsync();
        
        _logger.LogInformation("SeedIncidentLocations - ExistingCount: {Count}, ForceReseed: {ForceReseed}", 
            existingCount, forceReseed);
        
        // Skip seeding only if data already exists AND forceReseed is not enabled
        if (existingCount > 0 && !forceReseed)
        {
            _logger.LogInformation("Incident locations already exist ({Count} records), skipping seeding", existingCount);
            return;
        }
        
        _logger.LogInformation("Starting incident locations seeding...");

        var locations = new List<IncidentLocation>
        {
            // Academic Buildings
            IncidentLocation.Create("Primary School Block A", "PSA", "Primary school classrooms and facilities", "Primary School", "Ground", null, GeoLocation.Create(-6.1751, 106.8650), 1, false),
            IncidentLocation.Create("Primary School Block B", "PSB", "Primary school specialized rooms", "Primary School", "First", null, GeoLocation.Create(-6.1752, 106.8651), 2, false),
            IncidentLocation.Create("Secondary School Block", "SSB", "Secondary school main building", "Secondary School", "Ground", null, GeoLocation.Create(-6.1753, 106.8652), 3, false),
            IncidentLocation.Create("Science Laboratory", "LAB", "Chemistry and Physics laboratories", "Academic", "Second", "201-205", GeoLocation.Create(-6.1754, 106.8653), 4, true),
            IncidentLocation.Create("Computer Lab", "COMP", "IT and computer science facilities", "Academic", "First", "101-103", GeoLocation.Create(-6.1755, 106.8654), 5, false),
            
            // Common Areas
            IncidentLocation.Create("Main Cafeteria", "CAF", "Student and staff dining area", "Cafeteria", "Ground", null, GeoLocation.Create(-6.1756, 106.8655), 6, false),
            IncidentLocation.Create("Kitchen", "KITCHEN", "Food preparation and cooking area", "Cafeteria", "Ground", "K1-K5", GeoLocation.Create(-6.1757, 106.8656), 7, true),
            IncidentLocation.Create("Main Library", "LIB", "Library and study areas", "Library", "Ground", null, GeoLocation.Create(-6.1758, 106.8657), 8, false),
            IncidentLocation.Create("Gymnasium", "GYM", "Sports and physical education facility", "Sports Complex", "Ground", null, GeoLocation.Create(-6.1759, 106.8658), 9, false),
            IncidentLocation.Create("Swimming Pool", "POOL", "Swimming pool and aquatic facilities", "Sports Complex", "Ground", null, GeoLocation.Create(-6.1760, 106.8659), 10, true),
            
            // Outdoor Areas
            IncidentLocation.Create("Main Playground", "PLAY", "Primary school playground area", "Outdoor", null, null, GeoLocation.Create(-6.1761, 106.8660), 11, false),
            IncidentLocation.Create("Sports Field", "FIELD", "Football and athletics field", "Outdoor", null, null, GeoLocation.Create(-6.1762, 106.8661), 12, false),
            IncidentLocation.Create("Car Park A", "PARK-A", "Main visitor and staff parking", "Outdoor", null, null, GeoLocation.Create(-6.1763, 106.8662), 13, false),
            IncidentLocation.Create("Car Park B", "PARK-B", "Secondary parking area", "Outdoor", null, null, GeoLocation.Create(-6.1764, 106.8663), 14, false),
            
            // Administrative Areas
            IncidentLocation.Create("Administration Building", "ADMIN", "Administrative offices and meeting rooms", "Administration", "Ground", null, GeoLocation.Create(-6.1765, 106.8664), 15, false),
            IncidentLocation.Create("Reception Area", "RECEP", "Main reception and visitor area", "Administration", "Ground", "R1", GeoLocation.Create(-6.1766, 106.8665), 16, false),
            IncidentLocation.Create("IT Server Room", "SERVER", "Server and network infrastructure room", "IT Center", "Basement", "B1", GeoLocation.Create(-6.1767, 106.8666), 17, true),
            IncidentLocation.Create("Maintenance Workshop", "WORKSHOP", "Maintenance and repair workshop", "Maintenance", "Ground", null, GeoLocation.Create(-6.1768, 106.8667), 18, true),
            
            // Specialized Areas
            IncidentLocation.Create("Health Center", "HEALTH", "Medical and health services center", "Health Center", "Ground", null, GeoLocation.Create(-6.1769, 106.8668), 19, false),
            IncidentLocation.Create("Security Office", "SEC-OFF", "Security control room and office", "Security", "Ground", "S1", GeoLocation.Create(-6.1770, 106.8669), 20, false),
            IncidentLocation.Create("Art Studio", "ART", "Art and creative studies studio", "Creative Arts", "First", "A1-A3", GeoLocation.Create(-6.1771, 106.8670), 21, false),
            IncidentLocation.Create("Music Room", "MUSIC", "Music practice and performance room", "Creative Arts", "Second", "M1-M2", GeoLocation.Create(-6.1772, 106.8671), 22, false),
            IncidentLocation.Create("Auditorium", "AUD", "Main auditorium and assembly hall", "Assembly", "Ground", null, GeoLocation.Create(-6.1773, 106.8672), 23, false),
            
            // Storage and Utility Areas
            IncidentLocation.Create("Storage Room A", "STOR-A", "General storage and supplies", "Storage", "Basement", "B2", GeoLocation.Create(-6.1774, 106.8673), 24, false),
            IncidentLocation.Create("Chemical Storage", "CHEM-STOR", "Chemical and hazardous material storage", "Storage", "Basement", "B3", GeoLocation.Create(-6.1775, 106.8674), 25, true),
            IncidentLocation.Create("Boiler Room", "BOILER", "Heating and mechanical equipment room", "Utilities", "Basement", "U1", GeoLocation.Create(-6.1776, 106.8675), 26, true),
            IncidentLocation.Create("Generator Room", "GEN", "Emergency generator and electrical systems", "Utilities", "Basement", "U2", GeoLocation.Create(-6.1777, 106.8676), 27, true),
            
            // General/Other
            IncidentLocation.Create("Corridor/Hallway", "CORR", "Corridors, hallways, and transitional spaces", "General", null, null, null, 28, false),
            IncidentLocation.Create("Stairwell", "STAIR", "Staircases and stairwells", "General", null, null, null, 29, false),
            IncidentLocation.Create("Other/Unknown", "OTHER", "Other or unknown location", "General", null, null, null, 30, false)
        };

        _context.IncidentLocations.AddRange(locations);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} incident locations", locations.Count);
    }

    private async Task SeedHazardConfigurationAsync(bool forceReseed = false)
    {
        var existingCategories = await _context.HazardCategories.CountAsync();
        var existingTypes = await _context.HazardTypes.CountAsync();
        
        _logger.LogInformation("ForceReseed: {ForceReseed}, Existing hazard categories: {CatCount}, Existing types: {TypeCount}", 
            forceReseed, existingCategories, existingTypes);
        
        // Seed Hazard Categories
        if (existingCategories == 0 || forceReseed)
        {
            if (forceReseed && existingCategories > 0)
            {
                _logger.LogInformation("Re-seeding hazard configuration, removing dependent data first...");
                
                // Remove Hazards first (they may depend on categories/types)
                var existingHazards = await _context.Hazards.CountAsync();
                if (existingHazards > 0)
                {
                    _logger.LogInformation("Removing {Count} existing hazards...", existingHazards);
                    _context.Hazards.RemoveRange(_context.Hazards);
                    await _context.SaveChangesAsync();
                }
                
                // Remove HazardTypes (they depend on categories)
                if (existingTypes > 0)
                {
                    _logger.LogInformation("Removing {Count} existing hazard types...", existingTypes);
                    _context.HazardTypes.RemoveRange(_context.HazardTypes);
                    await _context.SaveChangesAsync();
                }
                
                // Now remove categories
                _logger.LogInformation("Removing {Count} existing hazard categories...", existingCategories);
                _context.HazardCategories.RemoveRange(_context.HazardCategories);
                await _context.SaveChangesAsync();
                existingCategories = 0; // Reset count after removal
            }
            
            var categories = new[]
            {
                HazardCategory.Create("Physical", "PHYS", "Physical hazards including slips, trips, falls", "#fd7e14", "Medium", 1),
                HazardCategory.Create("Chemical", "CHEM", "Chemical exposure and storage hazards", "#dc3545", "High", 2),
                HazardCategory.Create("Biological", "BIO", "Biological contamination and health hazards", "#28a745", "Medium", 3),
                HazardCategory.Create("Ergonomic", "ERGO", "Workplace ergonomic and repetitive strain hazards", "#ffc107", "Low", 4),
                HazardCategory.Create("Psychological", "PSYCH", "Mental health and workplace stress hazards", "#6f42c1", "Medium", 5),
                HazardCategory.Create("Environmental", "ENV", "Environmental and climate-related hazards", "#20c997", "Medium", 6),
                HazardCategory.Create("Fire", "FIRE", "Fire safety and prevention hazards", "#e74c3c", "High", 7),
                HazardCategory.Create("Electrical", "ELEC", "Electrical safety hazards", "#f39c12", "High", 8),
                HazardCategory.Create("Mechanical", "MECH", "Mechanical equipment and machinery hazards", "#3498db", "High", 9),
                HazardCategory.Create("Radiation", "RAD", "Radiation and electromagnetic hazards", "#9b59b6", "High", 10)
            };

            _context.HazardCategories.AddRange(categories);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} hazard categories", categories.Length);
        }

        // Seed Hazard Types
        if (existingTypes == 0 || forceReseed)
        {
            if (forceReseed && existingTypes > 0)
            {
                _logger.LogInformation("Re-seeding hazard types, removing {Count} existing types...", existingTypes);
                var existingTypesList = await _context.HazardTypes.ToListAsync();
                _context.HazardTypes.RemoveRange(existingTypesList);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed {Count} hazard types", existingTypesList.Count);
                existingTypes = 0; // Reset count after removal
            }
            
            // Get category IDs for foreign key relationships
            var physicalCategory = await _context.HazardCategories.FirstAsync(c => c.Code == "PHYS");
            var chemicalCategory = await _context.HazardCategories.FirstAsync(c => c.Code == "CHEM");
            var biologicalCategory = await _context.HazardCategories.FirstAsync(c => c.Code == "BIO");
            var fireCategory = await _context.HazardCategories.FirstAsync(c => c.Code == "FIRE");
            var electricalCategory = await _context.HazardCategories.FirstAsync(c => c.Code == "ELEC");
            var mechanicalCategory = await _context.HazardCategories.FirstAsync(c => c.Code == "MECH");

            var types = new[]
            {
                // Physical hazard types
                HazardType.Create("Slippery Surface", "SLIP", "Slipping hazards from wet or oily surfaces", physicalCategory.Id, 1.2m, false, 1),
                HazardType.Create("Trip Hazard", "TRIP", "Tripping hazards from obstacles or uneven surfaces", physicalCategory.Id, 1.1m, false, 2),
                HazardType.Create("Fall from Height", "FALL", "Fall from height hazards", physicalCategory.Id, 1.8m, true, 3),
                HazardType.Create("Sharp Object", "SHARP", "Cutting and laceration hazards from sharp objects", physicalCategory.Id, 1.3m, false, 4),
                HazardType.Create("Hot Surface", "HOT", "Heat and burn hazards from hot surfaces", physicalCategory.Id, 1.5m, false, 5),
                HazardType.Create("Confined Space", "CS", "Confined space entry hazards", physicalCategory.Id, 2.0m, true, 6),
                HazardType.Create("Noise Exposure", "NOISE", "Excessive noise exposure hazards", physicalCategory.Id, 1.4m, false, 7),
                
                // Chemical hazard types
                HazardType.Create("Toxic Gas Release", "TOX", "Toxic gas release and inhalation hazards", chemicalCategory.Id, 2.5m, true, 8),
                HazardType.Create("Corrosive Liquid Spill", "COR", "Corrosive liquid spill hazards", chemicalCategory.Id, 2.0m, true, 9),
                HazardType.Create("Chemical Exposure", "EXPO", "General chemical exposure hazards", chemicalCategory.Id, 1.8m, true, 10),
                HazardType.Create("Chemical Spill", "SPILL", "Chemical spill containment hazards", chemicalCategory.Id, 1.7m, false, 11),
                
                // Biological hazard types
                HazardType.Create("Biological Contamination", "BIO_CONT", "Biological contamination hazards", biologicalCategory.Id, 1.9m, true, 12),
                
                // Fire hazard types
                HazardType.Create("Combustible Material", "COM", "Combustible material fire hazards", fireCategory.Id, 2.2m, true, 13),
                HazardType.Create("Fire Hazard", "FIRE_T", "General fire hazards", fireCategory.Id, 2.5m, true, 14),
                HazardType.Create("Explosion Risk", "EXPL", "Explosion hazards", fireCategory.Id, 3.0m, true, 15),
                
                // Electrical hazard types
                HazardType.Create("Electrical Shock", "SHOCK", "Electrical shock hazards", electricalCategory.Id, 2.1m, true, 16),
                HazardType.Create("Exposed Wiring", "WIRE", "Exposed electrical wiring hazards", electricalCategory.Id, 1.6m, false, 17),
                
                // Mechanical hazard types
                HazardType.Create("Machinery Malfunction", "MACH", "Mechanical equipment malfunction hazards", mechanicalCategory.Id, 1.9m, true, 18),
                HazardType.Create("Moving Parts", "MOVE", "Moving machinery parts hazards", mechanicalCategory.Id, 1.7m, false, 19),
                HazardType.Create("Pressure System", "PRESS", "High pressure system hazards", mechanicalCategory.Id, 2.3m, true, 20)
            };

            _context.HazardTypes.AddRange(types);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} hazard types", types.Length);
        }
    }

    private async Task SeedPPEConfigurationAsync(bool forceReseed = false)
    {
        var existingCategories = await _context.PPECategories.CountAsync();
        var existingSizes = await _context.PPESizes.CountAsync();
        var existingStorageLocations = await _context.PPEStorageLocations.CountAsync();
        
        _logger.LogInformation("ForceReseed: {ForceReseed}, Existing PPE categories: {CatCount}, sizes: {SizeCount}, storage locations: {LocCount}", 
            forceReseed, existingCategories, existingSizes, existingStorageLocations);
        
        // Seed PPE Categories
        if (existingCategories == 0 || forceReseed)
        {
            if (forceReseed && existingCategories > 0)
            {
                _logger.LogInformation("Re-seeding PPE configuration, removing dependent data first...");
                
                // Remove PPEAssignments first (they depend on PPEItems)
                var existingAssignments = await _context.PPEAssignments.CountAsync();
                if (existingAssignments > 0)
                {
                    _logger.LogInformation("Removing {Count} existing PPE assignments...", existingAssignments);
                    _context.PPEAssignments.RemoveRange(_context.PPEAssignments);
                    await _context.SaveChangesAsync();
                }
                
                // Remove PPEInspections (they depend on PPEItems)
                var existingInspections = await _context.PPEInspections.CountAsync();
                if (existingInspections > 0)
                {
                    _logger.LogInformation("Removing {Count} existing PPE inspections...", existingInspections);
                    _context.PPEInspections.RemoveRange(_context.PPEInspections);
                    await _context.SaveChangesAsync();
                }
                
                // Remove PPERequests (they depend on categories)
                var existingRequests = await _context.PPERequests.CountAsync();
                if (existingRequests > 0)
                {
                    _logger.LogInformation("Removing {Count} existing PPE requests...", existingRequests);
                    _context.PPERequests.RemoveRange(_context.PPERequests);
                    await _context.SaveChangesAsync();
                }
                
                // Remove PPEItems (they depend on categories)
                var existingItems = await _context.PPEItems.CountAsync();
                if (existingItems > 0)
                {
                    _logger.LogInformation("Removing {Count} existing PPE items...", existingItems);
                    _context.PPEItems.RemoveRange(_context.PPEItems);
                    await _context.SaveChangesAsync();
                }
                
                // Remove PPEComplianceRequirements (they depend on categories)
                var existingCompliance = await _context.PPEComplianceRequirements.CountAsync();
                if (existingCompliance > 0)
                {
                    _logger.LogInformation("Removing {Count} existing PPE compliance requirements...", existingCompliance);
                    _context.PPEComplianceRequirements.RemoveRange(_context.PPEComplianceRequirements);
                    await _context.SaveChangesAsync();
                }
                
                // Now remove categories
                _logger.LogInformation("Removing {Count} existing PPE categories...", existingCategories);
                _context.PPECategories.RemoveRange(_context.PPECategories);
                await _context.SaveChangesAsync();
                existingCategories = 0; // Reset count after removal
            }
            
            var categories = new[]
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

            _context.PPECategories.AddRange(categories);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} PPE categories", categories.Length);
        }

        // Seed PPE Sizes
        if (existingSizes == 0 || forceReseed)
        {
            if (forceReseed && existingSizes > 0)
            {
                _logger.LogInformation("Re-seeding PPE sizes, removing {Count} existing sizes...", existingSizes);
                _context.PPESizes.RemoveRange(_context.PPESizes);
                await _context.SaveChangesAsync();
                existingSizes = 0; // Reset count after removal
            }
            
            var sizes = new[]
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

            _context.PPESizes.AddRange(sizes);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} PPE sizes", sizes.Length);
        }

        // Seed PPE Storage Locations
        if (existingStorageLocations == 0 || forceReseed)
        {
            if (forceReseed && existingStorageLocations > 0)
            {
                _logger.LogInformation("Re-seeding PPE storage locations, removing {Count} existing locations...", existingStorageLocations);
                _context.PPEStorageLocations.RemoveRange(_context.PPEStorageLocations);
                await _context.SaveChangesAsync();
                existingStorageLocations = 0; // Reset count after removal
            }
            
            var storageLocations = new[]
            {
                PPEStorageLocation.Create("Main Safety Office", "MSO", "system", "Primary safety equipment storage", "Building A, Ground Floor", "HSE Manager", "+62-21-1234567", 2000),
                PPEStorageLocation.Create("Chemistry Lab Storage", "CHEM", "system", "Chemistry laboratory PPE storage", "Science Building, 2nd Floor", "Lab Supervisor", "+62-21-1234568", 500),
                PPEStorageLocation.Create("Maintenance Workshop", "MAINT", "system", "Maintenance department storage", "Workshop Building", "Maintenance Supervisor", "+62-21-1234569", 1000),
                PPEStorageLocation.Create("PE Equipment Room", "PE", "system", "Physical education equipment storage", "Sports Complex", "PE Coordinator", "+62-21-1234570", 300),
                PPEStorageLocation.Create("Emergency Response Station", "ERS", "system", "Emergency response equipment", "Multiple Locations", "Security Chief", "+62-21-1234571", 200),
                PPEStorageLocation.Create("Warehouse A", "WH-A", "system", "Primary warehouse storage", "External Warehouse Complex", "Warehouse Manager", "+62-21-1234572", 5000),
                PPEStorageLocation.Create("Cafeteria Storage", "CAF", "system", "Food service PPE storage", "Cafeteria Building", "Food Service Manager", "+62-21-1234573", 150)
            };

            _context.PPEStorageLocations.AddRange(storageLocations);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} PPE storage locations", storageLocations.Length);
        }
    }
}