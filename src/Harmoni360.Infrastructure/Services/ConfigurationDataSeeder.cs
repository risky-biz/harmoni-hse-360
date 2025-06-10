using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services;

public class ConfigurationDataSeeder : IDataSeeder
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ConfigurationDataSeeder> _logger;
    private readonly bool _isDemoMode;
    private readonly bool _shouldSeedConfigurationData;
    private readonly bool _shouldReSeedConfigurationData;

    public ConfigurationDataSeeder(
        IApplicationDbContext context,
        ILogger<ConfigurationDataSeeder> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _isDemoMode = bool.TryParse(configuration["Application:DemoMode"], out var demoMode) && demoMode;
        _shouldSeedConfigurationData = bool.TryParse(configuration["DataSeeding:SeedConfigurationData"], out var seedConfig) && seedConfig;
        _shouldReSeedConfigurationData = bool.TryParse(configuration["DataSeeding:ReSeedConfigurationData"], out var reseedConfig) && reseedConfig;
    }

    public async Task SeedAsync()
    {
        if (!_shouldSeedConfigurationData)
        {
            _logger.LogInformation("Configuration data seeding is disabled");
            return;
        }

        _logger.LogInformation("Starting configuration data seeding in {Mode} mode", _isDemoMode ? "Demo" : "Production");

        await SeedDepartmentsAsync();
        await SeedIncidentCategoriesAsync();
        await SeedIncidentLocationsAsync();

        await _context.SaveChangesAsync();
        _logger.LogInformation("Configuration data seeding completed");
    }

    private async Task SeedDepartmentsAsync()
    {
        if (_shouldReSeedConfigurationData)
        {
            var existingDepartments = await _context.Departments.ToListAsync();
            _context.Departments.RemoveRange(existingDepartments);
            _logger.LogInformation("Removed {Count} existing departments for re-seeding", existingDepartments.Count);
        }

        if (await _context.Departments.AnyAsync())
        {
            _logger.LogInformation("Departments already exist, skipping seeding");
            return;
        }

        var departments = new List<Department>
        {
            Department.Create("Academic", "ACAD", "Academic departments including teaching staff and educational support", "Dr. Sarah Johnson", "sarah.johnson@bsj.edu", "Academic Block A", 1),
            Department.Create("Operations", "OPS", "Facilities management, maintenance, and operational support", "Michael Chen", "michael.chen@bsj.edu", "Administration Building", 2),
            Department.Create("Security", "SEC", "Campus security, safety coordination, and emergency response", "James Rodriguez", "james.rodriguez@bsj.edu", "Security Office", 3),
            Department.Create("Information Technology", "IT", "Technology infrastructure, systems administration, and user support", "Priya Sharma", "priya.sharma@bsj.edu", "IT Center", 4),
            Department.Create("Human Resources", "HR", "Staff recruitment, training, compliance, and employee relations", "Lisa Wang", "lisa.wang@bsj.edu", "Administration Building", 5),
            Department.Create("Health Services", "HEALTH", "Medical services, health monitoring, and wellness programs", "Dr. Ahmad Hassan", "ahmad.hassan@bsj.edu", "Health Center", 6),
            Department.Create("Food Services", "FOOD", "Cafeteria operations, food safety, and nutrition programs", "Roberto Silva", "roberto.silva@bsj.edu", "Cafeteria", 7),
            Department.Create("Student Services", "STU", "Student support, activities, and welfare coordination", "Emma Thompson", "emma.thompson@bsj.edu", "Student Center", 8),
            Department.Create("Maintenance", "MAINT", "Building maintenance, repairs, and preventive maintenance programs", "David Kim", "david.kim@bsj.edu", "Maintenance Workshop", 9),
            Department.Create("Finance", "FIN", "Financial operations, procurement, and budget management", "Maria Gonzalez", "maria.gonzalez@bsj.edu", "Finance Office", 10)
        };

        _context.Departments.AddRange(departments);
        _logger.LogInformation("Seeded {Count} departments", departments.Count);
    }

    private async Task SeedIncidentCategoriesAsync()
    {
        if (_shouldReSeedConfigurationData)
        {
            var existingCategories = await _context.IncidentCategories.ToListAsync();
            _context.IncidentCategories.RemoveRange(existingCategories);
            _logger.LogInformation("Removed {Count} existing incident categories for re-seeding", existingCategories.Count);
        }

        if (await _context.IncidentCategories.AnyAsync())
        {
            _logger.LogInformation("Incident categories already exist, skipping seeding");
            return;
        }

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
        _logger.LogInformation("Seeded {Count} incident categories", categories.Count);
    }

    private async Task SeedIncidentLocationsAsync()
    {
        if (_shouldReSeedConfigurationData)
        {
            var existingLocations = await _context.IncidentLocations.ToListAsync();
            _context.IncidentLocations.RemoveRange(existingLocations);
            _logger.LogInformation("Removed {Count} existing incident locations for re-seeding", existingLocations.Count);
        }

        if (await _context.IncidentLocations.AnyAsync())
        {
            _logger.LogInformation("Incident locations already exist, skipping seeding");
            return;
        }

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
        _logger.LogInformation("Seeded {Count} incident locations", locations.Count);
    }
}