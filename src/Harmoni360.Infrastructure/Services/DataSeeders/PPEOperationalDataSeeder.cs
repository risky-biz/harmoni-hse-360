using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class PPEOperationalDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PPEOperationalDataSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _random = new();

    public PPEOperationalDataSeeder(ApplicationDbContext context, ILogger<PPEOperationalDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting PPE operational data seeding...");

            // Get required data
            var users = await _context.Users.ToListAsync();
            var ppeItems = await _context.PPEItems.Include(p => p.Category).ToListAsync();
            var departments = new[] { "Operations", "Maintenance", "Safety", "Engineering", "Administration" };

            if (!users.Any() || !ppeItems.Any())
            {
                _logger.LogWarning("Cannot seed PPE operational data - missing users or PPE items");
                return;
            }

            // Clear existing operational data if force reseed
            var forceReseedValue = _configuration["DataSeeding:ForceReseed"];
            var forceReseed = string.Equals(forceReseedValue, "true", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(forceReseedValue, "True", StringComparison.OrdinalIgnoreCase) ||
                             (bool.TryParse(forceReseedValue, out var boolResult) && boolResult);
            if (forceReseed)
            {
                await ClearExistingDataAsync();
            }

            // Check if data already exists
            if (!forceReseed && await _context.PPEAssignments.AnyAsync())
            {
                _logger.LogInformation("PPE operational data already exists, skipping seeding");
                return;
            }

            // Create operational data
            await SeedPPEAssignmentsAsync(users, ppeItems, departments);
            await SeedPPERequestsAsync(users, ppeItems, departments);
            await SeedPPEInspectionsAsync(users, ppeItems);
            await SeedPPEComplianceRequirementsAsync(departments);

            await _context.SaveChangesAsync();
            _logger.LogInformation("PPE operational data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding PPE operational data");
            throw;
        }
    }

    private async Task ClearExistingDataAsync()
    {
        _logger.LogInformation("Clearing existing PPE operational data...");
        
        _context.PPEInspections.RemoveRange(_context.PPEInspections);
        _context.PPEAssignments.RemoveRange(_context.PPEAssignments);
        _context.PPERequests.RemoveRange(_context.PPERequests);
        _context.PPEComplianceRequirements.RemoveRange(_context.PPEComplianceRequirements);
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Existing PPE operational data cleared");
    }

    private async Task SeedPPEAssignmentsAsync(List<User> users, List<PPEItem> ppeItems, string[] departments)
    {
        _logger.LogInformation("Seeding PPE assignments...");

        var assignments = new List<PPEAssignment>();
        var startDate = DateTime.UtcNow.AddYears(-2);

        foreach (var user in users.Take(80)) // Assign PPE to most users
        {
            var userDepartment = user.Department ?? departments[_random.Next(departments.Length)];
            var assignmentCount = _random.Next(2, 6); // 2-5 PPE items per user

            var userPPEItems = ppeItems.OrderBy(x => _random.Next()).Take(assignmentCount);

            foreach (var ppeItem in userPPEItems)
            {
                var assignmentDate = startDate.AddDays(_random.Next(0, 600));
                var isActive = _random.NextDouble() > 0.2; // 80% active assignments

                var assignment = PPEAssignment.Create(
                    ppeItemId: ppeItem.Id,
                    assignedToId: user.Id,
                    assignedBy: "system",
                    purpose: $"Standard PPE assignment for {ppeItem.Category?.Name ?? "safety"} operations in {userDepartment}"
                );

                // Set assignment status and return details
                if (!isActive && _random.NextDouble() > 0.3)
                {
                    assignment.Return("system", "Returned in good condition");
                }
                else if (_random.NextDouble() > 0.7)
                {
                    assignment.MarkAsLost("system", "Equipment lost during operations");
                }

                assignments.Add(assignment);
            }
        }

        _context.PPEAssignments.AddRange(assignments);
        _logger.LogInformation($"Seeded {assignments.Count} PPE assignments");
    }

    private async Task SeedPPERequestsAsync(List<User> users, List<PPEItem> ppeItems, string[] departments)
    {
        _logger.LogInformation("Seeding PPE requests...");

        var startDate = DateTime.UtcNow.AddMonths(-6);

        foreach (var user in users.Take(50))
        {
            var requestCount = _random.Next(1, 4); // 1-3 requests per user

            for (int i = 0; i < requestCount; i++)
            {
                var ppeItem = ppeItems[_random.Next(ppeItems.Count)];
                var requestDate = startDate.AddDays(_random.Next(0, 180));
                var priority = (RequestPriority)_random.Next(1, 5);

                var request = PPERequest.Create(
                    requesterId: user.Id,
                    categoryId: ppeItem.CategoryId,
                    justification: GetPPERequestJustification(ppeItem, priority),
                    priority: priority,
                    createdBy: user.Name,
                    requiredDate: requestDate.AddDays(_random.Next(1, 14)),
                    notes: $"Request from {user.Department ?? departments[_random.Next(departments.Length)]} department"
                );

                // Save the request first to get the ID
                _context.PPERequests.Add(request);
                await _context.SaveChangesAsync();

                // Add request items
                var itemCount = _random.Next(1, 4); // 1-3 items per request
                for (int j = 0; j < itemCount; j++)
                {
                    var selectedItem = ppeItems[_random.Next(ppeItems.Count)];
                    request.AddRequestItem(
                        itemDescription: selectedItem.Name,
                        size: selectedItem.Size?.Name,
                        quantity: _random.Next(1, 5)
                    );
                }

                // Submit the request
                request.Submit(user.Name);
                
                // Process some requests
                var processChance = _random.NextDouble();
                if (processChance > 0.3) // 70% processed
                {
                    var processor = users[_random.Next(users.Count)];
                    
                    // Assign reviewer
                    request.AssignReviewer(processor.Id, "System");

                    if (processChance > 0.2) // 50% approved
                    {
                        request.Approve(processor.Name, "Request approved - equipment available");
                        
                        // Some approved requests are fulfilled
                        if (_random.NextDouble() > 0.3)
                        {
                            request.Fulfill(ppeItem.Id, processor.Name, "Equipment issued to requester");
                        }
                    }
                    else // 20% rejected
                    {
                        request.Reject(processor.Name, "Request rejected - equipment not available or not justified");
                    }
                }

                // Save changes after processing each request
                await _context.SaveChangesAsync();
            }
        }

        var totalRequests = await _context.PPERequests.CountAsync();
        _logger.LogInformation($"Seeded PPE requests - total count: {totalRequests}");
    }

    private async Task SeedPPEInspectionsAsync(List<User> users, List<PPEItem> ppeItems)
    {
        _logger.LogInformation("Seeding PPE inspections...");

        var inspections = new List<PPEInspection>();
        var assignments = await _context.PPEAssignments.Where(a => a.Status == AssignmentStatus.Active).ToListAsync();
        var inspectors = users.Where(u => u.Department == "Safety" || u.Email.Contains("manager")).ToList();

        foreach (var assignment in assignments.Take(100)) // Inspect some assignments
        {
            var inspectionCount = _random.Next(1, 4); // 1-3 inspections per assignment

            for (int i = 0; i < inspectionCount; i++)
            {
                var inspector = inspectors.Any() ? inspectors[_random.Next(inspectors.Count)] : users[_random.Next(users.Count)];
                var inspectionDate = assignment.AssignedDate.AddDays(_random.Next(30, 300));
                
                if (inspectionDate > DateTime.UtcNow) continue;

                var condition = (PPECondition)_random.Next(1, 6); // New to Poor (1-5)
                var isServiceable = condition <= PPECondition.Fair;

                var result = condition <= PPECondition.Fair ? InspectionResult.Passed : 
                            condition == PPECondition.Poor ? InspectionResult.PassedWithObservations : 
                            InspectionResult.Failed;

                var inspection = PPEInspection.Create(
                    ppeItemId: assignment.PPEItemId,
                    inspectorId: inspector.Id,
                    inspectionDate: inspectionDate,
                    result: result,
                    createdBy: inspector.Name,
                    findings: GetInspectionFindings(condition),
                    correctiveActions: GetInspectionRecommendations(condition, isServiceable),
                    recommendedCondition: condition,
                    requiresMaintenance: condition > PPECondition.Good,
                    nextInspectionIntervalDays: _random.Next(30, 90)
                );

                inspections.Add(inspection);
            }
        }

        _context.PPEInspections.AddRange(inspections);
        _logger.LogInformation($"Seeded {inspections.Count} PPE inspections");
    }

    private async Task SeedPPEComplianceRequirementsAsync(string[] departments)
    {
        _logger.LogInformation("Seeding PPE compliance requirements...");

        var requirements = new List<PPEComplianceRequirement>();
        var ppeCategories = await _context.PPECategories.ToListAsync();
        var roles = await _context.Roles.ToListAsync();

        foreach (var role in roles)
        {
            foreach (var category in ppeCategories)
            {
                var isMandatory = _random.NextDouble() > 0.3; // 70% mandatory
                if (!isMandatory && _random.NextDouble() > 0.5) continue; // Skip some non-mandatory

                var requirement = PPEComplianceRequirement.Create(
                    roleId: role.Id,
                    categoryId: category.Id,
                    isMandatory: isMandatory,
                    createdBy: "system",
                    riskAssessmentReference: GetRegulatoryReference(category.Name),
                    complianceNote: $"PPE compliance requirement for {category.Name} in {role.Name} role",
                    minimumQuantity: isMandatory ? _random.Next(1, 5) : 0,
                    replacementIntervalDays: _random.Next(30, 180),
                    requiresTraining: _random.NextDouble() > 0.5,
                    trainingRequirements: "Basic PPE safety training required"
                );

                requirements.Add(requirement);
            }
        }

        _context.PPEComplianceRequirements.AddRange(requirements);
        _logger.LogInformation($"Seeded {requirements.Count} PPE compliance requirements");
    }

    private string GetRandomLocation()
    {
        var locations = new[] 
        {
            "Main Plant", "Warehouse A", "Warehouse B", "Office Building", 
            "Production Floor 1", "Production Floor 2", "Maintenance Shop",
            "Loading Dock", "Chemical Storage", "Control Room"
        };
        return locations[_random.Next(locations.Length)];
    }

    private string GetPPERequestJustification(PPEItem ppeItem, RequestPriority priority)
    {
        var justifications = priority switch
        {
            RequestPriority.Urgent => new[]
            {
                $"Immediate replacement needed - current {ppeItem.Name} is damaged and unsafe",
                $"Safety requirement for urgent project - {ppeItem.Name} required immediately",
                $"Compliance issue - missing {ppeItem.Name} preventing work operations"
            },
            RequestPriority.High => new[]
            {
                $"High priority replacement of {ppeItem.Name} due to wear",
                $"Critical {ppeItem.Name} needed for new team member",
                $"Important backup {ppeItem.Name} required for operations"
            },
            RequestPriority.Medium => new[]
            {
                $"Scheduled replacement of {ppeItem.Name} due to wear",
                $"Additional {ppeItem.Name} needed for new team member",
                $"Backup {ppeItem.Name} required for critical operations"
            },
            _ => new[]
            {
                $"Routine replacement of {ppeItem.Name}",
                $"Standard issue {ppeItem.Name} for department operations",
                $"Preventive replacement of {ppeItem.Name} before expiration"
            }
        };
        return justifications[_random.Next(justifications.Length)];
    }

    private string GetInspectionFindings(PPECondition condition)
    {
        return condition switch
        {
            PPECondition.Excellent => "Equipment in excellent condition. All components functioning properly. No visible wear or damage.",
            PPECondition.Good => "Equipment in good working condition. Minor signs of use but fully functional and safe.",
            PPECondition.Fair => "Equipment showing moderate wear. Some components may need attention but still serviceable with monitoring.",
            PPECondition.Poor => "Equipment in poor condition. Significant wear and damage observed. Safety concerns identified.",
            _ => "Condition assessment completed. Equipment evaluated according to safety standards."
        };
    }

    private string GetInspectionRecommendations(PPECondition condition, bool isServiceable)
    {
        if (!isServiceable)
        {
            return "IMMEDIATE ACTION REQUIRED: Remove from service immediately. Schedule replacement. Do not use until repaired or replaced.";
        }

        return condition switch
        {
            PPECondition.Excellent => "Continue normal use. Schedule next routine inspection as planned.",
            PPECondition.Good => "Continue use with regular monitoring. Inspect more frequently if heavy usage expected.",
            PPECondition.Fair => "Monitor closely for deterioration. Consider replacement planning. Increase inspection frequency.",
            PPECondition.Poor => "Priority replacement recommended. Use only for non-critical operations until replaced.",
            _ => "Follow standard maintenance procedures and inspection schedule."
        };
    }

    private string GetRegulatoryReference(string categoryName)
    {
        var references = new Dictionary<string, string>
        {
            { "Head Protection", "ANSI/ISEA Z89.1-2019 - American National Standard for Industrial Head Protection" },
            { "Eye and Face Protection", "ANSI/ISEA Z87.1-2020 - Occupational and Educational Personal Eye and Face Protection Devices" },
            { "Respiratory Protection", "29 CFR 1910.134 - OSHA Respiratory Protection Standard" },
            { "Hand Protection", "ANSI/ISEA 105-2016 - Hand Protection Classification" },
            { "Foot Protection", "ANSI/ISEA Z41-1999 - Personal Protection - Protective Footwear" },
            { "Fall Protection", "29 CFR 1910.140 - OSHA Personal Fall Protection Systems" },
            { "Hearing Protection", "29 CFR 1910.95 - OSHA Occupational Noise Exposure Standard" }
        };

        return references.GetValueOrDefault(categoryName, "Internal Safety Standard - PPE-001");
    }
}