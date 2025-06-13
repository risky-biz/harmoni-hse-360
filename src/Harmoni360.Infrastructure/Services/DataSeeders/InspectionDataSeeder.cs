using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Inspections;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class InspectionDataSeeder : IDataSeeder
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<InspectionDataSeeder> _logger;
    private readonly Random _random = new();

    // Inspection locations
    private readonly string[] _locations = new[]
    {
        "Main Production Floor - Building A",
        "Warehouse Section B", 
        "Chemical Storage Area C-12",
        "Electrical Substation Room 101",
        "Laboratory Building D",
        "Loading Dock Area",
        "Maintenance Workshop",
        "Boiler Room - Basement",
        "HVAC Plant Room",
        "Emergency Exit Routes",
        "Fire Safety Equipment Areas",
        "Water Treatment Facility",
        "Compressor Station",
        "Office Building - Floor 3",
        "Cafeteria and Kitchen Area"
    };

    // Inspection titles by type
    private readonly Dictionary<InspectionType, string[]> _inspectionTitles = new()
    {
        [InspectionType.Safety] = new[]
        {
            "Monthly Safety Equipment Inspection",
            "Fire Extinguisher Maintenance Check",
            "Emergency Exit Route Assessment",
            "Personal Protective Equipment Audit",
            "Workplace Hazard Identification",
            "Machine Guarding Inspection",
            "Electrical Safety Assessment",
            "Confined Space Entry Preparation",
            "Fall Protection System Check",
            "Chemical Handling Safety Review"
        },
        [InspectionType.Environmental] = new[]
        {
            "Air Quality Monitoring Assessment", 
            "Waste Management Compliance Check",
            "Water Discharge Quality Inspection",
            "Chemical Storage Environmental Review",
            "Soil Contamination Assessment",
            "Noise Level Monitoring",
            "Emission Control System Inspection",
            "Groundwater Quality Assessment",
            "Environmental Management System Audit",
            "Biodiversity Impact Assessment"
        },
        [InspectionType.Compliance] = new[]
        {
            "Quality Control Process Audit",
            "Product Quality Assurance Check",
            "Manufacturing Process Review",
            "Quality Management System Inspection",
            "Calibration Verification Audit",
            "Documentation Compliance Review",
            "Customer Complaint Investigation",
            "Supplier Quality Assessment",
            "Quality Metrics Performance Review",
            "Continuous Improvement Process Audit"
        },
        [InspectionType.Equipment] = new[]
        {
            "Preventive Maintenance Schedule Review",
            "Equipment Condition Assessment",
            "Maintenance Procedure Compliance Check",
            "Spare Parts Inventory Audit",
            "Maintenance Personnel Competency Review",
            "Maintenance Documentation Audit",
            "Equipment Performance Analysis",
            "Maintenance Cost Effectiveness Review",
            "Maintenance Safety Procedure Check",
            "Predictive Maintenance System Audit"
        },
        [InspectionType.Fire] = new[]
        {
            "Fire Safety System Inspection",
            "Emergency Exit Assessment",
            "Fire Extinguisher Maintenance Check",
            "Sprinkler System Performance Test",
            "Fire Alarm System Verification",
            "Hot Work Permit Area Inspection",
            "Fire Prevention Program Audit",
            "Emergency Response Plan Review",
            "Fire Detection System Check",
            "Evacuation Procedure Assessment"
        },
        [InspectionType.Chemical] = new[]
        {
            "Chemical Storage Compliance Audit",
            "Material Safety Data Sheet Review",
            "Chemical Handling Procedure Check",
            "Spill Prevention System Inspection",
            "Personal Protective Equipment Assessment",
            "Chemical Waste Management Audit",
            "Ventilation System Performance Check",
            "Chemical Inventory Accuracy Review",
            "Emergency Shower and Eyewash Inspection",
            "Chemical Compatibility Assessment"
        },
        [InspectionType.Ergonomic] = new[]
        {
            "Workplace Ergonomic Assessment",
            "Computer Workstation Evaluation",
            "Manual Handling Risk Assessment",
            "Repetitive Strain Injury Prevention Check",
            "Office Furniture and Setup Inspection",
            "Physical Workload Assessment",
            "Ergonomic Training Effectiveness Review",
            "Workplace Design Optimization Audit",
            "Lifting and Carrying Technique Evaluation",
            "Ergonomic Equipment Utilization Check"
        },
        [InspectionType.Emergency] = new[]
        {
            "Emergency Response Plan Drill",
            "Crisis Management System Test",
            "Emergency Equipment Readiness Check",
            "Evacuation Procedure Assessment",
            "Emergency Communication System Test",
            "First Aid Kit and Supplies Inspection",
            "Emergency Lighting System Check",
            "Backup Power System Verification",
            "Emergency Contact System Review",
            "Incident Command Center Inspection"
        }
    };

    // Inspection items by category
    private readonly Dictionary<InspectionCategory, string[]> _inspectionItems = new()
    {
        [InspectionCategory.Routine] = new[]
        {
            "Check safety equipment functionality",
            "Verify emergency procedures are posted",
            "Inspect housekeeping standards",
            "Review incident reports since last inspection",
            "Confirm training records are up to date",
            "Check compliance with standard operating procedures",
            "Verify proper use of personal protective equipment",
            "Inspect work area organization and cleanliness"
        },
        [InspectionCategory.Planned] = new[]
        {
            "Complete comprehensive equipment inspection",
            "Review and update risk assessments",
            "Conduct thorough documentation review",
            "Perform calibration checks on measurement equipment",
            "Verify compliance with regulatory requirements",
            "Assess effectiveness of control measures",
            "Review emergency response procedures",
            "Evaluate training effectiveness and competency"
        },
        [InspectionCategory.Incident] = new[]
        {
            "Document immediate safety hazards identified",
            "Assess environmental impact of incident",
            "Review emergency response effectiveness",
            "Identify root causes of the incident",
            "Evaluate adequacy of existing controls",
            "Document witness statements and evidence",
            "Assess need for immediate corrective actions",
            "Review incident reporting procedures"
        },
        [InspectionCategory.Regulatory] = new[]
        {
            "Verify compliance with regulatory requirements",
            "Review permit conditions and limitations",
            "Check adherence to environmental regulations",
            "Assess compliance with safety standards",
            "Review regulatory reporting requirements",
            "Verify license conditions are met",
            "Check compliance with industry regulations",
            "Assess adherence to legal obligations"
        },
        [InspectionCategory.Audit] = new[]
        {
            "Review management system effectiveness",
            "Assess compliance with policy requirements",
            "Evaluate documentation adequacy and accuracy",
            "Review training and competency records",
            "Assess risk management processes",
            "Evaluate performance monitoring systems",
            "Review corrective action tracking systems",
            "Assess continuous improvement processes"
        },
        [InspectionCategory.Unplanned] = new[]
        {
            "Respond to immediate safety concerns",
            "Address equipment failure reports",
            "Investigate unexpected incidents",
            "Assess emergency response effectiveness",
            "Review unscheduled maintenance issues",
            "Evaluate reactive safety measures",
            "Address urgent compliance issues",
            "Investigate environmental concerns"
        },
        [InspectionCategory.Maintenance] = new[]
        {
            "Check equipment condition and performance",
            "Verify maintenance schedules are followed",
            "Assess spare parts availability",
            "Review maintenance documentation",
            "Evaluate preventive maintenance effectiveness",
            "Check equipment calibration status",
            "Assess maintenance safety procedures",
            "Review equipment lifecycle management"
        }
    };

    // Common finding descriptions
    private readonly Dictionary<FindingType, string[]> _findingDescriptions = new()
    {
        [FindingType.NonConformance] = new[]
        {
            "Safety equipment found to be non-functional or missing",
            "Procedures not being followed according to documented standards",
            "Required training certificates expired or missing",
            "Personal protective equipment not being used properly",
            "Documentation incomplete or inaccurate",
            "Environmental controls not operating within specified parameters",
            "Emergency equipment access blocked or obstructed",
            "Regulatory requirements not being met"
        },
        [FindingType.Observation] = new[]
        {
            "Housekeeping standards could be improved in work areas",
            "Some employees observed not following best practices",
            "Equipment showing signs of wear but still functional",
            "Minor deviations from standard procedures observed",
            "Documentation could be more detailed or clearer",
            "Communication between shifts could be enhanced",
            "Some safety signs showing wear and need replacement",
            "Work area organization could be optimized"
        },
        [FindingType.OpportunityForImprovement] = new[]
        {
            "Consider implementing additional safety controls",
            "Evaluate options for process automation to reduce risk",
            "Review training frequency to improve retention",
            "Consider upgrading equipment for better performance",
            "Evaluate implementing additional monitoring systems",
            "Consider streamlining documentation processes",
            "Evaluate enhancing emergency response capabilities",
            "Consider implementing predictive maintenance programs"
        },
        [FindingType.PositiveFinding] = new[]
        {
            "Excellent adherence to safety procedures observed",
            "Outstanding housekeeping standards maintained",
            "Proactive identification and reporting of hazards",
            "Exceptional emergency response preparedness",
            "Innovative approach to problem solving implemented",
            "Strong safety culture demonstrated by team",
            "Effective use of technology to enhance safety",
            "Exemplary documentation and record keeping"
        }
    };

    public InspectionDataSeeder(IApplicationDbContext context, ILogger<InspectionDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting inspection data seeding...");

            // Check if inspection data already exists
            var existingInspections = await _context.Inspections.AnyAsync();
            if (existingInspections)
            {
                _logger.LogInformation("Inspection data already exists, skipping seeding.");
                return;
            }

            // Get users for assignment
            var users = await _context.Users.Take(10).ToListAsync();
            if (!users.Any())
            {
                _logger.LogWarning("No users found for inspection assignment.");
                return;
            }

            // Get departments
            var departments = await _context.Departments.Take(5).ToListAsync();
            if (!departments.Any())
            {
                _logger.LogWarning("No departments found for inspection assignment.");
                return;
            }

            // Create 100 sample inspections
            for (int i = 1; i <= 100; i++)
            {
                var inspectionType = GetRandomEnumValue<InspectionType>();
                var inspectionCategory = GetRandomEnumValue<InspectionCategory>();
                var priority = GetRandomEnumValue<InspectionPriority>();
                var status = GetRandomInspectionStatus();
                
                var inspector = users[_random.Next(users.Count)];
                var department = departments[_random.Next(departments.Count)];
                var scheduledDate = GenerateRandomScheduledDate();

                var inspection = Inspection.Create(
                    title: GetRandomTitle(inspectionType),
                    description: $"Detailed {inspectionType} inspection focusing on {inspectionCategory.ToString().ToLower()} assessment in {GetRandomLocation()}.",
                    type: inspectionType,
                    category: inspectionCategory,
                    priority: priority,
                    scheduledDate: scheduledDate,
                    inspectorId: inspector.Id,
                    locationId: 1, // Default location
                    departmentId: department.Id,
                    facilityId: 1 // Default facility
                );

                // Set status and dates based on inspection status
                switch (status)
                {
                    case InspectionStatus.Scheduled:
                        inspection.Schedule(scheduledDate);
                        break;
                    case InspectionStatus.InProgress:
                        inspection.Schedule(scheduledDate);
                        inspection.StartInspection();
                        break;
                    case InspectionStatus.Completed:
                        inspection.Schedule(scheduledDate);
                        inspection.StartInspection();
                        inspection.CompleteInspection(
                            summary: GenerateInspectionSummary(inspectionType),
                            recommendations: GenerateRecommendations()
                        );
                        break;
                    case InspectionStatus.Cancelled:
                        inspection.Cancel("Inspection cancelled due to operational requirements.");
                        break;
                }

                // Set created date to create realistic timeline
                SetInspectionTimestamps(inspection, scheduledDate, status);

                // Save inspection first to get the ID
                await _context.Inspections.AddAsync(inspection);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created inspection {i}: {inspection.Title}");

                // Create inspection items with correct inspection ID
                var itemCount = _random.Next(3, 8);
                var categoryItems = _inspectionItems[inspectionCategory];
                var items = new List<InspectionItem>();
                
                for (int j = 0; j < itemCount; j++)
                {
                    var itemDescription = categoryItems[_random.Next(categoryItems.Length)];
                    var isCompleted = status == InspectionStatus.Completed || 
                                    (status == InspectionStatus.InProgress && _random.NextDouble() > 0.3);
                    
                    var item = InspectionItem.Create(
                        inspectionId: inspection.Id,
                        question: itemDescription,
                        type: InspectionItemType.YesNo,
                        isRequired: _random.NextDouble() > 0.2,
                        description: "Inspection item generated during seeding",
                        sortOrder: j
                    );

                    if (isCompleted)
                    {
                        item.UpdateResponse(
                            response: GenerateItemResponse(),
                            notes: _random.NextDouble() > 0.5 ? GenerateItemNotes() : null
                        );
                    }

                    items.Add(item);
                }

                // Save inspection items
                await _context.InspectionItems.AddRangeAsync(items);
                await _context.SaveChangesAsync();

                // Create findings for completed inspections
                if (status == InspectionStatus.Completed)
                {
                    var findingCount = _random.Next(0, 4); // 0-3 findings per inspection
                    var findings = new List<InspectionFinding>();
                    
                    for (int f = 0; f < findingCount; f++)
                    {
                        var findingType = GetRandomEnumValue<FindingType>();
                        var severity = GetRandomFindingSeverity(findingType);
                        var responsiblePerson = users[_random.Next(users.Count)];

                        var finding = InspectionFinding.Create(
                            inspectionId: inspection.Id,
                            description: GetRandomFindingDescription(findingType),
                            type: findingType,
                            severity: severity,
                            location: GetRandomLocation(),
                            equipment: GenerateEquipmentName()
                        );

                        // Set additional properties
                        finding.SetRootCause(GenerateRootCause());
                        finding.SetImmediateAction(GenerateImmediateAction());
                        finding.SetCorrectiveAction(
                            GenerateCorrectiveAction(),
                            DateTime.UtcNow.AddDays(_random.Next(7, 90)),
                            responsiblePerson.Id
                        );

                        // Some findings might be closed
                        if (_random.NextDouble() > 0.8)
                        {
                            finding.MarkAsResolved();
                            finding.MarkAsVerified();
                            finding.Close("Corrective actions implemented and verified effective.");
                        }
                        else if (_random.NextDouble() > 0.6)
                        {
                            finding.MarkAsResolved();
                        }

                        findings.Add(finding);
                    }

                    // Save inspection findings
                    if (findings.Any())
                    {
                        await _context.InspectionFindings.AddRangeAsync(findings);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            _logger.LogInformation("Inspection data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding inspection data");
            throw;
        }
    }

    private T GetRandomEnumValue<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(_random.Next(values.Length))!;
    }

    private InspectionStatus GetRandomInspectionStatus()
    {
        // Weight the distribution to be more realistic
        var rand = _random.NextDouble();
        return rand switch
        {
            < 0.1 => InspectionStatus.Draft,
            < 0.25 => InspectionStatus.Scheduled,
            < 0.35 => InspectionStatus.InProgress,
            < 0.85 => InspectionStatus.Completed,
            _ => InspectionStatus.Cancelled
        };
    }

    private FindingSeverity GetRandomFindingSeverity(FindingType type)
    {
        // Positive findings are always low severity
        if (type == FindingType.PositiveFinding)
            return FindingSeverity.Minor;

        // Non-conformances tend to be higher severity
        if (type == FindingType.NonConformance)
        {
            var rand = _random.NextDouble();
            return rand switch
            {
                < 0.1 => FindingSeverity.Critical,
                < 0.3 => FindingSeverity.Major,
                < 0.6 => FindingSeverity.Moderate,
                _ => FindingSeverity.Minor
            };
        }

        // Others are typically lower severity
        return GetRandomEnumValue<FindingSeverity>();
    }

    private DateTime GenerateRandomScheduledDate()
    {
        var daysFromNow = _random.Next(-90, 30); // 90 days in past to 30 days in future
        return DateTime.UtcNow.AddDays(daysFromNow);
    }

    private void SetInspectionTimestamps(Inspection inspection, DateTime scheduledDate, InspectionStatus status)
    {
        // Use reflection to set creation timestamp for realistic data
        var createdAtProperty = typeof(Inspection).BaseType?.GetProperty("CreatedAt");
        var lastModifiedAtProperty = typeof(Inspection).BaseType?.GetProperty("LastModifiedAt");
        
        var createdDate = scheduledDate.AddDays(-_random.Next(1, 30)); // Created 1-30 days before scheduled
        
        createdAtProperty?.SetValue(inspection, createdDate);
        lastModifiedAtProperty?.SetValue(inspection, 
            status == InspectionStatus.Completed ? scheduledDate.AddDays(_random.Next(0, 7)) : createdDate);
    }

    private string GetRandomTitle(InspectionType type) => 
        _inspectionTitles[type][_random.Next(_inspectionTitles[type].Length)];

    private string GetRandomLocation() => 
        _locations[_random.Next(_locations.Length)];

    private string GetRandomFindingDescription(FindingType type) => 
        _findingDescriptions[type][_random.Next(_findingDescriptions[type].Length)];


    private string GenerateInspectionSummary(InspectionType type) =>
        $"{type} inspection completed successfully. All critical areas examined and documented. " +
        $"Overall compliance level found to be {(_random.NextDouble() > 0.3 ? "satisfactory" : "requiring improvement")} " +
        $"with {_random.Next(0, 5)} findings identified for follow-up action.";

    private string GenerateRecommendations() =>
        "Continue current maintenance schedules. Implement additional training where gaps identified. " +
        "Review and update procedures based on findings. Schedule follow-up inspection as required.";

    private string GenerateItemResponse() =>
        _random.NextDouble() switch
        {
            < 0.6 => "Compliant",
            < 0.8 => "Satisfactory",
            < 0.9 => "Needs Improvement", 
            _ => "Non-Compliant"
        };

    private string GenerateItemNotes() =>
        "Additional observations noted during inspection. Recommend continued monitoring.";

    private string GenerateEquipmentName() =>
        $"Equipment-{_random.Next(100, 999)}";

    private string GenerateRootCause() =>
        "Analysis indicates procedural deviation as primary contributing factor.";

    private string GenerateImmediateAction() =>
        "Area secured and personnel notified. Temporary controls implemented.";

    private string GenerateCorrectiveAction() =>
        "Implement revised procedures and provide additional training to affected personnel.";
}