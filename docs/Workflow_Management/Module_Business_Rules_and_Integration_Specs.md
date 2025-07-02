# Module Business Rules and Integration Specifications

## Executive Summary

This document provides comprehensive business rules, validation logic, and integration specifications for implementing workflow management across all Harmoni360 HSE modules. It serves as the technical specification for Phase 4 (Module Integration) of the Visual Workflow Management System implementation.

## Table of Contents

1. [Business Rules Framework](#1-business-rules-framework)
2. [Module-Specific Business Rules](#2-module-specific-business-rules)
3. [Cross-Module Integration Specifications](#3-cross-module-integration-specifications)
4. [Validation Engine Requirements](#4-validation-engine-requirements)
5. [Implementation Guidelines](#5-implementation-guidelines)

---

## 1. Business Rules Framework

### 1.1 Rule Categories

#### Validation Rules
- **Data Completeness**: Required fields and information validation
- **Data Quality**: Format, range, and consistency checks
- **Business Logic**: Domain-specific validation requirements
- **Referential Integrity**: Cross-entity relationship validation

#### Authorization Rules
- **Permission-Based**: Role and permission requirements
- **Approval-Based**: Multi-level approval requirements
- **Competency-Based**: Skill and certification requirements
- **Context-Based**: Situational authorization logic

#### Process Rules
- **Sequence Rules**: Required order of operations
- **Timing Rules**: Deadlines, timeouts, and scheduling constraints
- **State Rules**: Valid state transitions and conditions
- **Integration Rules**: Cross-module workflow triggers

#### Compliance Rules
- **Regulatory Requirements**: Legal and regulatory compliance
- **Industry Standards**: HSE industry best practices
- **Client Requirements**: Client-specific compliance needs
- **Audit Requirements**: Documentation and trail requirements

### 1.2 Rule Engine Architecture

```csharp
public interface IBusinessRuleEngine
{
    Task<ValidationResult> ValidateAsync<T>(T entity, string ruleSet, WorkflowContext context);
    Task<bool> EvaluateConditionAsync(WorkflowCondition condition, WorkflowContext context);
    Task<IEnumerable<string>> GetApplicableRulesAsync(string entityType, string currentState);
    Task<RuleEvaluationResult> ExecuteRuleAsync(BusinessRule rule, WorkflowContext context);
}

public class WorkflowContext
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; }
    public string CurrentState { get; set; }
    public string TargetState { get; set; }
    public Guid UserId { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid? ClientId { get; set; }
}
```

---

## 2. Module-Specific Business Rules

### 2.1 Incident Management Business Rules

#### Data Validation Rules
```json
{
  "incident_validation_rules": {
    "required_fields": {
      "initial_report": ["description", "location", "reported_by", "occurred_at"],
      "investigation": ["investigator_id", "investigation_start_date"],
      "closure": ["root_cause", "corrective_actions_complete"]
    },
    "data_quality": {
      "description_min_length": 50,
      "location_format": "building.floor.room",
      "severity_levels": ["Low", "Medium", "High", "Critical"],
      "occurred_at_constraints": {
        "not_future": true,
        "not_older_than_days": 365
      }
    },
    "referential_integrity": {
      "investigator_must_exist": "Users.Id",
      "location_must_be_valid": "Locations.Code",
      "reported_by_must_exist": "Users.Id"
    }
  }
}
```

#### Process Rules
```csharp
public class IncidentBusinessRules : IModuleBusinessRules
{
    public async Task<bool> CanTransitionToInvestigationAsync(Incident incident, WorkflowContext context)
    {
        // Rule: High and Critical incidents require immediate investigation
        if (incident.Severity >= IncidentSeverity.High)
        {
            var timeSinceReported = DateTime.UtcNow - incident.ReportedAt;
            if (timeSinceReported > TimeSpan.FromHours(4))
            {
                throw new BusinessRuleException("High severity incidents must begin investigation within 4 hours");
            }
        }

        // Rule: Investigator must be certified for incident type
        var investigator = await _userService.GetByIdAsync(incident.InvestigatorId);
        var requiredCertifications = GetRequiredCertifications(incident.Type, incident.Severity);
        
        return await _certificationService.HasCertificationsAsync(investigator.Id, requiredCertifications);
    }

    public async Task<bool> CanCloseIncidentAsync(Incident incident, WorkflowContext context)
    {
        // Rule: All corrective actions must be completed
        var pendingActions = await _correctiveActionService.GetPendingAsync(incident.Id);
        if (pendingActions.Any())
        {
            throw new BusinessRuleException($"Cannot close incident with {pendingActions.Count()} pending corrective actions");
        }

        // Rule: Root cause analysis required for Medium+ incidents
        if (incident.Severity >= IncidentSeverity.Medium && string.IsNullOrEmpty(incident.RootCause))
        {
            throw new BusinessRuleException("Root cause analysis required for Medium and higher severity incidents");
        }

        // Rule: Management approval required for High+ incidents
        if (incident.Severity >= IncidentSeverity.High)
        {
            return await _permissionService.HasPermissionAsync(context.UserId, ModuleType.IncidentManagement, PermissionType.Approve);
        }

        return true;
    }
}
```

#### Escalation Rules
```json
{
  "escalation_rules": {
    "time_based": [
      {
        "condition": "severity == 'Critical' && state == 'open' && hours_elapsed > 1",
        "action": "escalate_to_safety_manager"
      },
      {
        "condition": "severity == 'High' && state == 'open' && hours_elapsed > 4", 
        "action": "escalate_to_incident_manager"
      },
      {
        "condition": "state == 'under_investigation' && days_elapsed > 30",
        "action": "escalate_to_department_head"
      }
    ],
    "event_based": [
      {
        "condition": "involves_fatality == true",
        "action": "immediate_escalation_to_ceo"
      },
      {
        "condition": "involves_regulatory_reporting == true",
        "action": "notify_compliance_team"
      }
    ]
  }
}
```

### 2.2 Work Permit Management Business Rules

#### Complex Approval Logic
```csharp
public class WorkPermitApprovalRules : IModuleBusinessRules
{
    public async Task<List<ApprovalLevel>> DetermineApprovalLevelsAsync(WorkPermit permit, WorkflowContext context)
    {
        var approvals = new List<ApprovalLevel>();

        // Base approval: Safety Officer (always required)
        approvals.Add(new ApprovalLevel
        {
            Level = 1,
            Role = "SafetyOfficer",
            Required = true,
            Conditions = new[] { "safety_plan_complete", "ppe_requirements_met" }
        });

        // Work type specific approvals
        if (permit.WorkType.HasFlag(WorkType.HotWork))
        {
            approvals.Add(new ApprovalLevel
            {
                Level = 2,
                Role = "HotWorkSpecialist", 
                Required = true,
                Conditions = new[] { "fire_watch_assigned", "extinguisher_available", "gas_test_complete" }
            });
        }

        if (permit.WorkType.HasFlag(WorkType.ConfinedSpace))
        {
            approvals.Add(new ApprovalLevel
            {
                Level = 2,
                Role = "ConfinedSpaceSpecialist",
                Required = true,
                Conditions = new[] { "atmospheric_test_passed", "rescue_team_standby", "communication_established" }
            });
        }

        // Risk-based approvals
        if (permit.RiskLevel >= RiskLevel.High)
        {
            approvals.Add(new ApprovalLevel
            {
                Level = 3,
                Role = "DepartmentHead",
                Required = true,
                Conditions = new[] { "area_isolated", "emergency_plan_reviewed" }
            });
        }

        // Value-based approvals
        if (permit.EstimatedValue > 50000)
        {
            approvals.Add(new ApprovalLevel
            {
                Level = 3,
                Role = "FinanceManager",
                Required = true,
                Conditions = new[] { "budget_approved", "procurement_verified" }
            });
        }

        // Sort by level and return
        return approvals.OrderBy(a => a.Level).ToList();
    }

    public async Task<bool> ValidateWorkStartConditionsAsync(WorkPermit permit, WorkflowContext context)
    {
        var validationResults = new List<bool>();

        // Weather conditions for outdoor work
        if (permit.IsOutdoorWork)
        {
            var weather = await _weatherService.GetCurrentConditionsAsync(permit.Location);
            validationResults.Add(weather.WindSpeed < 15); // mph
            validationResults.Add(weather.Visibility > 0.5); // miles
            validationResults.Add(!weather.SevereWeatherWarning);
        }

        // Equipment availability
        foreach (var equipment in permit.RequiredEquipment)
        {
            var isAvailable = await _equipmentService.IsAvailableAsync(equipment.Id, permit.PlannedStartDate);
            validationResults.Add(isAvailable);
        }

        // Personnel competency
        foreach (var worker in permit.AssignedWorkers)
        {
            var hasCompetency = await _competencyService.HasRequiredCompetencyAsync(worker.Id, permit.WorkType);
            validationResults.Add(hasCompetency);
        }

        // Area isolation
        if (permit.RequiresAreaIsolation)
        {
            var isIsolated = await _facilityService.IsAreaIsolatedAsync(permit.WorkArea);
            validationResults.Add(isIsolated);
        }

        return validationResults.All(result => result);
    }
}
```

#### Time-Based Constraints
```json
{
  "timing_constraints": {
    "permit_validity": {
      "hot_work": "24_hours",
      "electrical": "72_hours", 
      "excavation": "7_days",
      "confined_space": "8_hours"
    },
    "approval_deadlines": {
      "level_1_safety_officer": "4_hours",
      "level_2_specialist": "8_hours",
      "level_3_management": "24_hours"
    },
    "work_windows": {
      "hot_work": {
        "allowed_hours": "07:00-17:00",
        "weather_dependent": true,
        "wind_speed_limit": "15_mph"
      },
      "crane_operations": {
        "allowed_hours": "08:00-16:00",
        "wind_speed_limit": "20_mph",
        "visibility_minimum": "1_mile"
      }
    }
  }
}
```

### 2.3 Risk Management Business Rules

#### Risk Assessment Logic
```csharp
public class RiskAssessmentRules : IModuleBusinessRules
{
    public async Task<RiskLevel> CalculateRiskLevelAsync(RiskAssessment assessment, WorkflowContext context)
    {
        // Risk Matrix: Probability × Consequence
        var riskMatrix = new Dictionary<(Probability, Consequence), RiskLevel>
        {
            {(Probability.VeryLow, Consequence.Negligible), RiskLevel.Low},
            {(Probability.VeryLow, Consequence.Minor), RiskLevel.Low},
            {(Probability.Low, Consequence.Moderate), RiskLevel.Medium},
            {(Probability.Medium, Consequence.Major), RiskLevel.High},
            {(Probability.High, Consequence.Catastrophic), RiskLevel.Critical}
            // ... complete matrix mapping
        };

        var calculatedRisk = riskMatrix[(assessment.Probability, assessment.Consequence)];

        // Apply risk modifiers
        if (assessment.ExistingControls?.Any() == true)
        {
            calculatedRisk = ApplyControlEffectiveness(calculatedRisk, assessment.ExistingControls);
        }

        // Environmental factors
        if (assessment.WorkEnvironment == WorkEnvironment.Offshore)
        {
            calculatedRisk = EscalateRiskLevel(calculatedRisk);
        }

        return calculatedRisk;
    }

    public async Task<List<MitigationAction>> GenerateRequiredActionsAsync(RiskAssessment assessment, WorkflowContext context)
    {
        var actions = new List<MitigationAction>();

        switch (assessment.RiskLevel)
        {
            case RiskLevel.Critical:
                actions.Add(new MitigationAction
                {
                    Type = ActionType.StopWork,
                    Priority = Priority.Immediate,
                    DueDate = DateTime.UtcNow.AddHours(1),
                    Description = "Immediately stop all related work activities"
                });
                actions.Add(new MitigationAction
                {
                    Type = ActionType.Management Review,
                    Priority = Priority.Immediate,
                    DueDate = DateTime.UtcNow.AddHours(4),
                    Description = "Senior management review and action plan required"
                });
                break;

            case RiskLevel.High:
                actions.Add(new MitigationAction
                {
                    Type = ActionType.EngineeringControl,
                    Priority = Priority.High,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Description = "Implement engineering controls to reduce risk"
                });
                break;

            case RiskLevel.Medium:
                actions.Add(new MitigationAction
                {
                    Type = ActionType.AdministrativeControl,
                    Priority = Priority.Medium,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    Description = "Update procedures and provide additional training"
                });
                break;
        }

        return actions;
    }
}
```

### 2.4 Training Management Business Rules

#### Competency Validation
```csharp
public class TrainingCompetencyRules : IModuleBusinessRules
{
    public async Task<bool> ValidateParticipantEligibilityAsync(Training training, User participant, WorkflowContext context)
    {
        // Prerequisite training validation
        if (training.Prerequisites?.Any() == true)
        {
            foreach (var prerequisite in training.Prerequisites)
            {
                var hasPrerequisite = await _certificationService.HasValidCertificationAsync(
                    participant.Id, 
                    prerequisite.CertificationId,
                    training.StartDate);
                
                if (!hasPrerequisite)
                {
                    throw new BusinessRuleException($"Participant lacks prerequisite: {prerequisite.Name}");
                }
            }
        }

        // Role-based eligibility
        if (training.EligibleRoles?.Any() == true)
        {
            var participantRoles = await _userService.GetRolesAsync(participant.Id);
            var hasEligibleRole = training.EligibleRoles.Any(role => participantRoles.Contains(role));
            
            if (!hasEligibleRole)
            {
                throw new BusinessRuleException("Participant role not eligible for this training");
            }
        }

        // Medical clearance for high-risk training
        if (training.RequiresMedicalClearance)
        {
            var medicalClearance = await _healthService.GetLatestMedicalClearanceAsync(participant.Id);
            if (medicalClearance == null || medicalClearance.ExpiryDate < training.StartDate)
            {
                throw new BusinessRuleException("Valid medical clearance required");
            }
        }

        return true;
    }

    public async Task<CertificationResult> EvaluateCompetencyAsync(TrainingParticipant participant, WorkflowContext context)
    {
        var result = new CertificationResult();

        // Attendance requirement
        var attendanceRate = participant.AttendanceHours / participant.Training.TotalHours;
        if (attendanceRate < 0.8) // 80% minimum attendance
        {
            result.Passed = false;
            result.Reason = "Insufficient attendance";
            return result;
        }

        // Assessment score requirement
        if (participant.AssessmentScore < participant.Training.PassingScore)
        {
            result.Passed = false;
            result.Reason = "Assessment score below passing threshold";
            return result;
        }

        // Practical evaluation for hands-on training
        if (participant.Training.RequiresPracticalEvaluation)
        {
            if (!participant.PracticalEvaluationPassed)
            {
                result.Passed = false;
                result.Reason = "Practical evaluation not passed";
                return result;
            }
        }

        // Generate certification
        result.Passed = true;
        result.CertificationId = Guid.NewGuid();
        result.IssuedDate = DateTime.UtcNow;
        result.ExpiryDate = DateTime.UtcNow.AddMonths(participant.Training.CertificationValidityMonths);

        return result;
    }
}
```

---

## 3. Cross-Module Integration Specifications

### 3.1 Incident → Risk Assessment Integration

#### Trigger Conditions
```csharp
public class IncidentRiskIntegrationService : ICrossModuleIntegrationService
{
    public async Task ProcessIncidentClosureAsync(Incident incident, WorkflowContext context)
    {
        // Automatic risk assessment for certain incident types
        if (ShouldTriggerRiskAssessment(incident))
        {
            var riskAssessment = new RiskAssessment
            {
                TriggeredBy = $"Incident {incident.Number}",
                Location = incident.Location,
                Activity = incident.ActivityType,
                AssignedTo = incident.InvestigatorId,
                DueDate = DateTime.UtcNow.AddDays(GetRiskAssessmentTimeframe(incident.Severity)),
                Priority = MapIncidentSeverityToRiskPriority(incident.Severity)
            };

            await _workflowEngine.StartWorkflowAsync(
                "risk_assessment_workflow",
                riskAssessment.Id,
                new { TriggerSource = "IncidentClosure", IncidentId = incident.Id }
            );

            // Notify risk manager
            await _notificationService.SendAsync(new Notification
            {
                Template = "risk_assessment_required",
                Recipients = new[] { "role:RiskManager" },
                Data = new { Incident = incident, RiskAssessment = riskAssessment }
            });
        }
    }

    private bool ShouldTriggerRiskAssessment(Incident incident)
    {
        return incident.Severity >= IncidentSeverity.Medium ||
               incident.HasPotentialForRecurrence ||
               incident.InvolvesNewActivity ||
               incident.CorrectiveActions.Any(a => a.Type == ActionType.RiskReassessment);
    }
}
```

### 3.2 Risk Assessment → Work Permit Integration

#### Risk-Based Permit Requirements
```csharp
public class RiskPermitIntegrationService : ICrossModuleIntegrationService
{
    public async Task<WorkPermitRequirements> DeterminePermitRequirementsAsync(RiskAssessment riskAssessment, WorkflowContext context)
    {
        var requirements = new WorkPermitRequirements();

        // High risk activities automatically require work permits
        if (riskAssessment.RiskLevel >= RiskLevel.High)
        {
            requirements.WorkPermitRequired = true;
            requirements.SpecialConditions.Add("High risk activity - enhanced safety measures required");
        }

        // Specific hazards trigger specific permit types
        foreach (var hazard in riskAssessment.IdentifiedHazards)
        {
            switch (hazard.Type)
            {
                case HazardType.Fire:
                    requirements.RequiredPermitTypes.Add(PermitType.HotWork);
                    requirements.SpecialConditions.Add("Fire watch required");
                    break;
                
                case HazardType.ConfinedSpace:
                    requirements.RequiredPermitTypes.Add(PermitType.ConfinedSpace);
                    requirements.SpecialConditions.Add("Atmospheric testing and rescue team required");
                    break;
                
                case HazardType.Height:
                    requirements.RequiredPermitTypes.Add(PermitType.WorkAtHeight);
                    requirements.SpecialConditions.Add("Fall protection equipment required");
                    break;
            }
        }

        // Risk controls become permit preconditions
        foreach (var control in riskAssessment.RequiredControls)
        {
            requirements.Preconditions.Add(new PermitPrecondition
            {
                Description = control.Description,
                VerificationRequired = true,
                ResponsibleRole = control.ResponsibleRole
            });
        }

        return requirements;
    }
}
```

### 3.3 Training → Competency Integration

#### Competency-Based Authorization
```csharp
public class TrainingCompetencyIntegrationService : ICrossModuleIntegrationService
{
    public async Task<bool> ValidateCompetencyForWorkflowAsync(string workflowType, Guid userId, WorkflowContext context)
    {
        var requiredCompetencies = await GetRequiredCompetenciesAsync(workflowType);
        
        foreach (var competency in requiredCompetencies)
        {
            var certification = await _certificationService.GetLatestCertificationAsync(userId, competency.Id);
            
            if (certification == null || certification.IsExpired)
            {
                // Trigger training requirement
                await TriggerTrainingRequirementAsync(userId, competency, context);
                return false;
            }
        }

        return true;
    }

    private async Task TriggerTrainingRequirementAsync(Guid userId, Competency competency, WorkflowContext context)
    {
        var trainingRequirement = new TrainingRequirement
        {
            UserId = userId,
            CompetencyId = competency.Id,
            RequiredBy = DateTime.UtcNow.AddDays(competency.TrainingLeadTimeDays),
            TriggeredBy = $"Workflow: {context.EntityType}",
            Priority = GetTrainingPriority(competency.CriticalityLevel)
        };

        await _workflowEngine.StartWorkflowAsync(
            "training_requirement_workflow",
            trainingRequirement.Id,
            new { TriggerSource = "CompetencyValidation", OriginalWorkflow = context }
        );
    }
}
```

---

## 4. Validation Engine Requirements

### 4.1 Rule Execution Engine

```csharp
public class WorkflowValidationEngine : IWorkflowValidationEngine
{
    public async Task<ValidationResult> ValidateTransitionAsync(WorkflowTransition transition, WorkflowContext context)
    {
        var result = new ValidationResult();
        
        // Execute pre-conditions
        foreach (var condition in transition.Conditions)
        {
            var conditionResult = await EvaluateConditionAsync(condition, context);
            if (!conditionResult.IsValid)
            {
                result.Errors.Add(conditionResult.ErrorMessage);
            }
        }

        // Execute business rules
        var applicableRules = await GetApplicableRulesAsync(context.EntityType, transition.FromState, transition.ToState);
        foreach (var rule in applicableRules)
        {
            var ruleResult = await ExecuteBusinessRuleAsync(rule, context);
            if (!ruleResult.IsValid)
            {
                result.Errors.Add(ruleResult.ErrorMessage);
            }
        }

        // Execute approval validations
        if (transition.RequiresApproval)
        {
            var approvalResult = await ValidateApprovalRequirementsAsync(transition, context);
            if (!approvalResult.IsValid)
            {
                result.Errors.Add(approvalResult.ErrorMessage);
            }
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    private async Task<ConditionResult> EvaluateConditionAsync(WorkflowCondition condition, WorkflowContext context)
    {
        switch (condition.Type)
        {
            case ConditionType.Permission:
                return await EvaluatePermissionConditionAsync(condition, context);
            
            case ConditionType.Data:
                return await EvaluateDataConditionAsync(condition, context);
            
            case ConditionType.BusinessRule:
                return await EvaluateBusinessRuleConditionAsync(condition, context);
            
            case ConditionType.Time:
                return await EvaluateTimeConditionAsync(condition, context);
            
            default:
                throw new NotSupportedException($"Condition type {condition.Type} not supported");
        }
    }
}
```

### 4.2 Dynamic Rule Configuration

```json
{
  "rule_configuration": {
    "client_customizable_rules": [
      {
        "rule_id": "incident_investigation_timeframe",
        "description": "Maximum time allowed for incident investigation",
        "default_value": "30_days",
        "client_overrides": {
          "client_abc": "14_days",
          "client_def": "45_days"
        }
      },
      {
        "rule_id": "work_permit_approval_levels",
        "description": "Number of approval levels required for work permits",
        "default_value": 2,
        "risk_based_overrides": {
          "low_risk": 1,
          "medium_risk": 2,
          "high_risk": 3,
          "critical_risk": 4
        }
      }
    ],
    "regulatory_compliance_rules": [
      {
        "rule_id": "osha_incident_reporting",
        "description": "OSHA incident reporting requirements",
        "jurisdiction": "US",
        "mandatory": true,
        "cannot_override": true
      }
    ]
  }
}
```

---

## 5. Implementation Guidelines

### 5.1 Rule Implementation Priority

#### Phase 1: Core Business Rules (Weeks 1-2)
- Data validation rules
- Basic state transition rules
- Permission-based authorization rules

#### Phase 2: Process Rules (Weeks 3-4)
- Approval workflow rules
- Time-based constraints
- Integration trigger rules

#### Phase 3: Advanced Rules (Weeks 5-6)
- Complex business logic rules
- Client-specific customizations
- Regulatory compliance rules

### 5.2 Testing Strategy

#### Unit Testing
```csharp
[TestClass]
public class IncidentBusinessRulesTests
{
    [TestMethod]
    public async Task CanTransitionToInvestigation_HighSeverityWithinTimeframe_ReturnsTrue()
    {
        // Arrange
        var incident = CreateHighSeverityIncident(reportedAt: DateTime.UtcNow.AddHours(-2));
        var context = CreateWorkflowContext(incident);
        
        // Act
        var result = await _incidentRules.CanTransitionToInvestigationAsync(incident, context);
        
        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CanTransitionToInvestigation_HighSeverityExceedsTimeframe_ThrowsException()
    {
        // Arrange
        var incident = CreateHighSeverityIncident(reportedAt: DateTime.UtcNow.AddHours(-6));
        var context = CreateWorkflowContext(incident);
        
        // Act & Assert
        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _incidentRules.CanTransitionToInvestigationAsync(incident, context));
    }
}
```

#### Integration Testing
```csharp
[TestClass]
public class CrossModuleIntegrationTests
{
    [TestMethod]
    public async Task IncidentClosure_HighSeverity_TriggersRiskAssessment()
    {
        // Arrange
        var incident = CreateHighSeverityIncident();
        var context = CreateWorkflowContext(incident);
        
        // Act
        await _integrationService.ProcessIncidentClosureAsync(incident, context);
        
        // Assert
        var triggeredRiskAssessments = await _riskAssessmentRepository
            .GetByTriggerSourceAsync($"Incident {incident.Number}");
        
        Assert.AreEqual(1, triggeredRiskAssessments.Count());
    }
}
```

### 5.3 Performance Considerations

#### Rule Caching Strategy
```csharp
public class CachedBusinessRuleEngine : IBusinessRuleEngine
{
    private readonly IMemoryCache _ruleCache;
    
    public async Task<ValidationResult> ValidateAsync<T>(T entity, string ruleSet, WorkflowContext context)
    {
        var cacheKey = $"rules_{typeof(T).Name}_{ruleSet}_{context.ClientId}";
        
        var rules = await _ruleCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(15);
            return await _ruleRepository.GetApplicableRulesAsync(typeof(T).Name, ruleSet, context.ClientId);
        });
        
        return await ExecuteRulesAsync(entity, rules, context);
    }
}
```

#### Database Optimization
- Index rule lookup tables on entity type and state
- Cache frequently accessed rule definitions
- Optimize cross-module query performance
- Consider rule execution result caching for expensive validations

---

## Conclusion

This comprehensive business rules and integration specification provides the technical foundation for implementing sophisticated workflow management across all Harmoni360 HSE modules. The framework supports:

1. **Complex Business Logic**: Multi-layered validation and approval rules
2. **Cross-Module Integration**: Seamless workflow coordination between modules
3. **Client Customization**: Flexible rule configuration for client-specific requirements
4. **Regulatory Compliance**: Built-in compliance validation and audit trails
5. **Performance Optimization**: Efficient rule execution and caching strategies

The implementation approach ensures that business rules are maintainable, testable, and scalable while supporting the complex requirements of HSE workflow management.