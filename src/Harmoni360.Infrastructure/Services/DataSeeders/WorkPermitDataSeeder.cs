using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class WorkPermitDataSeeder : IDataSeeder
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<WorkPermitDataSeeder> _logger;
    private readonly Random _random = new();

    // Work locations
    private readonly string[] _locations = new[]
    {
        "Main Production Floor - Building A",
        "Warehouse Section B",
        "Rooftop Equipment Area",
        "Electrical Substation Room 101",
        "Chemical Storage Area C-12",
        "Confined Space - Tank T-301",
        "Loading Dock Area",
        "Maintenance Workshop",
        "Boiler Room - Basement",
        "Laboratory Building D",
        "Construction Site - North Wing",
        "Server Room - IT Building",
        "HVAC Plant Room",
        "Compressor Station",
        "Water Treatment Facility"
    };

    // Work descriptions
    private readonly Dictionary<WorkPermitType, string[]> _workDescriptions = new()
    {
        [WorkPermitType.HotWork] = new[]
        {
            "Welding repair on pipeline sections in chemical processing area",
            "Cutting and grinding operations for equipment modification",
            "Torch cutting of steel beams for structural repairs",
            "Brazing copper pipes in HVAC system maintenance",
            "Hot tapping on active process lines"
        },
        [WorkPermitType.ConfinedSpace] = new[]
        {
            "Tank cleaning and inspection in chemical storage area",
            "Manhole entry for sewer line maintenance",
            "Vessel internal repairs and coating application",
            "Duct cleaning in ventilation system",
            "Silo inspection and maintenance work"
        },
        [WorkPermitType.ElectricalWork] = new[]
        {
            "High voltage switchgear maintenance and testing",
            "Installation of new electrical panels in production area",
            "Cable pulling and termination for equipment upgrade",
            "Motor control center preventive maintenance",
            "Transformer inspection and oil sampling"
        },
        [WorkPermitType.ColdWork] = new[]
        {
            "Routine equipment inspection and lubrication",
            "Valve replacement in water treatment system",
            "Insulation installation on piping systems",
            "Scaffolding erection for maintenance access",
            "Non-sparking tool maintenance activities"
        },
        [WorkPermitType.General] = new[]
        {
            "General housekeeping and area cleanup",
            "Equipment relocation and positioning",
            "Painting and surface preparation work",
            "Material handling and storage organization",
            "Documentation and labeling updates"
        },
        [WorkPermitType.Special] = new[]
        {
            "Asbestos removal and disposal operations",
            "Radiation source handling and storage",
            "Major excavation near underground utilities",
            "Critical lift operations with crane",
            "Hazardous chemical transfer operations"
        }
    };

    // Safety precautions by work type
    private readonly Dictionary<WorkPermitType, string[]> _safetyPrecautions = new()
    {
        [WorkPermitType.HotWork] = new[]
        {
            "Fire extinguisher must be readily available",
            "Fire watch required for 30 minutes after completion",
            "Remove all combustible materials within 35 feet",
            "Gas monitoring required before and during work",
            "Hot work screens/blankets must be in place"
        },
        [WorkPermitType.ConfinedSpace] = new[]
        {
            "Continuous atmospheric monitoring required",
            "Standby person must be present at all times",
            "Emergency rescue equipment must be available",
            "Ventilation system must be operational",
            "Entry permit must be posted at entry point"
        },
        [WorkPermitType.ElectricalWork] = new[]
        {
            "Lock-out/Tag-out procedures must be followed",
            "Voltage testing required before work begins",
            "Insulated tools and PPE required",
            "Barricades and warning signs must be posted",
            "Ground fault protection must be verified"
        }
    };

    // PPE requirements
    private readonly string[] _ppeRequirements = new[]
    {
        "Safety helmet, safety glasses, steel-toed boots",
        "Full face shield, chemical resistant gloves, safety boots",
        "Arc flash suit, insulated gloves, face shield",
        "SCBA or supplied air, full body harness, safety boots",
        "Cut-resistant gloves, safety glasses, high-visibility vest"
    };

    // Supervisor names
    private readonly string[] _supervisors = new[]
    {
        "Robert Johnson", "Sarah Williams", "Michael Chen", "Jennifer Martinez",
        "David Thompson", "Lisa Anderson", "James Wilson", "Maria Garcia",
        "John Smith", "Emily Davis", "Christopher Lee", "Amanda Taylor"
    };

    // Safety officers
    private readonly string[] _safetyOfficers = new[]
    {
        "Thomas Brown", "Jessica Rodriguez", "William Moore", "Patricia Jackson",
        "Richard White", "Linda Harris", "Joseph Martin", "Barbara Lewis"
    };

    // Contractor companies
    private readonly string?[] _contractorCompanies = new string?[]
    {
        "Industrial Services Corp", "SafeWork Contractors Ltd", "Technical Solutions Inc",
        "ProMaintenance Services", "Specialized Construction Co", "Elite Engineering Services",
        null, null, null // Some permits won't have contractors
    };

    public WorkPermitDataSeeder(IApplicationDbContext context, ILogger<WorkPermitDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check for force reseed environment variable
            var forceReseed = Environment.GetEnvironmentVariable("HARMONI_FORCE_RESEED") == "true";
            
            // Check if we already have work permits (skip if not force reseeding)
            if (!forceReseed && await _context.WorkPermits.AnyAsync())
            {
                _logger.LogInformation("Work permits already exist, skipping seeding.");
                return;
            }

            _logger.LogInformation("Starting work permit data seeding...");

            var users = await _context.Users.ToListAsync();
            if (!users.Any())
            {
                _logger.LogWarning("No users found. Skipping work permit seeding.");
                return;
            }

            var workPermits = new List<WorkPermit>();
            var permitCounter = 1;
            var createdPermitNumbers = new HashSet<string>();

            // Helper function to create unique work permit
            WorkPermit CreateUniqueWorkPermit(WorkPermitStatus status, int counter)
            {
                var permit = CreateWorkPermit(users, status, counter);
                
                // If duplicate permit number, override with reflection to ensure uniqueness
                while (createdPermitNumbers.Contains(permit.PermitNumber))
                {
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + counter;
                    var type = permit.Type;
                    var prefix = type switch
                    {
                        WorkPermitType.HotWork => "HW",
                        WorkPermitType.ColdWork => "CW",
                        WorkPermitType.ConfinedSpace => "CS",
                        WorkPermitType.ElectricalWork => "EW",
                        WorkPermitType.Special => "SP",
                        WorkPermitType.General => "GP",
                        _ => "WP"
                    };
                    var year = DateTime.UtcNow.Year;
                    var month = DateTime.UtcNow.Month;
                    var uniqueNumber = $"{prefix}-{year:D4}{month:D2}-{(timestamp + counter) % 10000:D4}";
                    
                    // Use reflection to set the permit number
                    permit.GetType().GetProperty("PermitNumber")?.SetValue(permit, uniqueNumber);
                    counter++;
                }
                
                createdPermitNumbers.Add(permit.PermitNumber);
                return permit;
            }

            // Create work permits with various statuses
            // Draft permits (5)
            for (int i = 0; i < 5; i++)
            {
                workPermits.Add(CreateUniqueWorkPermit(WorkPermitStatus.Draft, permitCounter++));
            }

            // Pending approval permits (8)
            for (int i = 0; i < 8; i++)
            {
                workPermits.Add(CreateUniqueWorkPermit(WorkPermitStatus.PendingApproval, permitCounter++));
            }

            // Approved permits (6)
            for (int i = 0; i < 6; i++)
            {
                workPermits.Add(CreateUniqueWorkPermit(WorkPermitStatus.Approved, permitCounter++));
            }

            // In progress permits (4)
            for (int i = 0; i < 4; i++)
            {
                workPermits.Add(CreateUniqueWorkPermit(WorkPermitStatus.InProgress, permitCounter++));
            }

            // Completed permits (5)
            for (int i = 0; i < 5; i++)
            {
                workPermits.Add(CreateUniqueWorkPermit(WorkPermitStatus.Completed, permitCounter++));
            }

            // Rejected permits (2)
            for (int i = 0; i < 2; i++)
            {
                workPermits.Add(CreateUniqueWorkPermit(WorkPermitStatus.Rejected, permitCounter++));
            }

            // Cancelled permits (2)
            for (int i = 0; i < 2; i++)
            {
                workPermits.Add(CreateUniqueWorkPermit(WorkPermitStatus.Cancelled, permitCounter++));
            }

            // Expired permits (2)
            for (int i = 0; i < 2; i++)
            {
                workPermits.Add(CreateUniqueWorkPermit(WorkPermitStatus.Expired, permitCounter++));
            }

            _logger.LogInformation($"Created {workPermits.Count} work permits with unique permit numbers, saving to database...");
            
            // Validate permit number uniqueness before saving
            var allPermitNumbers = workPermits.Select(wp => wp.PermitNumber).ToList();
            var duplicates = allPermitNumbers.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            
            if (duplicates.Any())
            {
                _logger.LogError($"Found duplicate permit numbers: {string.Join(", ", duplicates)}");
                throw new InvalidOperationException($"Duplicate permit numbers detected: {string.Join(", ", duplicates)}");
            }
            
            // Log some permit numbers for verification
            var samplePermitNumbers = workPermits.Take(5).Select(wp => wp.PermitNumber).ToList();
            _logger.LogInformation($"Sample permit numbers: {string.Join(", ", samplePermitNumbers)}");
            
            _context.WorkPermits.AddRange(workPermits);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully seeded {workPermits.Count} work permits.");

            // Add hazards and precautions to some permits
            foreach (var permit in workPermits.Where(p => p.Status != WorkPermitStatus.Draft).Take(20))
            {
                AddHazardsAndPrecautions(permit);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully added hazards and precautions to work permits.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding work permit data");
            throw;
        }
    }

    private WorkPermit CreateWorkPermit(List<User> users, WorkPermitStatus status, int index)
    {
        var type = (WorkPermitType)_random.Next(0, 6);
        var priority = (WorkPermitPriority)_random.Next(0, 4);
        var requestor = users[_random.Next(users.Count)];
        var createdDate = DateTime.UtcNow.AddDays(-_random.Next(1, 60));
        
        // Determine dates based on status
        DateTime plannedStartDate;
        DateTime plannedEndDate;
        DateTime? actualStartDate = null;
        DateTime? actualEndDate = null;

        switch (status)
        {
            case WorkPermitStatus.Draft:
            case WorkPermitStatus.PendingApproval:
                plannedStartDate = DateTime.UtcNow.AddDays(_random.Next(1, 14));
                plannedEndDate = plannedStartDate.AddDays(_random.Next(1, 7));
                break;
            case WorkPermitStatus.Approved:
                plannedStartDate = DateTime.UtcNow.AddDays(_random.Next(-3, 7));
                plannedEndDate = plannedStartDate.AddDays(_random.Next(1, 5));
                break;
            case WorkPermitStatus.InProgress:
                plannedStartDate = DateTime.UtcNow.AddDays(-_random.Next(1, 3));
                plannedEndDate = plannedStartDate.AddDays(_random.Next(2, 7));
                actualStartDate = plannedStartDate.AddHours(_random.Next(0, 4));
                break;
            case WorkPermitStatus.Completed:
                plannedStartDate = DateTime.UtcNow.AddDays(-_random.Next(7, 30));
                plannedEndDate = plannedStartDate.AddDays(_random.Next(1, 5));
                actualStartDate = plannedStartDate.AddHours(_random.Next(0, 4));
                actualEndDate = actualStartDate.Value.AddDays(_random.Next(1, 3));
                break;
            case WorkPermitStatus.Expired:
                plannedStartDate = DateTime.UtcNow.AddDays(-_random.Next(10, 30));
                plannedEndDate = DateTime.UtcNow.AddDays(-_random.Next(1, 5));
                break;
            default:
                plannedStartDate = DateTime.UtcNow.AddDays(-_random.Next(5, 20));
                plannedEndDate = plannedStartDate.AddDays(_random.Next(1, 5));
                break;
        }

        var workScope = _workDescriptions[type][_random.Next(_workDescriptions[type].Length)];
        var location = _locations[_random.Next(_locations.Length)];
        var supervisor = _supervisors[_random.Next(_supervisors.Length)];
        var contractorCompany = _contractorCompanies[_random.Next(_contractorCompanies.Length)];

        var permit = WorkPermit.Create(
            $"{type} work at {location}",
            workScope,
            type,
            location,
            plannedStartDate,
            plannedEndDate,
            requestor.Id,
            requestor.Name,
            requestor.Department,
            requestor.Position,
            $"+62 812 {_random.Next(1000, 9999)} {_random.Next(1000, 9999)}",
            workScope,
            _random.Next(2, 15)
        );

        // Set safety requirements based on type using proper domain methods
        SetSafetyRequirements(permit, type);

        // Set Indonesian compliance for some permits
        if (_random.Next(100) < 70) // 70% have K3 compliance
        {
            permit.SetIndonesianCompliance(
                $"K3-{DateTime.Now.Year}-{_random.Next(1000, 9999)}",
                contractorCompany != null ? $"CWP-{_random.Next(1000, 9999)}" : "",
                true,
                _random.Next(100) < 80,
                type == WorkPermitType.Special ? $"ENV-{DateTime.Now.Year}-{_random.Next(100, 999)}" : ""
            );
        }

        // Set additional properties using reflection (for properties without proper domain methods)
        var permitType = permit.GetType();
        permitType.GetProperty("Status")?.SetValue(permit, status);
        permitType.GetProperty("Priority")?.SetValue(permit, priority);
        permitType.GetProperty("CreatedAt")?.SetValue(permit, createdDate);
        permitType.GetProperty("EquipmentToBeUsed")?.SetValue(permit, GetEquipmentForType(type));
        permitType.GetProperty("MaterialsInvolved")?.SetValue(permit, GetMaterialsForType(type));
        permitType.GetProperty("WorkSupervisor")?.SetValue(permit, supervisor);
        
        if (!string.IsNullOrEmpty(contractorCompany))
        {
            permitType.GetProperty("ContractorCompany")?.SetValue(permit, contractorCompany);
        }

        // Set risk assessment
        permitType.GetProperty("RiskAssessmentSummary")?.SetValue(permit, 
            $"Risk assessment conducted for {type} work. Identified hazards include: {GetHazardsForType(type)}. " +
            $"Control measures implemented. Residual risk level: {GetRiskLevelForType(type)}.");

        // Set emergency procedures for high-risk work
        if (IsHighRiskWork(type))
        {
            permitType.GetProperty("EmergencyProcedures")?.SetValue(permit,
                "1. Stop work immediately\n" +
                "2. Alert all personnel in area\n" +
                "3. Contact emergency response team: Ext. 911\n" +
                "4. Evacuate if necessary following posted routes\n" +
                "5. Report to assembly point and await further instructions");
        }

        // Add status-specific data
        switch (status)
        {
            case WorkPermitStatus.Approved:
            case WorkPermitStatus.InProgress:
            case WorkPermitStatus.Completed:
                if (_random.Next(100) < 80)
                {
                    permitType.GetProperty("SafetyOfficer")?.SetValue(permit, 
                        _safetyOfficers[_random.Next(_safetyOfficers.Length)]);
                }
                break;
        }

        return permit;
    }

    private void SetSafetyRequirements(WorkPermit permit, WorkPermitType type)
    {
        switch (type)
        {
            case WorkPermitType.HotWork:
                permit.SetSafetyRequirements(
                    requiresHotWork: true,
                    requiresFireWatch: true);
                break;
            case WorkPermitType.ConfinedSpace:
                permit.SetSafetyRequirements(
                    requiresConfinedSpace: true,
                    requiresGasMonitoring: true);
                break;
            case WorkPermitType.ElectricalWork:
                permit.SetSafetyRequirements(
                    requiresElectricalIsolation: true);
                break;
            case WorkPermitType.Special:
                // Randomly set various requirements
                permit.SetSafetyRequirements(
                    requiresHeightWork: _random.Next(100) < 40,
                    requiresRadiationWork: _random.Next(100) < 20,
                    requiresExcavation: _random.Next(100) < 30);
                break;
        }
    }

    private void AddHazardsAndPrecautions(WorkPermit permit)
    {
        // Add 2-4 hazards
        var hazardCount = _random.Next(2, 5);
        for (int i = 0; i < hazardCount; i++)
        {
            var category = (Domain.Enums.HazardCategory)_random.Next(0, 10);
            var likelihood = _random.Next(1, 6);
            var severity = _random.Next(1, 6);
            var riskLevel = CalculateRiskLevel(likelihood, severity);

            permit.AddHazard(
                GetHazardDescription(category),
                (int)category + 1, // Category ID
                riskLevel,
                GetControlMeasures(category),
                _supervisors[_random.Next(_supervisors.Length)]
            );
        }

        // Add 3-6 precautions
        var precautionCount = _random.Next(3, 7);
        var precautionCategories = Enum.GetValues<PrecautionCategory>();
        
        for (int i = 0; i < precautionCount; i++)
        {
            var category = precautionCategories[_random.Next(precautionCategories.Length)];
            var isRequired = _random.Next(100) < 80; // 80% are required
            var priority = _random.Next(1, 6);

            permit.AddPrecaution(
                GetPrecautionDescription(category),
                category,
                isRequired,
                priority,
                _supervisors[_random.Next(_supervisors.Length)],
                "Visual inspection and sign-off",
                category == PrecautionCategory.K3_Compliance,
                category == PrecautionCategory.K3_Compliance ? $"SNI-{_random.Next(1000, 9999)}" : "",
                _random.Next(100) < 30 // 30% are mandatory by law
            );
        }
    }

    private string GetWorkTypePrefix(WorkPermitType type) => type switch
    {
        WorkPermitType.HotWork => "HW",
        WorkPermitType.ConfinedSpace => "CS",
        WorkPermitType.ElectricalWork => "EL",
        WorkPermitType.ColdWork => "CW",
        WorkPermitType.Special => "SP",
        _ => "GP"
    };

    private RiskLevel GetRiskLevelForType(WorkPermitType type) => type switch
    {
        WorkPermitType.HotWork => _random.Next(100) < 70 ? RiskLevel.High : RiskLevel.Medium,
        WorkPermitType.ConfinedSpace => RiskLevel.High,
        WorkPermitType.ElectricalWork => _random.Next(100) < 60 ? RiskLevel.High : RiskLevel.Medium,
        WorkPermitType.Special => _random.Next(100) < 80 ? RiskLevel.Critical : RiskLevel.High,
        WorkPermitType.ColdWork => _random.Next(100) < 80 ? RiskLevel.Low : RiskLevel.Medium,
        _ => _random.Next(100) < 70 ? RiskLevel.Low : RiskLevel.Medium
    };

    private bool IsHighRiskWork(WorkPermitType type) =>
        type == WorkPermitType.HotWork || 
        type == WorkPermitType.ConfinedSpace || 
        type == WorkPermitType.Special ||
        type == WorkPermitType.ElectricalWork;

    private string GetEquipmentForType(WorkPermitType type) => type switch
    {
        WorkPermitType.HotWork => "Welding machine, cutting torch, grinding equipment, fire blankets, spark screens",
        WorkPermitType.ConfinedSpace => "Gas monitor, ventilation blower, retrieval tripod, SCBA, communication equipment",
        WorkPermitType.ElectricalWork => "Multimeter, insulated tools, lockout devices, voltage detectors, arc flash PPE",
        WorkPermitType.ColdWork => "Hand tools, power tools, ladders, material handling equipment",
        WorkPermitType.Special => "Specialized equipment as per work scope, monitoring devices, emergency equipment",
        _ => "Standard hand tools, measuring equipment, safety barriers"
    };

    private string GetMaterialsForType(WorkPermitType type) => type switch
    {
        WorkPermitType.HotWork => "Welding rods, grinding discs, metal components, fire-resistant materials",
        WorkPermitType.ConfinedSpace => "Cleaning chemicals, replacement parts, coating materials",
        WorkPermitType.ElectricalWork => "Electrical cables, connectors, circuit breakers, insulation materials",
        WorkPermitType.ColdWork => "Replacement parts, lubricants, gaskets, fasteners",
        WorkPermitType.Special => "Specialized materials as per work scope, containment materials",
        _ => "General maintenance supplies, cleaning materials"
    };

    private string GetHazardsForType(WorkPermitType type) => type switch
    {
        WorkPermitType.HotWork => "fire/explosion risk, burns, toxic fumes, UV radiation",
        WorkPermitType.ConfinedSpace => "oxygen deficiency, toxic atmosphere, engulfment, entrapment",
        WorkPermitType.ElectricalWork => "electrical shock, arc flash, burns, electrocution",
        WorkPermitType.Special => "multiple hazards depending on specific work nature",
        _ => "general industrial hazards"
    };

    private RiskLevel CalculateRiskLevel(int likelihood, int severity)
    {
        var riskScore = likelihood * severity;
        return riskScore switch
        {
            >= 20 => RiskLevel.Critical,
            >= 15 => RiskLevel.High,
            >= 10 => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    private string GetHazardDescription(Domain.Enums.HazardCategory category) => category switch
    {
        Domain.Enums.HazardCategory.Physical => "Exposure to moving machinery parts and equipment",
        Domain.Enums.HazardCategory.Chemical => "Potential exposure to hazardous chemicals and vapors",
        Domain.Enums.HazardCategory.Biological => "Risk of exposure to biological contaminants",
        Domain.Enums.HazardCategory.Ergonomic => "Repetitive motion and awkward working positions",
        Domain.Enums.HazardCategory.Fire => "Fire and explosion hazards from hot work activities",
        Domain.Enums.HazardCategory.Electrical => "Electrical shock hazard from energized equipment",
        Domain.Enums.HazardCategory.Mechanical => "Crushing or impact hazards from equipment",
        Domain.Enums.HazardCategory.Environmental => "Environmental conditions affecting work safety",
        Domain.Enums.HazardCategory.Radiological => "Potential radiation exposure",
        _ => "General workplace hazards"
    };

    private string GetControlMeasures(Domain.Enums.HazardCategory category) => category switch
    {
        Domain.Enums.HazardCategory.Physical => "Machine guarding, lockout/tagout, barriers, warning signs",
        Domain.Enums.HazardCategory.Chemical => "Ventilation, PPE, spill containment, SDS review",
        Domain.Enums.HazardCategory.Biological => "PPE, hygiene protocols, vaccination if required",
        Domain.Enums.HazardCategory.Ergonomic => "Proper lifting techniques, ergonomic tools, regular breaks",
        Domain.Enums.HazardCategory.Fire => "Fire watch, remove combustibles, fire extinguishers ready",
        Domain.Enums.HazardCategory.Electrical => "De-energization, LOTO, insulated tools, rubber mats",
        Domain.Enums.HazardCategory.Mechanical => "Guards in place, safety devices operational, clear zones",
        Domain.Enums.HazardCategory.Environmental => "Weather monitoring, proper lighting, ventilation",
        Domain.Enums.HazardCategory.Radiological => "Shielding, distance, time limits, monitoring",
        _ => "Standard safety procedures and PPE"
    };

    private string GetPrecautionDescription(PrecautionCategory category) => category switch
    {
        PrecautionCategory.PersonalProtectiveEquipment => _ppeRequirements[_random.Next(_ppeRequirements.Length)],
        PrecautionCategory.Isolation => "Equipment locked out and tagged, energy sources verified zero",
        PrecautionCategory.FireSafety => _safetyPrecautions[WorkPermitType.HotWork][_random.Next(_safetyPrecautions[WorkPermitType.HotWork].Length)],
        PrecautionCategory.GasMonitoring => "Continuous gas monitoring for O2, LEL, H2S, CO",
        PrecautionCategory.AccessControl => "Area barricaded, warning signs posted, entry controlled",
        PrecautionCategory.EmergencyProcedures => "Emergency contact numbers posted, evacuation route clear",
        PrecautionCategory.K3_Compliance => "Work complies with Indonesian K3 safety standards",
        _ => "General safety precautions as per work scope"
    };
}