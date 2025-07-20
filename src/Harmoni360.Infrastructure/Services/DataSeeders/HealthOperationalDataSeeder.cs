using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class HealthOperationalDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthOperationalDataSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _random = new();

    public HealthOperationalDataSeeder(ApplicationDbContext context, ILogger<HealthOperationalDataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting Health Management operational data seeding...");

            // Get required data
            var users = await _context.Users.ToListAsync();

            if (!users.Any())
            {
                _logger.LogWarning("Cannot seed Health operational data - missing users");
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
            if (!forceReseed && await _context.HealthRecords.AnyAsync())
            {
                _logger.LogInformation("Health operational data already exists, skipping seeding");
                return;
            }

            // Create operational data
            await SeedHealthRecordsAsync(users);
            await SeedHealthIncidentsAsync(users);
            await SeedVaccinationRecordsAsync(users);
            await SeedMedicalConditionsAsync(users);
            await SeedEmergencyContactsAsync(users);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Health Management operational data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding Health operational data");
            throw;
        }
    }

    private async Task ClearExistingDataAsync()
    {
        _logger.LogInformation("Clearing existing Health operational data...");
        
        _context.EmergencyContacts.RemoveRange(_context.EmergencyContacts);
        _context.MedicalConditions.RemoveRange(_context.MedicalConditions);
        _context.VaccinationRecords.RemoveRange(_context.VaccinationRecords);
        _context.HealthIncidents.RemoveRange(_context.HealthIncidents);
        _context.HealthRecords.RemoveRange(_context.HealthRecords);
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Existing Health operational data cleared");
    }

    private Task SeedHealthRecordsAsync(List<User> users)
    {
        _logger.LogInformation("Seeding health records...");

        var healthRecords = new List<HealthRecord>();

        foreach (var user in users)
        {
            var personType = GetPersonType(user);
            var bloodType = GetRandomBloodType();
            var dateOfBirth = GetRandomDateOfBirth(personType);
            var medicalNotes = GetRandomMedicalNotes();

            var record = HealthRecord.Create(
                personId: user.Id,
                personType: personType,
                dateOfBirth: dateOfBirth,
                bloodType: bloodType,
                medicalNotes: medicalNotes
            );

            healthRecords.Add(record);
        }

        _context.HealthRecords.AddRange(healthRecords);
        _logger.LogInformation($"Seeded {healthRecords.Count} health records");
        return Task.CompletedTask;
    }

    private async Task SeedHealthIncidentsAsync(List<User> users)
    {
        _logger.LogInformation("Seeding health incidents...");

        var healthRecords = await _context.HealthRecords.ToListAsync();
        var incidents = new List<HealthIncident>();
        var startDate = DateTime.UtcNow.AddYears(-2);

        if (!healthRecords.Any())
        {
            _logger.LogWarning("No health records found, skipping health incidents seeding");
            return;
        }

        // Create 50-80 health incidents over 2 years
        var incidentCount = _random.Next(50, Math.Min(81, healthRecords.Count * 3)); // Limit to available records
        for (int i = 0; i < incidentCount; i++)
        {
            var healthRecord = healthRecords[_random.Next(healthRecords.Count)];
            var incidentDate = startDate.AddDays(_random.Next(0, 730));
            var type = (HealthIncidentType)_random.Next(1, 9); // Injury through Other
            var severity = (HealthIncidentSeverity)_random.Next(1, 6); // Minor through LifeThreatening
            var treatmentLocation = (TreatmentLocation)_random.Next(1, 7); // SchoolNurse through Other

            var incident = HealthIncident.Create(
                healthRecordId: healthRecord.Id,
                type: type,
                severity: severity,
                symptoms: GetSymptoms(type),
                treatmentProvided: GetTreatmentProvided(type, severity),
                treatmentLocation: treatmentLocation,
                incidentDateTime: incidentDate,
                requiredHospitalization: severity >= HealthIncidentSeverity.Serious && _random.NextDouble() > 0.7,
                treatedBy: GetTreatedBy(treatmentLocation)
            );

            incidents.Add(incident);
        }

        _context.HealthIncidents.AddRange(incidents);
        _logger.LogInformation($"Seeded {incidents.Count} health incidents");
    }

    private async Task SeedVaccinationRecordsAsync(List<User> users)
    {
        _logger.LogInformation("Seeding vaccination records...");

        var healthRecords = await _context.HealthRecords.ToListAsync();
        var vaccinationRecords = new List<VaccinationRecord>();
        var vaccines = new[]
        {
            "Hepatitis B", "Tetanus", "Influenza", "COVID-19", "Hepatitis A",
            "Measles/Mumps/Rubella", "Varicella", "Pneumococcal", "Meningococcal"
        };

        if (!healthRecords.Any())
        {
            _logger.LogWarning("No health records found, skipping vaccination records seeding");
            return;
        }

        foreach (var healthRecord in healthRecords)
        {
            var vaccineCount = _random.Next(2, 6); // 2-5 vaccines per person
            var selectedVaccines = vaccines.OrderBy(x => _random.Next()).Take(vaccineCount);

            foreach (var vaccine in selectedVaccines)
            {
                var administrationDate = DateTime.UtcNow.AddDays(-_random.Next(30, 1095));
                var isRequired = _random.NextDouble() > 0.3; // 70% required vaccines
                var expiryDate = GetVaccineExpiryDate(vaccine, administrationDate);

                var record = VaccinationRecord.Create(
                    healthRecordId: healthRecord.Id,
                    vaccineName: vaccine,
                    isRequired: isRequired,
                    dateAdministered: administrationDate,
                    expiryDate: expiryDate,
                    batchNumber: GenerateBatchNumber(),
                    administeredBy: GetVaccineAdministrator(),
                    administrationLocation: GetAdministrationLocation(),
                    notes: GetVaccinationNotes(vaccine)
                );

                vaccinationRecords.Add(record);
            }
        }

        _context.VaccinationRecords.AddRange(vaccinationRecords);
        _logger.LogInformation($"Seeded {vaccinationRecords.Count} vaccination records");
    }

    private async Task SeedMedicalConditionsAsync(List<User> users)
    {
        _logger.LogInformation("Seeding medical conditions...");

        var healthRecords = await _context.HealthRecords.ToListAsync();
        var medicalConditions = new List<MedicalCondition>();

        if (!healthRecords.Any())
        {
            _logger.LogWarning("No health records found, skipping medical conditions seeding");
            return;
        }

        var recordsToProcess = healthRecords.Take(Math.Min(30, healthRecords.Count)); // Up to 30 people have medical conditions
        foreach (var healthRecord in recordsToProcess)
        {
            var conditionCount = _random.Next(1, 3); // 1-2 conditions per person

            for (int i = 0; i < conditionCount; i++)
            {
                var conditionType = (ConditionType)_random.Next(1, 8); // Allergy through Other
                var severity = (ConditionSeverity)_random.Next(1, 5); // Mild through LifeThreatening
                var conditionName = GetConditionName(conditionType);
                var description = GetConditionDescription(conditionType, conditionName);
                var requiresEmergencyAction = severity >= ConditionSeverity.Severe;

                var condition = MedicalCondition.Create(
                    healthRecordId: healthRecord.Id,
                    type: conditionType,
                    name: conditionName,
                    description: description,
                    severity: severity,
                    treatmentPlan: GetTreatmentPlan(conditionType, severity),
                    diagnosedDate: DateTime.UtcNow.AddDays(-_random.Next(30, 1095)),
                    requiresEmergencyAction: requiresEmergencyAction,
                    emergencyInstructions: requiresEmergencyAction ? GetEmergencyInstructions(conditionType) : null
                );

                medicalConditions.Add(condition);
            }
        }

        _context.MedicalConditions.AddRange(medicalConditions);
        _logger.LogInformation($"Seeded {medicalConditions.Count} medical conditions");
    }

    private async Task SeedEmergencyContactsAsync(List<User> users)
    {
        _logger.LogInformation("Seeding emergency contacts...");

        var healthRecords = await _context.HealthRecords.ToListAsync();
        var emergencyContacts = new List<EmergencyContact>();

        if (!healthRecords.Any())
        {
            _logger.LogWarning("No health records found, skipping emergency contacts seeding");
            return;
        }

        foreach (var healthRecord in healthRecords)
        {
            var contactCount = _random.Next(1, 3); // 1-2 emergency contacts per person

            for (int i = 0; i < contactCount; i++)
            {
                var relationship = (ContactRelationship)_random.Next(1, 9); // Parent through Other
                var isPrimary = i == 0; // First contact is primary

                var contact = EmergencyContact.Create(
                    healthRecordId: healthRecord.Id,
                    name: GenerateContactName(relationship),
                    relationship: relationship,
                    primaryPhone: GeneratePhoneNumber(),
                    email: GenerateEmailAddress(),
                    secondaryPhone: _random.NextDouble() > 0.5 ? GeneratePhoneNumber() : null,
                    address: GenerateAddress(),
                    isPrimaryContact: isPrimary,
                    authorizedForPickup: _random.NextDouble() > 0.3, // 70% authorized for pickup
                    authorizedForMedicalDecisions: relationship == ContactRelationship.Parent || relationship == ContactRelationship.Guardian,
                    customRelationship: relationship == ContactRelationship.Other ? "Family Friend" : null,
                    notes: GetContactNotes(relationship)
                );

                emergencyContacts.Add(contact);
            }
        }

        _context.EmergencyContacts.AddRange(emergencyContacts);
        _logger.LogInformation($"Seeded {emergencyContacts.Count} emergency contacts");
    }

    private PersonType GetPersonType(User user)
    {
        // Randomly assign person types based on user characteristics
        var types = Enum.GetValues<PersonType>();
        return types[_random.Next(types.Length)];
    }

    private BloodType? GetRandomBloodType()
    {
        if (_random.NextDouble() < 0.1) return null; // 10% don't have blood type recorded

        var bloodTypes = Enum.GetValues<BloodType>();
        return bloodTypes[_random.Next(bloodTypes.Length)];
    }

    private DateTime? GetRandomDateOfBirth(PersonType personType)
    {
        if (_random.NextDouble() < 0.2) return null; // 20% don't have DOB recorded

        return personType switch
        {
            PersonType.Student => DateTime.UtcNow.AddYears(-_random.Next(18, 25)),
            PersonType.Staff => DateTime.UtcNow.AddYears(-_random.Next(25, 65)),
            PersonType.Visitor => DateTime.UtcNow.AddYears(-_random.Next(20, 80)),
            PersonType.Contractor => DateTime.UtcNow.AddYears(-_random.Next(25, 60)),
            _ => DateTime.UtcNow.AddYears(-_random.Next(20, 70))
        };
    }

    private string? GetRandomMedicalNotes()
    {
        if (_random.NextDouble() < 0.6) return null; // 60% don't have medical notes

        var notes = new[]
        {
            "No known allergies or medical conditions",
            "Regular medical check-ups recommended",
            "Requires regular medication monitoring",
            "Contact primary care physician for any concerns",
            "Previous medical history documented in file",
            "Special dietary requirements noted"
        };
        return notes[_random.Next(notes.Length)];
    }

    private string GetSymptoms(HealthIncidentType type)
    {
        return type switch
        {
            HealthIncidentType.Injury => "Pain, swelling, limited mobility, bruising",
            HealthIncidentType.Illness => "Nausea, headache, dizziness, fatigue",
            HealthIncidentType.AllergicReaction => "Rash, itching, swelling, difficulty breathing",
            HealthIncidentType.MedicationIssue => "Adverse reaction, side effects, medication error",
            HealthIncidentType.MentalHealthEpisode => "Anxiety, panic, emotional distress",
            HealthIncidentType.ChronicConditionFlareUp => "Worsening of existing condition symptoms",
            HealthIncidentType.EmergencyResponse => "Chest pain, difficulty breathing, loss of consciousness",
            _ => "Various symptoms reported by individual"
        };
    }

    private string GetTreatmentProvided(HealthIncidentType type, HealthIncidentSeverity severity)
    {
        var treatments = type switch
        {
            HealthIncidentType.Injury => "First aid applied, wound care, pain management",
            HealthIncidentType.Illness => "Rest, hydration, symptom monitoring",
            HealthIncidentType.AllergicReaction => "Antihistamine administered, emergency protocol followed",
            HealthIncidentType.MedicationIssue => "Medication discontinued, alternative treatment provided",
            HealthIncidentType.MentalHealthEpisode => "Counseling support, calm environment provided",
            HealthIncidentType.ChronicConditionFlareUp => "Condition-specific treatment protocol followed",
            HealthIncidentType.EmergencyResponse => "Emergency medical services contacted, life support provided",
            _ => "Standard medical care provided per protocols"
        };

        return severity >= HealthIncidentSeverity.Serious 
            ? $"URGENT: {treatments}. Advanced medical intervention required."
            : treatments;
    }

    private string? GetTreatedBy(TreatmentLocation location)
    {
        return location switch
        {
            TreatmentLocation.SchoolNurse => "School Nurse",
            TreatmentLocation.Hospital => "Emergency Department Physician",
            TreatmentLocation.Clinic => "Medical Clinic Staff",
            TreatmentLocation.Homecare => "Family Member/Guardian",
            _ => "Medical Professional"
        };
    }

    private DateTime? GetVaccineExpiryDate(string vaccine, DateTime administrationDate)
    {
        return vaccine switch
        {
            "Influenza" => administrationDate.AddYears(1),
            "Tetanus" => administrationDate.AddYears(10),
            "COVID-19" => administrationDate.AddMonths(6),
            "Hepatitis B" => administrationDate.AddYears(20),
            "Measles/Mumps/Rubella" => null, // Lifetime immunity
            "Varicella" => null, // Lifetime immunity
            _ => administrationDate.AddYears(5)
        };
    }

    private string GenerateBatchNumber()
    {
        return $"VAC-{DateTime.UtcNow.Year}-{_random.Next(10000, 99999)}";
    }

    private string GetVaccineAdministrator()
    {
        var administrators = new[] { "Dr. Smith", "Nurse Johnson", "Dr. Williams", "Nurse Davis", "Dr. Brown" };
        return administrators[_random.Next(administrators.Length)];
    }

    private string GetAdministrationLocation()
    {
        var locations = new[] { "Health Clinic", "School Nurse Office", "Medical Center", "Hospital", "Community Health Center" };
        return locations[_random.Next(locations.Length)];
    }

    private string? GetVaccinationNotes(string vaccine)
    {
        if (_random.NextDouble() < 0.7) return null; // 70% don't have notes

        return vaccine switch
        {
            "COVID-19" => "Part of ongoing vaccination program",
            "Influenza" => "Annual flu vaccine administered",
            "Tetanus" => "Booster shot administered per schedule",
            _ => "Routine vaccination per health guidelines"
        };
    }

    private string GetConditionName(ConditionType type)
    {
        return type switch
        {
            ConditionType.Allergy => GetAllergyName(),
            ConditionType.ChronicCondition => GetChronicConditionName(),
            ConditionType.MedicationDependency => GetMedicationDependencyName(),
            ConditionType.PhysicalLimitation => GetPhysicalLimitationName(),
            ConditionType.MentalHealthCondition => GetMentalHealthConditionName(),
            ConditionType.Dietary => GetDietaryConditionName(),
            _ => "General Medical Condition"
        };
    }

    private string GetAllergyName()
    {
        var allergies = new[] { "Peanut Allergy", "Shellfish Allergy", "Latex Allergy", "Bee Sting Allergy", "Medication Allergy", "Food Dye Allergy" };
        return allergies[_random.Next(allergies.Length)];
    }

    private string GetChronicConditionName()
    {
        var conditions = new[] { "Asthma", "Diabetes Type 1", "Epilepsy", "Heart Condition", "Arthritis", "Hypertension" };
        return conditions[_random.Next(conditions.Length)];
    }

    private string GetMedicationDependencyName()
    {
        var dependencies = new[] { "Insulin Dependency", "Inhaler Dependency", "Anti-seizure Medication", "Heart Medication" };
        return dependencies[_random.Next(dependencies.Length)];
    }

    private string GetPhysicalLimitationName()
    {
        var limitations = new[] { "Mobility Limitation", "Hearing Impairment", "Vision Impairment", "Joint Limitation" };
        return limitations[_random.Next(limitations.Length)];
    }

    private string GetMentalHealthConditionName()
    {
        var conditions = new[] { "Anxiety Disorder", "ADHD", "Depression", "Autism Spectrum Disorder" };
        return conditions[_random.Next(conditions.Length)];
    }

    private string GetDietaryConditionName()
    {
        var conditions = new[] { "Gluten Intolerance", "Lactose Intolerance", "Vegan Diet", "Diabetic Diet" };
        return conditions[_random.Next(conditions.Length)];
    }

    private string GetConditionDescription(ConditionType type, string name)
    {
        return $"{name} - {type} requiring ongoing monitoring and appropriate management";
    }

    private string? GetTreatmentPlan(ConditionType type, ConditionSeverity severity)
    {
        if (_random.NextDouble() < 0.3) return null; // 30% don't have treatment plan

        return type switch
        {
            ConditionType.Allergy => "Avoid allergen exposure, carry emergency medication if prescribed",
            ConditionType.ChronicCondition => "Regular medical monitoring, medication compliance, lifestyle management",
            ConditionType.MedicationDependency => "Strict medication schedule, regular medical review",
            ConditionType.PhysicalLimitation => "Accommodation planning, adaptive equipment as needed",
            ConditionType.MentalHealthCondition => "Counseling support, medication if prescribed, stress management",
            ConditionType.Dietary => "Specialized diet plan, nutrition monitoring",
            _ => "Regular monitoring and appropriate medical care"
        };
    }

    private string GetEmergencyInstructions(ConditionType type)
    {
        return type switch
        {
            ConditionType.Allergy => "Administer emergency medication immediately, call 911, monitor breathing",
            ConditionType.ChronicCondition => "Call emergency services, provide comfort and support, contact parent/guardian",
            ConditionType.MedicationDependency => "Ensure medication access, call medical emergency if needed",
            _ => "Call emergency services immediately, provide basic first aid, contact emergency contacts"
        };
    }

    private string GenerateContactName(ContactRelationship relationship)
    {
        var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Lisa", "Robert", "Mary", "James", "Jennifer" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
        
        return $"{firstNames[_random.Next(firstNames.Length)]} {lastNames[_random.Next(lastNames.Length)]}";
    }

    private string GeneratePhoneNumber()
    {
        return $"+1-{_random.Next(200, 999)}-{_random.Next(200, 999)}-{_random.Next(1000, 9999)}";
    }

    private string GenerateEmailAddress()
    {
        var domains = new[] { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com" };
        var username = $"contact{_random.Next(1000, 9999)}";
        return $"{username}@{domains[_random.Next(domains.Length)]}";
    }

    private string GenerateAddress()
    {
        var streetNumbers = _random.Next(100, 9999);
        var streetNames = new[] { "Main St", "Oak Ave", "Pine Dr", "First St", "Second Ave", "Park Blvd" };
        var cities = new[] { "Springfield", "Franklin", "Georgetown", "Madison", "Kingston" };
        
        return $"{streetNumbers} {streetNames[_random.Next(streetNames.Length)]}, {cities[_random.Next(cities.Length)]}";
    }

    private string? GetContactNotes(ContactRelationship relationship)
    {
        if (_random.NextDouble() < 0.6) return null; // 60% don't have notes

        return relationship switch
        {
            ContactRelationship.Parent => "Primary contact for all medical decisions",
            ContactRelationship.Guardian => "Legal guardian with full medical authority",
            ContactRelationship.Spouse => "Emergency contact with medical decision authority",
            _ => "Emergency contact - contact if primary is unavailable"
        };
    }
}