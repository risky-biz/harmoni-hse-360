using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class HealthDataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthDataSeeder> _logger;
    private readonly IConfiguration _configuration;

    public HealthDataSeeder(ApplicationDbContext context, ILogger<HealthDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Check if we should re-seed health data even if it exists
        var reSeedHealthData = _configuration["DataSeeding:ReSeedHealthData"] == "true";

        if (!reSeedHealthData && (await _context.HealthRecords.AnyAsync() || await _context.MedicalConditions.AnyAsync()))
        {
            _logger.LogInformation("Health data already exists and ReSeedHealthData is false, skipping health data seeding");
            return;
        }

        _logger.LogInformation("Starting health data seeding...");

        // If re-seeding is enabled, clear existing data first
        if (reSeedHealthData)
        {
            if (await _context.HealthIncidents.AnyAsync())
            {
                _logger.LogInformation("Clearing existing health incidents for re-seeding...");
                _context.HealthIncidents.RemoveRange(_context.HealthIncidents);
            }
            if (await _context.EmergencyContacts.AnyAsync())
            {
                _logger.LogInformation("Clearing existing emergency contacts for re-seeding...");
                _context.EmergencyContacts.RemoveRange(_context.EmergencyContacts);
            }
            if (await _context.VaccinationRecords.AnyAsync())
            {
                _logger.LogInformation("Clearing existing vaccination records for re-seeding...");
                _context.VaccinationRecords.RemoveRange(_context.VaccinationRecords);
            }
            if (await _context.MedicalConditions.AnyAsync())
            {
                _logger.LogInformation("Clearing existing medical conditions for re-seeding...");
                _context.MedicalConditions.RemoveRange(_context.MedicalConditions);
            }
            if (await _context.HealthRecords.AnyAsync())
            {
                _logger.LogInformation("Clearing existing health records for re-seeding...");
                _context.HealthRecords.RemoveRange(_context.HealthRecords);
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing health data cleared");
        }

        await SeedHealthRecordsAsync();
        await SeedMedicalConditionsAsync();
        await SeedVaccinationRecordsAsync();
        await SeedEmergencyContactsAsync();
        await SeedHealthIncidentsAsync();
    }

    private async Task SeedHealthRecordsAsync()
    {
        _logger.LogInformation("Starting health record seeding...");

        // Get all users to create health records for them
        var users = await _context.Users.ToListAsync();
        var healthRecords = new List<HealthRecord>();
        var random = new Random();

        foreach (var user in users)
        {
            // Determine person type based on email domain and role
            var personType = DeterminePersonType(user.Email);
            
            // Generate random date of birth (18-65 years old)
            var age = random.Next(18, 66);
            var dateOfBirth = DateTime.UtcNow.AddYears(-age).AddDays(random.Next(-365, 365));
            
            // Random blood type (some may not have it recorded)
            var bloodType = random.NextDouble() < 0.8 ? GetRandomBloodType(random) : (BloodType?)null;
            
            // Some medical notes
            var medicalNotes = GenerateMedicalNotes(personType, random);

            var healthRecord = HealthRecord.Create(
                personId: user.Id,
                personType: personType,
                dateOfBirth: dateOfBirth,
                bloodType: bloodType,
                medicalNotes: medicalNotes
            );

            healthRecords.Add(healthRecord);
        }

        await _context.HealthRecords.AddRangeAsync(healthRecords);
        _logger.LogInformation("Seeded {Count} health records", healthRecords.Count);
    }

    private async Task SeedMedicalConditionsAsync()
    {
        _logger.LogInformation("Starting medical condition seeding...");

        // Get health records
        var healthRecords = await _context.HealthRecords.ToListAsync();
        var medicalConditions = new List<MedicalCondition>();
        var random = new Random();

        // Common medical conditions for seeding
        var conditionTemplates = new[]
        {
            new { Type = ConditionType.Allergy, Name = "Peanut Allergy", Severity = ConditionSeverity.Severe, Emergency = true, Instructions = "Use EpiPen immediately, call 911" },
            new { Type = ConditionType.Allergy, Name = "Bee Sting Allergy", Severity = ConditionSeverity.Moderate, Emergency = true, Instructions = "Monitor for swelling, seek medical attention if severe" },
            new { Type = ConditionType.ChronicCondition, Name = "Asthma", Severity = ConditionSeverity.Moderate, Emergency = false, Instructions = "Use inhaler as prescribed" },
            new { Type = ConditionType.ChronicCondition, Name = "Type 1 Diabetes", Severity = ConditionSeverity.Severe, Emergency = true, Instructions = "Monitor blood sugar, have glucose tablets available" },
            new { Type = ConditionType.MedicationDependency, Name = "Daily Insulin", Severity = ConditionSeverity.Severe, Emergency = true, Instructions = "Ensure medication schedule is maintained" },
            new { Type = ConditionType.PhysicalLimitation, Name = "Mobility Issues", Severity = ConditionSeverity.Mild, Emergency = false, Instructions = "Ensure accessible facilities" },
            new { Type = ConditionType.Dietary, Name = "Celiac Disease", Severity = ConditionSeverity.Moderate, Emergency = false, Instructions = "Strict gluten-free diet required" },
            new { Type = ConditionType.MentalHealthCondition, Name = "Anxiety Disorder", Severity = ConditionSeverity.Mild, Emergency = false, Instructions = "Provide calm environment during episodes" },
            new { Type = ConditionType.Allergy, Name = "Latex Allergy", Severity = ConditionSeverity.Moderate, Emergency = false, Instructions = "Use non-latex gloves and equipment" },
            new { Type = ConditionType.Other, Name = "Epilepsy", Severity = ConditionSeverity.Severe, Emergency = true, Instructions = "Do not restrain during seizure, time episode, call for help" }
        };

        // Assign medical conditions to some health records (about 30% have conditions)
        var recordsWithConditions = healthRecords.Take((int)(healthRecords.Count * 0.3)).ToList();

        foreach (var healthRecord in recordsWithConditions)
        {
            // Each person gets 1-3 conditions
            var conditionCount = random.Next(1, 4);
            var selectedConditions = conditionTemplates.OrderBy(x => random.Next()).Take(conditionCount);

            foreach (var template in selectedConditions)
            {
                var diagnosedDate = DateTime.UtcNow.AddDays(-random.Next(30, 1825)); // 1 month to 5 years ago
                
                var medicalCondition = MedicalCondition.Create(
                    healthRecordId: healthRecord.Id,
                    type: template.Type,
                    name: template.Name,
                    description: $"Diagnosed {template.Name.ToLower()} requiring ongoing management",
                    severity: template.Severity,
                    treatmentPlan: GenerateTreatmentPlan(template.Name),
                    diagnosedDate: diagnosedDate,
                    requiresEmergencyAction: template.Emergency,
                    emergencyInstructions: template.Emergency ? template.Instructions : null
                );

                medicalConditions.Add(medicalCondition);
            }
        }

        await _context.MedicalConditions.AddRangeAsync(medicalConditions);
        _logger.LogInformation("Seeded {Count} medical conditions", medicalConditions.Count);
    }

    private async Task SeedVaccinationRecordsAsync()
    {
        _logger.LogInformation("Starting vaccination record seeding...");

        // Get health records
        var healthRecords = await _context.HealthRecords.ToListAsync();
        var vaccinationRecords = new List<VaccinationRecord>();
        var random = new Random();

        // Common vaccinations required for school/work
        var requiredVaccines = new[]
        {
            "COVID-19", "Influenza (Annual)", "Hepatitis B", "MMR (Measles, Mumps, Rubella)",
            "DPT (Diphtheria, Pertussis, Tetanus)", "Polio", "Varicella (Chickenpox)"
        };

        var optionalVaccines = new[]
        {
            "HPV", "Meningococcal", "Pneumococcal", "Hepatitis A", "Japanese Encephalitis"
        };

        foreach (var healthRecord in healthRecords)
        {
            // All records get required vaccines
            foreach (var vaccine in requiredVaccines)
            {
                var isAnnual = vaccine.Contains("Annual");
                var administered = random.NextDouble() < 0.9; // 90% have required vaccines

                DateTime? dateAdministered = null;
                DateTime? expiryDate = null;
                
                if (administered)
                {
                    // Administered within last 1-3 years for most vaccines
                    var daysBack = isAnnual ? random.Next(30, 365) : random.Next(365, 1095);
                    dateAdministered = DateTime.UtcNow.AddDays(-daysBack);
                    
                    // Set expiry for vaccines that expire
                    if (isAnnual)
                        expiryDate = dateAdministered.Value.AddDays(365);
                    else if (vaccine.Contains("Tetanus") || vaccine.Contains("DPT"))
                        expiryDate = dateAdministered.Value.AddYears(10);
                }

                var vaccination = VaccinationRecord.Create(
                    healthRecordId: healthRecord.Id,
                    vaccineName: vaccine,
                    isRequired: true,
                    dateAdministered: dateAdministered,
                    expiryDate: expiryDate,
                    batchNumber: administered ? $"BATCH-{random.Next(1000, 9999)}" : null,
                    administeredBy: administered ? GetRandomProvider(random) : null,
                    administrationLocation: administered ? GetRandomLocation(random) : null,
                    notes: GenerateVaccineNotes(vaccine, administered, random)
                );

                vaccinationRecords.Add(vaccination);
            }

            // Some get optional vaccines (30% chance)
            if (random.NextDouble() < 0.3)
            {
                var optionalCount = random.Next(1, 3);
                var selectedOptional = optionalVaccines.OrderBy(x => random.Next()).Take(optionalCount);

                foreach (var vaccine in selectedOptional)
                {
                    var administered = random.NextDouble() < 0.7; // 70% completion rate for optional
                    
                    DateTime? dateAdministered = null;
                    if (administered)
                    {
                        dateAdministered = DateTime.UtcNow.AddDays(-random.Next(180, 1095));
                    }

                    var vaccination = VaccinationRecord.Create(
                        healthRecordId: healthRecord.Id,
                        vaccineName: vaccine,
                        isRequired: false,
                        dateAdministered: dateAdministered,
                        batchNumber: administered ? $"OPT-{random.Next(1000, 9999)}" : null,
                        administeredBy: administered ? GetRandomProvider(random) : null,
                        administrationLocation: administered ? GetRandomLocation(random) : null,
                        notes: $"Optional vaccination for {vaccine}"
                    );

                    vaccinationRecords.Add(vaccination);
                }
            }
        }

        await _context.VaccinationRecords.AddRangeAsync(vaccinationRecords);
        _logger.LogInformation("Seeded {Count} vaccination records", vaccinationRecords.Count);
    }

    private async Task SeedEmergencyContactsAsync()
    {
        _logger.LogInformation("Starting emergency contact seeding...");

        // Get health records
        var healthRecords = await _context.HealthRecords.ToListAsync();
        var emergencyContacts = new List<EmergencyContact>();
        var random = new Random();

        foreach (var healthRecord in healthRecords)
        {
            // Each person gets 1-3 emergency contacts
            var contactCount = random.Next(1, 4);
            
            for (int i = 1; i <= contactCount; i++)
            {
                var relationship = GetRandomRelationship(random, i == 1);
                var isPrimary = i == 1;
                
                var contact = EmergencyContact.Create(
                    healthRecordId: healthRecord.Id,
                    name: GenerateContactName(relationship, random),
                    relationship: relationship,
                    primaryPhone: GeneratePhoneNumber(random),
                    email: random.NextDouble() < 0.8 ? GenerateEmail(random) : null,
                    secondaryPhone: random.NextDouble() < 0.3 ? GeneratePhoneNumber(random) : null,
                    address: random.NextDouble() < 0.6 ? GenerateAddress(random) : null,
                    isPrimaryContact: isPrimary,
                    authorizedForPickup: relationship == ContactRelationship.Parent || relationship == ContactRelationship.Guardian || random.NextDouble() < 0.5,
                    authorizedForMedicalDecisions: relationship == ContactRelationship.Parent || relationship == ContactRelationship.Guardian || (relationship == ContactRelationship.Spouse && random.NextDouble() < 0.8),
                    customRelationship: relationship == ContactRelationship.Other ? "Family Friend" : null,
                    notes: random.NextDouble() < 0.3 ? "Available 24/7 for emergencies" : null
                );

                contact.SetContactOrder(i);
                emergencyContacts.Add(contact);
            }
        }

        await _context.EmergencyContacts.AddRangeAsync(emergencyContacts);
        _logger.LogInformation("Seeded {Count} emergency contacts", emergencyContacts.Count);
    }

    private async Task SeedHealthIncidentsAsync()
    {
        _logger.LogInformation("Starting health incident seeding...");

        // Get health records
        var healthRecords = await _context.HealthRecords.ToListAsync();
        var healthIncidents = new List<HealthIncident>();
        var random = new Random();

        // Sample health incident templates
        var incidentTemplates = new[]
        {
            new { Type = HealthIncidentType.Injury, Severity = HealthIncidentSeverity.Minor, Symptoms = "Minor cut on hand", Treatment = "Cleaned wound, applied bandage, tetanus shot up to date", Location = TreatmentLocation.SchoolNurse },
            new { Type = HealthIncidentType.AllergicReaction, Severity = HealthIncidentSeverity.Serious, Symptoms = "Swelling, difficulty breathing after eating peanuts", Treatment = "EpiPen administered, emergency services called", Location = TreatmentLocation.Hospital },
            new { Type = HealthIncidentType.Illness, Severity = HealthIncidentSeverity.Moderate, Symptoms = "Fever, headache, nausea", Treatment = "Rest, fluids, fever reducer, sent home", Location = TreatmentLocation.SchoolNurse },
            new { Type = HealthIncidentType.MedicationIssue, Severity = HealthIncidentSeverity.Minor, Symptoms = "Forgot daily medication", Treatment = "Contacted parent, medication administered", Location = TreatmentLocation.SchoolNurse },
            new { Type = HealthIncidentType.ChronicConditionFlareUp, Severity = HealthIncidentSeverity.Moderate, Symptoms = "Asthma attack during PE", Treatment = "Inhaler used, rest period, monitored breathing", Location = TreatmentLocation.Classroom },
            new { Type = HealthIncidentType.Injury, Severity = HealthIncidentSeverity.Serious, Symptoms = "Sprained ankle during sports", Treatment = "Ice applied, ankle wrapped, x-ray taken", Location = TreatmentLocation.Clinic },
            new { Type = HealthIncidentType.MentalHealthEpisode, Severity = HealthIncidentSeverity.Moderate, Symptoms = "Anxiety attack before exam", Treatment = "Counseling, breathing exercises, parents notified", Location = TreatmentLocation.SchoolNurse },
            new { Type = HealthIncidentType.Other, Severity = HealthIncidentSeverity.Minor, Symptoms = "Headache and dizziness", Treatment = "Rest, water, blood pressure checked", Location = TreatmentLocation.SchoolNurse }
        };

        // Create incidents for some health records (about 20% have had health incidents)
        var recordsWithIncidents = healthRecords.Take((int)(healthRecords.Count * 0.2)).ToList();

        foreach (var healthRecord in recordsWithIncidents)
        {
            // Each person gets 1-2 health incidents
            var incidentCount = random.Next(1, 3);
            var selectedIncidents = incidentTemplates.OrderBy(x => random.Next()).Take(incidentCount);

            foreach (var template in selectedIncidents)
            {
                var incidentDate = DateTime.UtcNow.AddDays(-random.Next(7, 365)); // 1 week to 1 year ago
                var requiredHospitalization = template.Severity >= HealthIncidentSeverity.Serious && random.NextDouble() < 0.3;

                var healthIncident = HealthIncident.Create(
                    healthRecordId: healthRecord.Id,
                    type: template.Type,
                    severity: template.Severity,
                    symptoms: template.Symptoms,
                    treatmentProvided: template.Treatment,
                    treatmentLocation: template.Location,
                    incidentDateTime: incidentDate,
                    requiredHospitalization: requiredHospitalization,
                    treatedBy: GetRandomProvider(random)
                );

                // Add realistic follow-up based on severity
                if (template.Severity >= HealthIncidentSeverity.Moderate)
                {
                    healthIncident.NotifyParents();
                    
                    if (template.Severity >= HealthIncidentSeverity.Serious)
                    {
                        healthIncident.SetFollowUpRequired("Follow up with family doctor within 48 hours");
                        
                        if (requiredHospitalization)
                        {
                            healthIncident.SetReturnToSchoolDate(incidentDate.AddDays(3));
                        }
                    }
                }

                // Most incidents are resolved
                if (random.NextDouble() < 0.8)
                {
                    healthIncident.Resolve("Incident fully resolved, no ongoing concerns");
                }

                healthIncidents.Add(healthIncident);
            }
        }

        await _context.HealthIncidents.AddRangeAsync(healthIncidents);
        _logger.LogInformation("Seeded {Count} health incidents", healthIncidents.Count);
    }

    // Helper methods
    private PersonType DeterminePersonType(string email)
    {
        if (email.Contains("@harmoni360.com"))
            return PersonType.Staff;
        else if (email.Contains("@bsj.sch.id"))
            return PersonType.Student;
        else
            return PersonType.Visitor;
    }

    private BloodType GetRandomBloodType(Random random)
    {
        var bloodTypes = Enum.GetValues<BloodType>();
        return bloodTypes[random.Next(bloodTypes.Length)];
    }

    private string? GenerateMedicalNotes(PersonType personType, Random random)
    {
        if (random.NextDouble() < 0.7) // 70% have some notes
        {
            var notes = new[]
            {
                "No known allergies or medical conditions",
                "Regular health checkups completed",
                "Immunizations up to date",
                "Minor health concerns noted during screening",
                "Physical activity restrictions noted"
            };
            return notes[random.Next(notes.Length)];
        }
        return null;
    }

    private string GenerateTreatmentPlan(string conditionName)
    {
        return conditionName switch
        {
            "Peanut Allergy" => "Strict avoidance of peanuts, carry EpiPen at all times",
            "Asthma" => "Daily controller medication, rescue inhaler available",
            "Type 1 Diabetes" => "Regular blood glucose monitoring, insulin as prescribed",
            "Epilepsy" => "Daily anticonvulsant medication, avoid known triggers",
            _ => "Follow physician recommendations for ongoing care"
        };
    }

    private ContactRelationship GetRandomRelationship(Random random, bool isPrimary)
    {
        if (isPrimary)
        {
            var primaryRelationships = new[] { ContactRelationship.Parent, ContactRelationship.Guardian, ContactRelationship.Spouse };
            return primaryRelationships[random.Next(primaryRelationships.Length)];
        }
        
        var relationships = Enum.GetValues<ContactRelationship>();
        return relationships[random.Next(relationships.Length)];
    }

    private string GenerateContactName(ContactRelationship relationship, Random random)
    {
        var firstNames = new[] { "Michael", "Sarah", "David", "Jennifer", "Robert", "Lisa", "James", "Maria", "William", "Jessica" };
        var lastNames = new[] { "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez" };
        
        return $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
    }

    private string GeneratePhoneNumber(Random random)
    {
        return $"+62-21-{random.Next(1000, 9999)}{random.Next(100, 999)}";
    }

    private string GenerateEmail(Random random)
    {
        var providers = new[] { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com" };
        var names = new[] { "contact", "emergency", "family", "parent", "guardian" };
        return $"{names[random.Next(names.Length)]}{random.Next(100, 999)}@{providers[random.Next(providers.Length)]}";
    }

    private string GenerateAddress(Random random)
    {
        var streets = new[] { "Jalan Kemang", "Jalan Sudirman", "Jalan Thamrin", "Jalan Senayan", "Jalan Menteng" };
        var numbers = random.Next(1, 200);
        return $"{streets[random.Next(streets.Length)]} No. {numbers}, Jakarta Selatan";
    }

    private string GetRandomProvider(Random random)
    {
        var providers = new[] { "Dr. Ahmad Susanto", "Dr. Siti Rahayu", "Nurse Patricia", "Dr. Budi Santoso", "Dr. Maya Sari" };
        return providers[random.Next(providers.Length)];
    }

    private string GetRandomLocation(Random random)
    {
        var locations = new[] { "School Health Office", "Jakarta Medical Center", "Kemang Medical Clinic", "BSJ Health Center", "Family Clinic" };
        return locations[random.Next(locations.Length)];
    }

    private string GenerateVaccineNotes(string vaccine, bool administered, Random random)
    {
        if (!administered)
            return "Vaccination pending";
            
        var notes = new[]
        {
            "No adverse reactions observed",
            "Mild soreness at injection site",
            "Standard vaccination protocol followed",
            "Completed as part of routine health program"
        };
        return notes[random.Next(notes.Length)];
    }
}