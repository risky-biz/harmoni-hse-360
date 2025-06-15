using Microsoft.EntityFrameworkCore;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;
using Harmoni360.Infrastructure.Persistence;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class TrainingDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly Random _random = new();
    private readonly DateTime _baseDate = DateTime.UtcNow;

    // Sample users
    private readonly List<(int Id, string Name, string Department, string Position)> _sampleUsers = new()
    {
        (1, "John Smith", "Operations", "HSE Manager"),
        (2, "Sarah Johnson", "Human Resources", "Training Coordinator"),
        (3, "Michael Chen", "Safety Department", "Safety Officer"),
        (4, "Emma Wilson", "Engineering", "Senior Engineer"),
        (5, "Ahmad Rahman", "Quality Control", "QC Inspector"),
        (6, "Lisa Anderson", "Production", "Production Supervisor"),
        (7, "David Brown", "Maintenance", "Maintenance Lead"),
        (8, "Siti Nurhaliza", "Administration", "Admin Officer"),
        (9, "Robert Taylor", "Logistics", "Logistics Coordinator"),
        (10, "Maya Putri", "Environmental", "Environmental Officer")
    };

    public TrainingDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if trainings already exist
        if (await _context.Trainings.AnyAsync())
        {
            return;
        }

        var trainings = new List<Training>();

        // 1. K3 Mandatory Safety Training (Indonesian compliance)
        var k3Training = CreateK3SafetyTraining();
        trainings.Add(k3Training);

        // 2. Fire Safety and Emergency Response Training
        var fireTraining = CreateFireSafetyTraining();
        trainings.Add(fireTraining);

        // 3. Work at Height Training (Critical for construction/industrial)
        var heightTraining = CreateWorkAtHeightTraining();
        trainings.Add(heightTraining);

        // 4. Confined Space Entry Training
        var confinedSpaceTraining = CreateConfinedSpaceTraining();
        trainings.Add(confinedSpaceTraining);

        // 5. Chemical Handling and MSDS Training
        var chemicalTraining = CreateChemicalHandlingTraining();
        trainings.Add(chemicalTraining);

        // 6. Environmental Management System Training
        var environmentalTraining = CreateEnvironmentalTraining();
        trainings.Add(environmentalTraining);

        // 7. First Aid and Medical Emergency Training
        var firstAidTraining = CreateFirstAidTraining();
        trainings.Add(firstAidTraining);

        // 8. ISO 45001:2018 Internal Auditor Training
        var isoTraining = CreateISOAuditorTraining();
        trainings.Add(isoTraining);

        // 9. Electrical Safety Training
        var electricalTraining = CreateElectricalSafetyTraining();
        trainings.Add(electricalTraining);

        // 10. Leadership in Safety Excellence
        var leadershipTraining = CreateLeadershipTraining();
        trainings.Add(leadershipTraining);

        // Add all trainings to context
        await _context.Trainings.AddRangeAsync(trainings);
        await _context.SaveChangesAsync();

        // Add participants to trainings
        await AddParticipantsToTrainings(trainings);

        // Add requirements to trainings
        await AddRequirementsToTrainings(trainings);

        // Add comments to some trainings
        await AddCommentsToTrainings(trainings);

        // Simulate some trainings being completed with certifications
        await SimulateCompletedTrainings(trainings);
    }

    private Training CreateK3SafetyTraining()
    {
        var training = Training.Create(
            title: "K3 Umum (General K3) Certification Training",
            description: "Comprehensive 120-hour K3 General certification training program mandated by Indonesian Ministry of Manpower. Covers all aspects of workplace safety, health, and environmental management according to Indonesian regulations.",
            type: TrainingType.K3Training,
            category: TrainingCategory.MandatoryCompliance,
            deliveryMethod: TrainingDeliveryMethod.InPerson,
            scheduledStartDate: _baseDate.AddDays(10),
            scheduledEndDate: _baseDate.AddDays(14),
            venue: "HSE Training Center - Main Building",
            maxParticipants: 30,
            minParticipants: 10,
            instructorName: "Ir. Budi Santoso, M.K3",
            costPerParticipant: 5000000, // IDR 5 million
            geoLocation: GeoLocation.Create(-6.2088, 106.8456)
        );

        training.SetInstructorDetails(
            "Ir. Budi Santoso, M.K3",
            "Certified AK3 Umum Instructor, 15+ years experience in industrial safety",
            "+62-812-3456-7890",
            true
        );

        training.SetCertificationDetails(
            true,
            CertificationType.K3Certificate,
            ValidityPeriod.ThreeYears,
            "Kementerian Ketenagakerjaan RI"
        );

        training.SetIndonesianCompliance(
            true,
            "Permenaker No. 02/MEN/1992",
            true,
            "KEP.87/MEN/2024",
            "SKKNI No. 248 Tahun 2020"
        );

        training.GetType().GetProperty("VenueAddress")?.SetValue(training, "Jl. HR Rasuna Said Kav. C-22, Jakarta Selatan 12940");
        training.GetType().GetProperty("LearningObjectives")?.SetValue(training, "1. Understand Indonesian K3 regulations and compliance requirements\n2. Implement workplace safety management systems\n3. Conduct risk assessments and hazard identification\n4. Lead safety committees and programs\n5. Investigate workplace incidents");
        training.GetType().GetProperty("CourseOutline")?.SetValue(training, "Day 1-2: K3 Regulations and Legal Framework\nDay 3-4: Hazard Identification and Risk Assessment\nDay 5-6: Safety Management Systems\nDay 7-8: Incident Investigation and Reporting\nDay 9-10: Practical Applications and Examination");
        training.GetType().GetProperty("Prerequisites")?.SetValue(training, "Minimum D3/S1 degree, 2 years work experience, medical fitness certificate");

        return training;
    }

    private Training CreateFireSafetyTraining()
    {
        var training = Training.Create(
            title: "Advanced Fire Safety and Emergency Response Training",
            description: "Comprehensive fire safety training covering fire prevention, emergency response procedures, evacuation planning, and hands-on fire suppression techniques.",
            type: TrainingType.FireSafety,
            category: TrainingCategory.SafetyTraining,
            deliveryMethod: TrainingDeliveryMethod.Hybrid,
            scheduledStartDate: _baseDate.AddDays(15),
            scheduledEndDate: _baseDate.AddDays(16),
            venue: "Fire Training Ground - Safety Complex",
            maxParticipants: 20,
            minParticipants: 8,
            instructorName: "Captain Ahmad Firdaus",
            costPerParticipant: 1500000,
            geoLocation: GeoLocation.Create(-6.2297, 106.8295)
        );

        training.SetCertificationDetails(
            true,
            CertificationType.Competency,
            ValidityPeriod.TwoYears,
            "Indonesian Fire Safety Association"
        );

        training.GetType().GetProperty("OnlineLink")?.SetValue(training, "https://training.harmoni360.com/fire-safety-2024");
        training.GetType().GetProperty("OnlinePlatform")?.SetValue(training, "Harmoni360 LMS");

        return training;
    }

    private Training CreateWorkAtHeightTraining()
    {
        var training = Training.Create(
            title: "Working at Height - Advanced Safety Training",
            description: "Intensive training on safe work practices at height, including fall protection systems, equipment inspection, and rescue procedures.",
            type: TrainingType.SafetyOrientation,
            category: TrainingCategory.SpecializedTraining,
            deliveryMethod: TrainingDeliveryMethod.InPerson,
            scheduledStartDate: _baseDate.AddDays(20),
            scheduledEndDate: _baseDate.AddDays(21),
            venue: "Practical Training Facility - Tower Area",
            maxParticipants: 12,
            minParticipants: 6,
            instructorName: "Mark Thompson",
            costPerParticipant: 2000000
        );

        training.SetCertificationDetails(
            true,
            CertificationType.Professional,
            ValidityPeriod.ThreeYears,
            "International Work at Height Association"
        );

        training.GetType().GetProperty("Prerequisites")?.SetValue(training, "Medical fitness certificate, basic safety training completed");

        return training;
    }

    private Training CreateConfinedSpaceTraining()
    {
        var training = Training.Create(
            title: "Confined Space Entry and Rescue Operations",
            description: "Specialized training for safe entry, work, and rescue operations in confined spaces. Includes atmospheric monitoring, ventilation, and emergency procedures.",
            type: TrainingType.ConfinedSpaceEntry,
            category: TrainingCategory.SpecializedTraining,
            deliveryMethod: TrainingDeliveryMethod.InPerson,
            scheduledStartDate: _baseDate.AddDays(25),
            scheduledEndDate: _baseDate.AddDays(27),
            venue: "Industrial Training Center - Confined Space Simulator",
            maxParticipants: 10,
            minParticipants: 4,
            instructorName: "James Rodriguez",
            costPerParticipant: 3000000
        );

        training.SetAssessmentDetails(AssessmentMethod.Practical, 85.0m);
        training.GetType().GetProperty("Priority")?.SetValue(training, TrainingPriority.Critical);

        return training;
    }

    private Training CreateChemicalHandlingTraining()
    {
        var training = Training.Create(
            title: "Chemical Safety and Hazardous Materials Management",
            description: "Comprehensive training on safe handling, storage, and disposal of hazardous chemicals. Includes MSDS interpretation and emergency response.",
            type: TrainingType.ChemicalHandling,
            category: TrainingCategory.SafetyTraining,
            deliveryMethod: TrainingDeliveryMethod.InPerson,
            scheduledStartDate: _baseDate.AddDays(30),
            scheduledEndDate: _baseDate.AddDays(31),
            venue: "Chemical Safety Laboratory",
            maxParticipants: 15,
            minParticipants: 5,
            instructorName: "Dr. Ratna Wijayanti",
            costPerParticipant: 1800000
        );

        training.SetIndonesianCompliance(
            true,
            "Permenaker No. 187/MEN/1999",
            true,
            "",
            ""
        );

        return training;
    }

    private Training CreateEnvironmentalTraining()
    {
        var training = Training.Create(
            title: "Environmental Management System ISO 14001:2015",
            description: "Training on environmental management system implementation, environmental impact assessment, and regulatory compliance.",
            type: TrainingType.EnvironmentalAwareness,
            category: TrainingCategory.ProfessionalDevelopment,
            deliveryMethod: TrainingDeliveryMethod.Online,
            scheduledStartDate: _baseDate.AddDays(35),
            scheduledEndDate: _baseDate.AddDays(37),
            venue: "Online Platform",
            maxParticipants: 50,
            minParticipants: 10,
            instructorName: "Ir. Dewi Kartika, M.Env",
            costPerParticipant: 1200000
        );

        training.GetType().GetProperty("OnlineLink")?.SetValue(training, "https://training.harmoni360.com/iso14001-2024");
        training.GetType().GetProperty("OnlinePlatform")?.SetValue(training, "Zoom Professional");

        return training;
    }

    private Training CreateFirstAidTraining()
    {
        var training = Training.Create(
            title: "Emergency First Aid and CPR Certification",
            description: "Basic life support training including CPR, AED usage, wound care, and emergency response procedures.",
            type: TrainingType.EmergencyResponse,
            category: TrainingCategory.SafetyTraining,
            deliveryMethod: TrainingDeliveryMethod.InPerson,
            scheduledStartDate: _baseDate.AddDays(5),
            scheduledEndDate: _baseDate.AddDays(5),
            venue: "Medical Training Room",
            maxParticipants: 16,
            minParticipants: 8,
            instructorName: "Dr. Anita Susanto",
            costPerParticipant: 800000
        );

        training.SetCertificationDetails(
            true,
            CertificationType.Competency,
            ValidityPeriod.TwoYears,
            "Indonesian Red Cross"
        );

        // Mark this as already started
        training.GetType().GetProperty("Status")?.SetValue(training, TrainingStatus.InProgress);
        training.GetType().GetProperty("ActualStartDate")?.SetValue(training, _baseDate.AddDays(5));

        return training;
    }

    private Training CreateISOAuditorTraining()
    {
        var training = Training.Create(
            title: "ISO 45001:2018 Internal Auditor Training",
            description: "Comprehensive training on conducting internal audits for Occupational Health and Safety Management Systems according to ISO 45001:2018 standards.",
            type: TrainingType.Compliance,
            category: TrainingCategory.ProfessionalDevelopment,
            deliveryMethod: TrainingDeliveryMethod.InPerson,
            scheduledStartDate: _baseDate.AddDays(40),
            scheduledEndDate: _baseDate.AddDays(42),
            venue: "Corporate Training Center",
            maxParticipants: 20,
            minParticipants: 8,
            instructorName: "William Anderson, CQI IRCA",
            costPerParticipant: 4500000
        );

        training.SetCertificationDetails(
            true,
            CertificationType.ISO_Certificate,
            ValidityPeriod.ThreeYears,
            "CQI and IRCA Certified"
        );

        return training;
    }

    private Training CreateElectricalSafetyTraining()
    {
        var training = Training.Create(
            title: "Electrical Safety and Lockout/Tagout Procedures",
            description: "Essential training on electrical hazards, safe work practices, and proper lockout/tagout procedures for electrical equipment.",
            type: TrainingType.ElectricalSafety,
            category: TrainingCategory.SafetyTraining,
            deliveryMethod: TrainingDeliveryMethod.InPerson,
            scheduledStartDate: _baseDate.AddDays(-10),
            scheduledEndDate: _baseDate.AddDays(-9),
            venue: "Electrical Workshop",
            maxParticipants: 15,
            minParticipants: 5,
            instructorName: "Eng. Sugiarto",
            costPerParticipant: 1600000
        );

        // This training will be marked as completed later in SimulateCompletedTrainings
        // after participants are enrolled

        return training;
    }

    private Training CreateLeadershipTraining()
    {
        var training = Training.Create(
            title: "Leadership in Safety Excellence",
            description: "Advanced leadership training focusing on building a strong safety culture, effective communication, and leading by example in HSE practices.",
            type: TrainingType.LeadershipDevelopment,
            category: TrainingCategory.LeadershipTraining,
            deliveryMethod: TrainingDeliveryMethod.Workshop,
            scheduledStartDate: _baseDate.AddDays(50),
            scheduledEndDate: _baseDate.AddDays(52),
            venue: "Executive Training Suite",
            maxParticipants: 25,
            minParticipants: 10,
            instructorName: "Prof. Dr. Hendro Martono",
            costPerParticipant: 3500000
        );

        training.GetType().GetProperty("Prerequisites")?.SetValue(training, "Minimum 3 years in supervisory role, basic HSE training completed");

        return training;
    }

    private async Task AddParticipantsToTrainings(List<Training> trainings)
    {
        // Add participants to various trainings (skip completed or cancelled trainings)
        foreach (var training in trainings)
        {
            // Skip trainings that are completed or cancelled - cannot enroll new participants
            if (training.Status == TrainingStatus.Completed || training.Status == TrainingStatus.Cancelled)
                continue;

            var participantCount = _random.Next(training.MinParticipants, Math.Min(training.MaxParticipants, 15));
            var selectedUsers = _sampleUsers.OrderBy(x => _random.Next()).Take(participantCount).ToList();

            foreach (var user in selectedUsers)
            {
                training.EnrollParticipant(user.Id, user.Name, user.Department, user.Position, "System Seeder");
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task AddRequirementsToTrainings(List<Training> trainings)
    {
        // Add requirements to specific trainings
        var k3Training = trainings.First(t => t.Type == TrainingType.K3Training);
        k3Training.AddRequirement("Medical fitness certificate from authorized clinic", true, _baseDate.AddDays(5));
        k3Training.AddRequirement("Minimum D3/S1 educational qualification", true);
        k3Training.AddRequirement("2 years work experience in relevant field", true);
        k3Training.AddRequirement("Passport photo 3x4 (6 copies)", true, _baseDate.AddDays(8));

        var heightTraining = trainings.First(t => t.Title.Contains("Working at Height"));
        heightTraining.AddRequirement("Medical certificate confirming no fear of heights", true, _baseDate.AddDays(15));
        heightTraining.AddRequirement("Basic PPE training completed", true);
        heightTraining.AddRequirement("Physical fitness assessment passed", true);

        var confinedTraining = trainings.First(t => t.Type == TrainingType.ConfinedSpaceEntry);
        confinedTraining.AddRequirement("Medical examination including lung function test", true, _baseDate.AddDays(20));
        confinedTraining.AddRequirement("Basic safety orientation completed", true);
        confinedTraining.AddRequirement("Claustrophobia assessment passed", true);

        await _context.SaveChangesAsync();
    }

    private async Task AddCommentsToTrainings(List<Training> trainings)
    {
        var k3Training = trainings.First(t => t.Type == TrainingType.K3Training);
        k3Training.AddComment("Registration is now open. Please ensure all prerequisites are met before enrolling.", TrainingCommentType.AdminNote, "HR Department", 2);
        k3Training.AddComment("Can we arrange transport from the main office to the training venue?", TrainingCommentType.General, "Michael Chen", 3);
        k3Training.AddComment("Transport will be arranged. Departure at 7:30 AM from main lobby.", TrainingCommentType.AdminNote, "Training Coordinator", 2);

        var firstAidTraining = trainings.First(t => t.Type == TrainingType.EmergencyResponse);
        firstAidTraining.AddComment("Morning session completed. Participants showed excellent engagement in CPR practice.", TrainingCommentType.InstructorNote, "Dr. Anita Susanto", null);
        firstAidTraining.AddComment("Afternoon session will focus on wound care and emergency scenarios.", TrainingCommentType.InstructorNote, "Dr. Anita Susanto", null);

        await _context.SaveChangesAsync();
    }

    private async Task SimulateCompletedTrainings(List<Training> trainings)
    {
        var electricalTraining = trainings.First(t => t.Type == TrainingType.ElectricalSafety);
        
        // First mark the training as completed with proper dates
        electricalTraining.GetType().GetProperty("Status")?.SetValue(electricalTraining, TrainingStatus.Completed);
        electricalTraining.GetType().GetProperty("ActualStartDate")?.SetValue(electricalTraining, _baseDate.AddDays(-10));
        electricalTraining.GetType().GetProperty("ActualEndDate")?.SetValue(electricalTraining, _baseDate.AddDays(-9));
        
        // Mark participants as completed with scores
        foreach (var participant in electricalTraining.Participants)
        {
            participant.GetType().GetProperty("AttendanceMarked")?.SetValue(participant, true);
            participant.GetType().GetProperty("AttendanceDate")?.SetValue(participant, _baseDate.AddDays(-10));
            participant.GetType().GetProperty("FinalScore")?.SetValue(participant, (decimal)(_random.Next(75, 98) + _random.NextDouble()));
            participant.GetType().GetProperty("Passed")?.SetValue(participant, true);
            participant.GetType().GetProperty("CompletedAt")?.SetValue(participant, _baseDate.AddDays(-9));
            participant.GetType().GetProperty("Status")?.SetValue(participant, ParticipantStatus.Completed);
            participant.GetType().GetProperty("Feedback")?.SetValue(participant, "Excellent training program. Very practical and relevant to daily work.");
            participant.GetType().GetProperty("Rating")?.SetValue(participant, (decimal)(_random.Next(4, 5) + _random.NextDouble()));
        }

        electricalTraining.GetType().GetProperty("EvaluationSummary")?.SetValue(electricalTraining, 
            "Training completed successfully with all participants passing the assessment. High engagement and practical understanding demonstrated.");
        electricalTraining.GetType().GetProperty("AverageRating")?.SetValue(electricalTraining, 4.6m);
        electricalTraining.GetType().GetProperty("TotalRatings")?.SetValue(electricalTraining, electricalTraining.Participants.Count);

        await _context.SaveChangesAsync();
    }
}