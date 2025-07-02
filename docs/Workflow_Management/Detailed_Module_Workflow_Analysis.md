# Detailed Module Workflow Analysis for Harmoni360 HSE

## Executive Summary

This document provides comprehensive workflow analysis for all 20 modules in the Harmoni360 HSE system. Each module analysis includes current states, transitions, business rules, approval workflows, and integration requirements for the Visual Workflow Management System implementation.

## Table of Contents

1. [Analysis Methodology](#1-analysis-methodology)
2. [Module Workflow Details](#2-module-workflow-details)
3. [Cross-Module Dependencies](#3-cross-module-dependencies)
4. [Workflow Pattern Summary](#4-workflow-pattern-summary)
5. [Integration Requirements](#5-integration-requirements)

---

## 1. Analysis Methodology

### 1.1 Analysis Framework

Each module analysis follows this structure:
- **Current Status Enums**: Documented status values and meanings
- **Workflow States**: Mapped status to workflow states
- **State Transitions**: Valid transitions and trigger conditions
- **Business Rules**: Validation rules and constraints
- **Approval Requirements**: Multi-level approval configurations
- **Notifications**: Event-driven notification patterns
- **Integration Points**: Cross-module dependencies
- **Complexity Assessment**: Implementation complexity rating

### 1.2 Data Sources

- **Codebase Analysis**: Entity definitions, status enums, command handlers
- **HSE Workflow Diagrams**: Reference documentation from docs/HSE_Workflow/
- **Business Logic**: CQRS command handlers and domain events
- **Permission System**: Role-based access patterns

---

## 2. Module Workflow Details

### 2.1 Incident Management
**Complexity: High | Priority: Critical**

#### Current Status Enum
```csharp
public enum IncidentStatus
{
    Open = 1,           // Initial report received
    Reported = 2,       // Formally reported and assigned number
    InProgress = 3,     // Under investigation or action
    UnderInvestigation = 4,  // Formal investigation started
    AwaitingAction = 5, // Investigation complete, awaiting corrective action
    Resolved = 6,       // Actions completed, awaiting verification
    Closed = 7          // Incident fully closed and archived
}
```

#### Workflow States Mapping
```json
{
  "states": [
    {
      "name": "open",
      "displayName": "Open",
      "type": "initial",
      "color": "#FF6B6B",
      "description": "Initial incident report received",
      "timeout": 60,
      "timeoutAction": "escalate_to_manager"
    },
    {
      "name": "reported",
      "displayName": "Reported", 
      "type": "intermediate",
      "color": "#FFA500",
      "description": "Incident formally reported with assigned number"
    },
    {
      "name": "under_investigation",
      "displayName": "Under Investigation",
      "type": "intermediate", 
      "color": "#9B59B6",
      "description": "Formal investigation in progress"
    },
    {
      "name": "awaiting_action",
      "displayName": "Awaiting Action",
      "type": "intermediate",
      "color": "#3498DB", 
      "description": "Investigation complete, corrective actions required"
    },
    {
      "name": "resolved",
      "displayName": "Resolved",
      "type": "intermediate",
      "color": "#2ECC71",
      "description": "Actions completed, awaiting verification"
    },
    {
      "name": "closed",
      "displayName": "Closed",
      "type": "final",
      "color": "#95A5A6",
      "description": "Incident fully closed and archived"
    }
  ]
}
```

#### State Transitions
```json
{
  "transitions": [
    {
      "name": "assign_number",
      "from": "open",
      "to": "reported",
      "conditions": [
        {"type": "permission", "value": "IncidentManagement.Update"},
        {"type": "data", "field": "description", "operator": "not_empty"}
      ],
      "actions": [
        {"type": "generate_incident_number"},
        {"type": "notify", "recipients": ["role:IncidentManager"]},
        {"type": "audit_log", "message": "Incident number assigned"}
      ]
    },
    {
      "name": "start_investigation", 
      "from": "reported",
      "to": "under_investigation",
      "conditions": [
        {"type": "permission", "value": "IncidentManagement.Approve"},
        {"type": "data", "field": "investigator_id", "operator": "not_null"},
        {"type": "severity_check", "min_level": "medium"}
      ],
      "actions": [
        {"type": "assign_investigator"},
        {"type": "notify", "template": "investigation_started"},
        {"type": "create_investigation_plan"}
      ]
    },
    {
      "name": "complete_investigation",
      "from": "under_investigation", 
      "to": "awaiting_action",
      "conditions": [
        {"type": "permission", "value": "IncidentManagement.Update"},
        {"type": "data", "field": "investigation_report", "operator": "not_empty"},
        {"type": "business_rule", "rule": "all_findings_documented"}
      ],
      "actions": [
        {"type": "generate_corrective_actions"},
        {"type": "notify", "recipients": ["data:responsible_manager"]},
        {"type": "schedule_action_deadlines"}
      ]
    },
    {
      "name": "complete_actions",
      "from": "awaiting_action",
      "to": "resolved", 
      "conditions": [
        {"type": "business_rule", "rule": "all_actions_completed"},
        {"type": "data", "field": "verification_required", "operator": "==", "value": true}
      ],
      "actions": [
        {"type": "request_verification"},
        {"type": "notify", "recipients": ["role:SafetyManager"]}
      ]
    },
    {
      "name": "close_incident",
      "from": "resolved",
      "to": "closed",
      "conditions": [
        {"type": "permission", "value": "IncidentManagement.Approve"},
        {"type": "business_rule", "rule": "verification_complete"},
        {"type": "approval_required", "approvers": ["role:IncidentManager"]}
      ],
      "actions": [
        {"type": "archive_incident"},
        {"type": "update_statistics"},
        {"type": "notify", "template": "incident_closed"}
      ]
    }
  ]
}
```

#### Business Rules
- High and critical severity incidents require immediate escalation
- Investigation must be completed within 30 days for major incidents
- All corrective actions must be assigned with due dates
- Verification required for incidents involving injuries or property damage
- Cannot close incident with pending corrective actions

#### Integration Points
- **Risk Management**: Can spawn risk assessments from incidents
- **Training Management**: May require additional training based on findings
- **Work Permit Management**: May require permit reviews for similar work
- **Reporting**: Feeds into regulatory and management reports

---

### 2.2 Work Permit Management
**Complexity: Very High | Priority: Critical**

#### Current Status Enum
```csharp
public enum WorkPermitStatus
{
    Draft = 1,          // Being prepared
    PendingApproval = 2, // Submitted for approval
    Approved = 3,       // All approvals received
    Rejected = 4,       // Approval rejected
    InProgress = 5,     // Work in progress
    Completed = 6,      // Work completed
    Cancelled = 7,      // Permit cancelled
    Expired = 8         // Permit expired
}
```

#### Workflow States Mapping
```json
{
  "states": [
    {
      "name": "draft",
      "displayName": "Draft",
      "type": "initial", 
      "color": "#BDC3C7",
      "description": "Permit being prepared"
    },
    {
      "name": "pending_approval",
      "displayName": "Pending Approval",
      "type": "intermediate",
      "color": "#F39C12",
      "description": "Submitted for multi-level approval",
      "timeout": 1440,
      "timeoutAction": "escalate_approval"
    },
    {
      "name": "approved", 
      "displayName": "Approved",
      "type": "intermediate",
      "color": "#27AE60",
      "description": "All required approvals received"
    },
    {
      "name": "rejected",
      "displayName": "Rejected", 
      "type": "terminal",
      "color": "#E74C3C",
      "description": "Approval rejected with reasons"
    },
    {
      "name": "in_progress",
      "displayName": "Work In Progress",
      "type": "intermediate",
      "color": "#3498DB",
      "description": "Authorized work in progress"
    },
    {
      "name": "completed",
      "displayName": "Completed",
      "type": "final",
      "color": "#2ECC71", 
      "description": "Work completed successfully"
    },
    {
      "name": "cancelled",
      "displayName": "Cancelled",
      "type": "terminal",
      "color": "#95A5A6",
      "description": "Permit cancelled before completion"
    },
    {
      "name": "expired",
      "displayName": "Expired", 
      "type": "terminal",
      "color": "#E67E22",
      "description": "Permit expired without work completion"
    }
  ]
}
```

#### Multi-Level Approval Configuration
```json
{
  "approval_workflows": {
    "hot_work": {
      "levels": [
        {
          "level": 1,
          "role": "SafetyOfficer",
          "required": true,
          "conditions": ["fire_watch_assigned", "extinguisher_available"]
        },
        {
          "level": 2, 
          "role": "DepartmentHead",
          "required": true,
          "conditions": ["area_isolated", "emergency_plan_reviewed"]
        },
        {
          "level": 3,
          "role": "HotWorkSpecialist", 
          "required": true,
          "conditions": ["gas_test_complete", "weather_suitable"]
        }
      ]
    },
    "confined_space": {
      "levels": [
        {
          "level": 1,
          "role": "SafetyOfficer", 
          "required": true,
          "conditions": ["atmospheric_test_passed", "rescue_team_standby"]
        },
        {
          "level": 2,
          "role": "ConfinedSpaceSpecialist",
          "required": true, 
          "conditions": ["entry_plan_approved", "communication_established"]
        }
      ]
    }
  }
}
```

#### State Transitions
```json
{
  "transitions": [
    {
      "name": "submit_for_approval",
      "from": "draft",
      "to": "pending_approval",
      "conditions": [
        {"type": "permission", "value": "WorkPermitManagement.Create"},
        {"type": "business_rule", "rule": "all_required_fields_complete"},
        {"type": "business_rule", "rule": "safety_requirements_met"}
      ],
      "actions": [
        {"type": "start_approval_workflow"},
        {"type": "notify_approvers"},
        {"type": "schedule_approval_deadlines"}
      ]
    },
    {
      "name": "approve_level",
      "from": "pending_approval", 
      "to": "pending_approval",
      "conditions": [
        {"type": "permission", "value": "WorkPermitManagement.Approve"},
        {"type": "approval_level_check"}
      ],
      "actions": [
        {"type": "record_approval"},
        {"type": "check_all_approvals_complete"},
        {"type": "notify_next_approver"}
      ]
    },
    {
      "name": "final_approval",
      "from": "pending_approval",
      "to": "approved", 
      "conditions": [
        {"type": "business_rule", "rule": "all_approvals_received"},
        {"type": "business_rule", "rule": "no_blocking_conditions"}
      ],
      "actions": [
        {"type": "activate_permit"},
        {"type": "notify_requestor"},
        {"type": "schedule_expiry_reminder"}
      ]
    },
    {
      "name": "start_work",
      "from": "approved",
      "to": "in_progress",
      "conditions": [
        {"type": "permission", "value": "WorkPermitManagement.Update"},
        {"type": "business_rule", "rule": "all_preconditions_met"},
        {"type": "time_check", "within_validity_period": true}
      ],
      "actions": [
        {"type": "log_work_start"},
        {"type": "activate_monitoring"},
        {"type": "notify_safety_team"}
      ]
    }
  ]
}
```

#### Business Rules
- Hot work permits require fire watch and gas testing
- Confined space permits require atmospheric testing and rescue team
- Permits expire after specified duration (typically 24-48 hours)
- Weather conditions must be suitable for outdoor work
- All safety equipment must be verified before work starts
- Daily safety briefings required for multi-day permits

---

### 2.3 Risk Management (Hazards)
**Complexity: Medium | Priority: High**

#### Current Status Enum
```csharp
public enum HazardStatus
{
    Reported = 1,         // Initial hazard identification
    UnderAssessment = 2,  // Risk assessment in progress  
    ActionRequired = 3,   // Assessment complete, mitigation needed
    Mitigating = 4,       // Mitigation actions in progress
    Monitoring = 5,       // Controls implemented, under monitoring
    Resolved = 6,         // Hazard eliminated or adequately controlled
    Closed = 7            // Formally closed and archived
}
```

#### Workflow States Mapping
```json
{
  "states": [
    {
      "name": "reported",
      "displayName": "Reported",
      "type": "initial",
      "color": "#E74C3C", 
      "description": "Hazard identified and reported"
    },
    {
      "name": "under_assessment", 
      "displayName": "Under Assessment",
      "type": "intermediate",
      "color": "#F39C12",
      "description": "Risk assessment in progress",
      "timeout": 2880,
      "timeoutAction": "escalate_assessment"
    },
    {
      "name": "action_required",
      "displayName": "Action Required", 
      "type": "intermediate",
      "color": "#E67E22",
      "description": "Assessment complete, mitigation actions required"
    },
    {
      "name": "mitigating",
      "displayName": "Mitigating",
      "type": "intermediate",
      "color": "#3498DB",
      "description": "Mitigation actions in progress"
    },
    {
      "name": "monitoring",
      "displayName": "Monitoring", 
      "type": "intermediate",
      "color": "#9B59B6",
      "description": "Controls implemented, effectiveness being monitored"
    },
    {
      "name": "resolved",
      "displayName": "Resolved",
      "type": "intermediate",
      "color": "#2ECC71",
      "description": "Hazard adequately controlled"
    },
    {
      "name": "closed",
      "displayName": "Closed",
      "type": "final", 
      "color": "#95A5A6",
      "description": "Hazard formally closed"
    }
  ]
}
```

#### Risk Assessment Integration
```json
{
  "risk_assessment_workflow": {
    "probability_levels": ["Very Low", "Low", "Medium", "High", "Very High"],
    "consequence_levels": ["Negligible", "Minor", "Moderate", "Major", "Catastrophic"],
    "risk_matrix": {
      "low": {"color": "#2ECC71", "action": "monitor"},
      "medium": {"color": "#F39C12", "action": "mitigate"},
      "high": {"color": "#E67E22", "action": "immediate_action"},
      "critical": {"color": "#E74C3C", "action": "stop_work"}
    },
    "approval_requirements": {
      "high_risk": ["role:RiskManager", "role:SafetyManager"],
      "critical_risk": ["role:RiskManager", "role:SafetyManager", "role:DepartmentHead"]
    }
  }
}
```

---

### 2.4 Training Management
**Complexity: Medium | Priority: Medium**

#### Current Status Enum
```csharp
public enum TrainingStatus
{
    Draft = 1,
    Scheduled = 2,
    InProgress = 3, 
    Completed = 4,
    Cancelled = 5,
    Postponed = 6,
    UnderReview = 7,
    Approved = 8,
    Rejected = 9
}

public enum ParticipantStatus
{
    Enrolled = 1,
    Attending = 2,
    Completed = 3,
    Failed = 4,
    NoShow = 5,
    Withdrawn = 6,
    Pending = 7,
    WaitingList = 8
}
```

#### Workflow States Mapping
```json
{
  "training_workflow": {
    "states": [
      {
        "name": "draft",
        "displayName": "Draft",
        "type": "initial",
        "color": "#BDC3C7"
      },
      {
        "name": "scheduled", 
        "displayName": "Scheduled",
        "type": "intermediate",
        "color": "#3498DB"
      },
      {
        "name": "in_progress",
        "displayName": "In Progress",
        "type": "intermediate", 
        "color": "#F39C12"
      },
      {
        "name": "completed",
        "displayName": "Completed",
        "type": "final",
        "color": "#2ECC71"
      }
    ]
  },
  "participant_workflow": {
    "states": [
      {
        "name": "enrolled",
        "displayName": "Enrolled", 
        "type": "initial",
        "color": "#3498DB"
      },
      {
        "name": "attending",
        "displayName": "Attending",
        "type": "intermediate",
        "color": "#F39C12"
      },
      {
        "name": "completed",
        "displayName": "Completed",
        "type": "final",
        "color": "#2ECC71"
      },
      {
        "name": "failed",
        "displayName": "Failed", 
        "type": "terminal",
        "color": "#E74C3C"
      }
    ]
  }
}
```

---

### 2.5 Audit Management
**Complexity: Medium | Priority: Medium**

#### Current Status Enum
```csharp
public enum AuditStatus
{
    Draft = 1,
    Scheduled = 2, 
    InProgress = 3,
    Completed = 4,
    Overdue = 5,
    Cancelled = 6,
    Archived = 7,
    UnderReview = 8
}

public enum FindingStatus  
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Verified = 4,
    Closed = 5
}
```

---

### 2.6 Inspection Management
**Complexity: Medium | Priority: Medium**

#### Current Status Enum
```csharp
public enum InspectionStatus
{
    Draft = 1,
    Scheduled = 2,
    InProgress = 3, 
    Completed = 4,
    Overdue = 5,
    Cancelled = 6,
    Archived = 7
}

public enum InspectionItemStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    NonCompliant = 4,
    NotApplicable = 5
}
```

---

### 2.7 License Management
**Complexity: Medium | Priority: Medium**

#### Current Status Enum
```csharp
public enum LicenseStatus
{
    Draft = 1,
    PendingSubmission = 2,
    Submitted = 3,
    UnderReview = 4,
    Approved = 5,
    Active = 6,
    Rejected = 7,
    Expired = 8,
    Suspended = 9,
    Revoked = 10,
    PendingRenewal = 11
}
```

---

### 2.8 Waste Management
**Complexity: Medium | Priority: Low**

#### Current Status Enum
```csharp
public enum WasteReportStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    InStorage = 5,
    AwaitingPickup = 6,
    InTransit = 7,
    Disposed = 8,
    Rejected = 9,
    Cancelled = 10
}
```

---

### 2.9 PPE Management
**Complexity: Low | Priority: Low**

#### Current Status Enum
```csharp
public enum RequestStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5,
    Fulfilled = 6,
    Cancelled = 7
}

public enum PPEItemStatus
{
    Active = 1,
    Inactive = 2,
    Retired = 3
}
```

---

### 2.10 Health Monitoring
**Complexity: Low | Priority: Low**

#### Current Implementation
- Uses boolean flags (IsActive, IsApproved) rather than status enums
- Simple approval workflow for health records
- Medical surveillance tracking
- Vaccination compliance monitoring

---

## 3. Cross-Module Dependencies

### 3.1 Primary Dependencies

#### Incident → Risk Management
- Incidents can spawn new hazard assessments
- Risk levels influence incident escalation procedures
- Shared corrective action tracking

#### Risk Management → Work Permit Management  
- High-risk activities require work permits
- Risk assessments inform permit approval requirements
- Mitigation measures become permit preconditions

#### Training Management → All Modules
- Competency requirements for various roles
- Training completion affects authorization levels
- Incident findings may trigger additional training

### 3.2 Secondary Dependencies

#### Audit → Multiple Modules
- Audit findings can create incidents, risks, or training needs
- Compliance tracking across all modules
- Corrective action assignment and tracking

#### Inspection → Multiple Modules
- Equipment inspections affect PPE management
- Safety inspections influence risk assessments
- Inspection findings may require permits for remediation

---

## 4. Workflow Pattern Summary

### 4.1 Common Patterns Identified

#### Linear Progression Pattern (70% of modules)
**Pattern**: Draft → Submitted → Approved → Active → Completed/Closed
**Used by**: Training, Audit, Inspection, License, Waste Management

#### Branching Decision Pattern (60% of modules)  
**Pattern**: Submitted → (Approved | Rejected) → [Continue | Terminate]
**Used by**: Work Permits, PPE Requests, License Applications

#### Multi-Stage Approval Pattern (40% of modules)
**Pattern**: Submitted → Level1 Approval → Level2 Approval → ... → Final Approval
**Used by**: Work Permits, High-Risk Activities, Major Incidents

#### Monitoring/Renewal Pattern (30% of modules)
**Pattern**: Active → Under Review → (Renewed | Expired)
**Used by**: Licenses, Risk Assessments, Training Certifications

### 4.2 Approval Complexity Matrix

| Module | Approval Levels | Conditional Logic | Integration Points |
|--------|----------------|-------------------|-------------------|
| Work Permits | 1-5 levels | High | Very High |
| Incidents | 1-2 levels | Medium | High |
| Risk Management | 1-3 levels | High | High |
| Training | 1-2 levels | Low | Medium |
| Audits | 1-2 levels | Medium | Medium |
| Inspections | 1 level | Low | Medium |
| Licenses | 1-2 levels | Medium | Low |
| Waste Management | 1-2 levels | Low | Low |
| PPE | 1 level | Low | Low |

---

## 5. Integration Requirements

### 5.1 High Priority Integrations (Phase 1)

#### 1. Incident Management
- **Timeline**: Week 1-4 of Phase 4
- **Complexity**: Very High
- **Dependencies**: Risk Management, Notification System
- **Custom Requirements**: Multi-channel notifications, escalation paths

#### 2. Work Permit Management  
- **Timeline**: Week 5-10 of Phase 4
- **Complexity**: Very High
- **Dependencies**: Risk Management, Training Management
- **Custom Requirements**: Multi-level approvals, conditional workflows

#### 3. Risk Management
- **Timeline**: Week 11-14 of Phase 4  
- **Complexity**: High
- **Dependencies**: Incident Management, Work Permits
- **Custom Requirements**: Risk matrix calculations, approval routing

### 5.2 Medium Priority Integrations (Phase 2)

#### 4. Training Management
- **Timeline**: Phase 5, Week 1-4
- **Complexity**: Medium
- **Dependencies**: User Management, Certification tracking
- **Custom Requirements**: Participant tracking, competency management

#### 5. Audit Management
- **Timeline**: Phase 5, Week 5-8
- **Complexity**: Medium  
- **Dependencies**: Multiple modules for findings
- **Custom Requirements**: Finding workflows, corrective action tracking

### 5.3 Module-Specific Workflow Configurations

Each module will require:

1. **Default Workflow Definition**: Standard workflow for the module
2. **Client Customization Templates**: Common variations per client type
3. **Integration Mappings**: Cross-module workflow triggers
4. **Approval Hierarchies**: Role-based approval configurations
5. **Notification Templates**: Module-specific notification content
6. **Business Rule Definitions**: Validation and constraint logic
7. **Migration Scripts**: Legacy status to workflow state mapping

---

## Conclusion

This detailed analysis provides the foundation for implementing the Visual Workflow Management System across all 20 Harmoni360 HSE modules. The analysis reveals:

1. **High Variation in Complexity**: From simple linear workflows (PPE) to complex multi-stage approval processes (Work Permits)
2. **Strong Cross-Module Dependencies**: Especially between Incident, Risk, and Work Permit management
3. **Common Patterns**: 4 primary workflow patterns that can be templatized
4. **Clear Integration Priorities**: Critical path through Incident → Work Permit → Risk Management

The phased approach should prioritize high-complexity, high-dependency modules first, allowing the workflow engine to mature before tackling simpler modules. This analysis serves as the detailed specification for Phase 4 (Module Integration) of the implementation roadmap.