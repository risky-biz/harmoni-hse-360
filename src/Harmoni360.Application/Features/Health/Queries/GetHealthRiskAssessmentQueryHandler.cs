using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public class GetHealthRiskAssessmentQueryHandler : IRequestHandler<GetHealthRiskAssessmentQuery, HealthRiskAssessmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetHealthRiskAssessmentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthRiskAssessmentDto> Handle(GetHealthRiskAssessmentQuery request, CancellationToken cancellationToken)
    {
        var healthRecords = await _context.HealthRecords
            .Include(hr => hr.MedicalConditions)
            .Include(hr => hr.Vaccinations)
            .Include(hr => hr.HealthIncidents)
            .Include(hr => hr.EmergencyContacts)
            .Where(hr => hr.IsActive)
            .ToListAsync(cancellationToken);

        var totalRecords = healthRecords.Count;
        var criticalConditions = healthRecords.SelectMany(hr => hr.MedicalConditions)
            .Count(mc => mc.RequiresEmergencyAction);
        var overdueVaccinations = healthRecords.SelectMany(hr => hr.Vaccinations)
            .Count(v => v.ExpiryDate.HasValue && v.ExpiryDate.Value < DateTime.UtcNow);
        var missingContacts = healthRecords.Count(hr => !hr.EmergencyContacts.Any());

        var riskLevel = criticalConditions > 10 || overdueVaccinations > 20 || missingContacts > 50 ? "High" :
                       criticalConditions > 5 || overdueVaccinations > 10 || missingContacts > 20 ? "Medium" : "Low";

        return new HealthRiskAssessmentDto
        {
            TotalPopulation = totalRecords,
            HighRiskIndividuals = criticalConditions,
            MediumRiskIndividuals = Math.Max(0, totalRecords / 10),
            LowRiskIndividuals = totalRecords - criticalConditions,
            OverallRiskScore = CalculateRiskScore(criticalConditions, overdueVaccinations, missingContacts, totalRecords),
            
            RiskFactors = new List<RiskFactorBreakdown> 
            { 
                new RiskFactorBreakdown
                {
                    RiskFactor = "Critical Medical Conditions",
                    Category = "Medical",
                    AffectedCount = criticalConditions,
                    RiskWeight = 0.5m,
                    SeverityLevel = "High",
                    AffectedDepartments = new List<string>()
                },
                new RiskFactorBreakdown
                {
                    RiskFactor = "Overdue Vaccinations",
                    Category = "Medical",
                    AffectedCount = overdueVaccinations,
                    RiskWeight = 0.3m,
                    SeverityLevel = "Medium",
                    AffectedDepartments = new List<string>()
                },
                new RiskFactorBreakdown
                {
                    RiskFactor = "Missing Emergency Contacts",
                    Category = "Environmental",
                    AffectedCount = missingContacts,
                    RiskWeight = 0.2m,
                    SeverityLevel = "Medium",
                    AffectedDepartments = new List<string>()
                }
            },
            
            StudentMetrics = new PopulationHealthMetrics
            {
                PopulationType = "Student",
                TotalCount = healthRecords.Count(hr => hr.PersonType == Domain.Entities.PersonType.Student),
                AverageRiskScore = 50,
                CriticalConditionsCount = criticalConditions / 2,
                ChronicConditionsCount = 0,
                HealthIncidentsLastMonth = 0,
                VaccinationComplianceRate = 85,
                EmergencyContactCompleteness = totalRecords - missingContacts
            },
            
            StaffMetrics = new PopulationHealthMetrics
            {
                PopulationType = "Staff",
                TotalCount = healthRecords.Count(hr => hr.PersonType == Domain.Entities.PersonType.Staff),
                AverageRiskScore = 30,
                CriticalConditionsCount = criticalConditions / 2,
                ChronicConditionsCount = 0,
                HealthIncidentsLastMonth = 0,
                VaccinationComplianceRate = 90,
                EmergencyContactCompleteness = totalRecords - missingContacts
            },
            
            IncidentCorrelations = new List<HealthIncidentCorrelation>(),
            HighRiskIndividualDetails = new List<HighRiskIndividualDto>(),
            Recommendations = new List<HealthRecommendationDto>
            {
                new HealthRecommendationDto
                {
                    Type = "Immediate",
                    Category = "Emergency Preparedness",
                    Title = "Address Missing Emergency Contacts",
                    Description = "Ensure all health records have emergency contacts",
                    Priority = "High",
                    AffectedAreas = new List<string> { "All Departments" },
                    EstimatedImpact = missingContacts,
                    RecommendedBy = DateTime.UtcNow
                }
            },
            
            AssessmentDate = DateTime.UtcNow,
            FromDate = DateTime.UtcNow.AddYears(-1),
            ToDate = DateTime.UtcNow
        };
    }

    private decimal CalculateRiskScore(int critical, int overdue, int missing, int total)
    {
        if (total == 0) return 0;
        
        var criticalWeight = 0.5m;
        var overdueWeight = 0.3m;
        var missingWeight = 0.2m;
        
        var score = (critical * criticalWeight + overdue * overdueWeight + missing * missingWeight) / total * 100;
        return Math.Min(100, Math.Max(0, score));
    }
}