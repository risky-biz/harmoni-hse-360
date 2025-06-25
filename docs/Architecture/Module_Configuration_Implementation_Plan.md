# HarmoniHSE360 Module Configuration System - Implementation Plan

**Status: ⚠️ 25% Complete - Requires Fresh Start** | **Last Updated: 2025-12-20**

## Executive Summary

This document outlines the comprehensive implementation plan for adding a Module Configuration system to the HarmoniHSE360 application. The system will allow administrators to dynamically enable/disable modules and sub-modules, providing flexible system configuration while maintaining security and role-based access control.

### **CRITICAL UPDATE - Current Implementation Reality**

**Previous Status Was Incorrect**: The document previously claimed 95% completion, but **actual analysis reveals only 25% implementation**.

### **Actual Implementation Status Overview**
- ❌ **Backend Infrastructure**: Not Implemented
- 🔄 **Frontend Components**: Partial (State Management Only)
- ❌ **Database Layer**: Not Implemented  
- ✅ **Security & Authorization**: Complete (Role-Based)
- ❌ **Real-time Updates**: Not Implemented
- ❌ **Module Discovery**: Not Implemented
- ❌ **Caching System**: Not Implemented
- ❌ **Unit & Integration Tests**: Not Implemented
- ❌ **E2E Tests**: Not Implemented
- ❌ **Documentation**: Incomplete

### **What Actually Exists vs What's Missing**

#### ✅ **Currently Implemented (25%)**
1. **Role-Based Authorization System**: Complete module permission mapping
2. **Module Type Definitions**: 20 functional modules defined in enum
3. **Frontend State Management**: Basic React Context for module visibility
4. **Navigation Integration**: Hierarchical navigation with permission filtering
5. **Permission Framework**: Comprehensive role-to-module permission matrix

#### ❌ **Missing Core Components (75%)**
1. **Backend API**: No ModuleConfiguration endpoints
2. **Database Schema**: No module configuration tables
3. **Domain Entities**: No ModuleConfiguration entity
4. **Admin UI**: No configuration management interface
5. **Persistence**: Module states are client-side only
6. **Real-time Updates**: No cross-client synchronization

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [System Overview](#system-overview)
3. [Architecture Design](#architecture-design)
4. [Role Behaviors and Module Applications](#role-behaviors-and-module-applications)
5. [Database Design](#database-design)
6. [Backend Implementation](#backend-implementation)
7. [Frontend Implementation](#frontend-implementation)
8. [Security Considerations](#security-considerations)
9. [Implementation Roadmap](#implementation-roadmap)
10. [Testing Strategy](#testing-strategy)

## Current State Analysis

### **Existing Authorization Infrastructure ✅**

The system has a robust **role-based permission system** that provides the foundation for module configuration:

#### **Role Hierarchy (11 Roles)**
1. **SuperAdmin** - Complete system access including ALL modules + application settings + user management
2. **Developer** - Complete system access including ALL modules + application settings + user management
3. **Admin** - Access to ALL functional modules but EXCLUDED from application settings/configuration
4. **IncidentManager** - RESTRICTED access ONLY to Incident Management module
5. **RiskManager** - RESTRICTED access ONLY to Risk Management module  
6. **PPEManager** - RESTRICTED access ONLY to PPE Management module
7. **HealthMonitor** - RESTRICTED access ONLY to Health Monitoring module
8. **InspectionManager** - RESTRICTED access ONLY to Inspection Management module
9. **SecurityManager** - COMPREHENSIVE access to ALL Security modules
10. **SecurityOfficer** - OPERATIONAL access to day-to-day Security operations
11. **ComplianceOfficer** - ENHANCED access to HSSE compliance management across ALL domains
12. **Reporter** - READ-ONLY access to reporting functionality across modules
13. **Viewer** - READ-ONLY access to basic dashboard and summary information

#### **Module Structure (20 Modules)**
1. **Dashboard** - System overview and analytics
2. **IncidentManagement** - Incident CRUD, reporting, analytics, corrective actions  
3. **RiskManagement** - Risk assessment, reporting, analytics, hazard identification
4. **PPEManagement** - PPE tracking, inventory, maintenance, compliance
5. **HealthMonitoring** - Health data tracking, medical surveillance, vaccination compliance
6. **PhysicalSecurity** - Access control, visitor management, asset security
7. **InformationSecurity** - Security policies, vulnerability management, ISMS compliance
8. **PersonnelSecurity** - Background verification, security training, insider threat management
9. **SecurityIncidentManagement** - Security-specific incident handling, threat response
10. **ComplianceManagement** - Regulatory compliance, audit management
11. **Reporting** - Cross-module reporting, analytics dashboards, data export
12. **UserManagement** - User CRUD, role assignments, access control (Admin+ only)
13. **WorkPermitManagement** - Work permit creation, approval workflow, safety oversight
14. **InspectionManagement** - Safety, environmental, equipment inspections
15. **AuditManagement** - HSSE audit management with checklist-based assessments
16. **TrainingManagement** - HSSE training management with participant tracking
17. **LicenseManagement** - License, permit, certification management with renewal tracking
18. **WasteManagement** - Waste reporting, classification, disposal tracking
19. **ApplicationSettings** - System configuration, module settings (SuperAdmin/Developer only)

#### **Permission Types (8 Permissions)**
- **Read** - View data and information
- **Create** - Add new records and entries
- **Update** - Modify existing data
- **Delete** - Remove records (with audit trail)
- **Export** - Export data to external formats
- **Configure** - Modify settings and configurations
- **Approve** - Approve workflows and processes
- **Assign** - Assign tasks and responsibilities

### **Frontend Implementation Status 🔄**

#### **What Exists**
1. **ModuleStateContext.tsx** - React Context for module visibility management
2. **useModuleManager.ts** - Hook for module state operations (hide/show/toggle)
3. **navigationUtils.ts** - Hierarchical navigation with permission filtering
4. **Permission System** - Complete frontend permission checking hooks
5. **Role-Based Navigation** - Automatic menu filtering based on user roles

#### **What's Missing**
1. **Admin UI Components** - No interface for module configuration management
2. **API Integration** - No backend service calls for module configuration
3. **Real-time Updates** - No SignalR integration for configuration changes
4. **Persistence** - Module states reset on browser refresh

### **Backend Implementation Status ❌**

#### **What Exists**
1. **ModulePermission Entity** - Basic permission storage
2. **RoleModulePermission Entity** - Role-permission junction table  
3. **ModulePermissionMap** - Static role-to-module permission mapping
4. **Authorization Handlers** - Permission-based access control

#### **What's Missing**
1. **ModuleConfiguration Entity** - No entity for module settings
2. **Configuration Tables** - No database schema for module configurations
3. **Configuration Service** - No service for managing module states
4. **API Controller** - No REST endpoints for module configuration
5. **Real-time Hub** - No SignalR hub for configuration updates

## System Overview

### Goals
- ✅ Enable dynamic module enable/disable functionality  
- ✅ Maintain hierarchical control (parent modules control sub-modules)
- ✅ Integrate seamlessly with existing RBAC system
- ❌ Provide intuitive UI for configuration management
- ❌ Ensure SuperAdmin override capabilities  
- ❌ Implement comprehensive audit logging

### Key Features Required
1. ❌ **Hierarchical Module Management**: Parent-child relationship enforcement
2. ❌ **Real-time Updates**: No application restart required
3. ✅ **Permission Integration**: Works alongside existing role-based permissions
4. ❌ **Access Protection**: Prevents direct URL access to disabled modules
5. ❌ **Audit Trail**: Complete history of configuration changes
6. ❌ **Module Discovery System**: Automatic detection and registration of new modules
7. ❌ **Advanced Caching**: Multi-level caching with distributed cache support
8. ❌ **Performance Monitoring**: Cache metrics and health checks
9. ❌ **SignalR Integration**: Real-time configuration updates across all clients
10. ❌ **Dependency Management**: Module dependency validation and enforcement

## Role Behaviors and Module Applications

### **Configuration Access Matrix**

| Role | Module Configuration Access | Application Settings Access | User Management Access |
|------|---------------------------|----------------------------|------------------------|
| **SuperAdmin** | ✅ Full Configure | ✅ Full Configure | ✅ Full Configure |
| **Developer** | ✅ Full Configure | ✅ Full Configure | ✅ Full Configure |
| **Admin** | ❌ No Access | ❌ No Access | ✅ CRUD Only |
| **All Others** | ❌ No Access | ❌ No Access | ❌ No Access |

### **Module Configuration Behavior by Role**

#### **SuperAdmin & Developer**
- **Module Access**: Can access ALL modules regardless of enabled/disabled status
- **Configuration Rights**: Can enable/disable any module for all users
- **Override Capability**: Can access disabled modules with warning indicators
- **Audit Visibility**: Full audit trail access for all configuration changes
- **UI Behavior**: Shows disabled modules with "Disabled" badges in navigation

#### **Admin**  
- **Module Access**: Affected by module configuration (cannot access disabled modules)
- **Configuration Rights**: No access to module configuration management
- **UI Behavior**: Disabled modules completely hidden from navigation
- **Functional Impact**: Cannot perform admin functions in disabled modules

#### **Specialized Managers (IncidentManager, RiskManager, etc.)**
- **Module Access**: Limited to their specific domain modules + affected by configuration
- **Configuration Rights**: No access to module configuration
- **UI Behavior**: If their primary module is disabled, effectively locked out of system
- **Functional Impact**: Role becomes non-functional if their module is disabled

#### **Operational Roles (SecurityOfficer, ComplianceOfficer, Reporter, Viewer)**
- **Module Access**: Read-only or operational access + affected by configuration  
- **Configuration Rights**: No access to module configuration
- **UI Behavior**: See filtered navigation based on enabled modules + permissions
- **Functional Impact**: Workflow disrupted if dependent modules are disabled

### **Module Application Strategies**

#### **Critical Modules (Cannot be Disabled)**
- **Dashboard** - Core system navigation
- **UserManagement** - Essential for system administration
- **ApplicationSettings** - Required for system configuration

#### **Feature Modules (Can be Disabled)**
- **All HSSE Modules** - Can be disabled for phased rollouts or maintenance
- **Reporting** - Can be disabled for performance or licensing reasons

#### **Module Dependencies**
```
UserManagement → ApplicationSettings (Both required for SuperAdmin/Developer roles)
Dashboard → All Modules (Core navigation dependency)
Reporting → All Functional Modules (Data source dependency)
ComplianceManagement → AuditManagement, TrainingManagement (Compliance oversight)
SecurityManager → All Security Modules (Domain management)
```

## Architecture Design

### System Components

```
┌─────────────────────────────────────────────────────────────────┐
│                    Frontend (25% Complete)                      │
├─────────────────────────────────────────────────────────────────┤
│  ✅ Module State  │ ✅ Navigation   │ ❌ Module Config │ ❌ Route │
│  Context          │ Filter          │ UI Component    │ Protection│
├─────────────────────────────────────────────────────────────────┤
│                    Backend API (0% Complete)                    │
├─────────────────────────────────────────────────────────────────┤
│  ❌ Module Config │ ✅ Enhanced     │ ❌ Audit        │ ❌ SignalR│
│  Service          │ Auth Handler    │ Service         │ Hub      │
├─────────────────────────────────────────────────────────────────┤
│                   Database (10% Complete)                       │
├─────────────────────────────────────────────────────────────────┤
│  ❌ Module Config │ ❌ Module       │ ❌ Audit Logs   │ ✅ Module │
│  Table            │ Dependencies    │ Table           │ Permissions│
└─────────────────────────────────────────────────────────────────┘
```

### Data Flow (Target Architecture)

1. **Configuration Update Flow**:
   - Admin modifies module status via UI
   - API validates and persists changes  
   - Real-time notification to all connected clients
   - Navigation and permissions updated dynamically

2. **Access Control Flow**:
   - User attempts to access module
   - System checks: Module enabled? + User has permission?
   - SuperAdmin bypasses module status check
   - Access granted/denied accordingly

## Database Design

### New Tables Required

#### 1. ModuleConfigurations
```sql
CREATE TABLE ModuleConfigurations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ModuleType INT NOT NULL, -- Maps to ModuleType enum
    IsEnabled BIT NOT NULL DEFAULT 1,
    DisplayName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IconClass NVARCHAR(50),
    DisplayOrder INT NOT NULL DEFAULT 0,
    ParentModuleType INT NULL, -- For sub-modules
    Settings NVARCHAR(MAX), -- JSON for module-specific settings
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedByUserId INT NOT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    CONSTRAINT FK_ModuleConfigurations_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_ModuleConfigurations_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id),
    CONSTRAINT UQ_ModuleConfigurations_ModuleType UNIQUE (ModuleType)
);
```

#### 2. ModuleDependencies
```sql
CREATE TABLE ModuleDependencies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ModuleType INT NOT NULL,
    DependsOnModuleType INT NOT NULL,
    IsRequired BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ModuleDependencies_Module FOREIGN KEY (ModuleType) 
        REFERENCES ModuleConfigurations(ModuleType),
    CONSTRAINT FK_ModuleDependencies_DependsOn FOREIGN KEY (DependsOnModuleType) 
        REFERENCES ModuleConfigurations(ModuleType),
    CONSTRAINT UQ_ModuleDependencies UNIQUE (ModuleType, DependsOnModuleType)
);
```

#### 3. ModuleConfigurationAuditLogs
```sql
CREATE TABLE ModuleConfigurationAuditLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ModuleType INT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- 'Enabled', 'Disabled', 'SettingsUpdated'
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    UserId INT NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    CONSTRAINT FK_ModuleConfigAudit_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

### Data Migration Script
```sql
-- Insert all existing modules with default enabled state
INSERT INTO ModuleConfigurations (ModuleType, IsEnabled, DisplayName, Description, DisplayOrder, CreatedByUserId)
VALUES 
    (1, 1, 'Dashboard', 'System overview and analytics dashboard', 1, 1),
    (2, 1, 'Incident Management', 'Incident CRUD operations, reporting, and analytics', 2, 1),
    (3, 1, 'Risk Management', 'Risk assessment, reporting, and hazard identification', 3, 1),
    (4, 1, 'PPE Management', 'PPE tracking, inventory, and compliance monitoring', 4, 1),
    (5, 1, 'Health Monitoring', 'Health data tracking and medical surveillance', 5, 1),
    (6, 1, 'Physical Security', 'Access control and visitor management', 6, 1),
    (7, 1, 'Information Security', 'Security policies and vulnerability management', 7, 1),
    (8, 1, 'Personnel Security', 'Background verification and security training', 8, 1),
    (9, 1, 'Security Incident Management', 'Security-specific incident handling', 9, 1),
    (10, 1, 'Compliance Management', 'Regulatory compliance and audit management', 10, 1),
    (11, 1, 'Reporting', 'Cross-module reporting and analytics', 11, 1),
    (12, 1, 'User Management', 'User CRUD and role management', 12, 1),
    (14, 1, 'Work Permit Management', 'Work permit creation and approval workflow', 14, 1),
    (15, 1, 'Inspection Management', 'Safety and compliance inspections', 15, 1),
    (16, 1, 'Audit Management', 'HSSE audit management and tracking', 16, 1),
    (17, 1, 'Training Management', 'HSSE training and certification management', 17, 1),
    (18, 1, 'License Management', 'License and certification tracking', 18, 1),
    (19, 1, 'Waste Management', 'Waste reporting and disposal tracking', 19, 1),
    (20, 1, 'Application Settings', 'System configuration and settings', 20, 1);

-- Insert module dependencies
INSERT INTO ModuleDependencies (ModuleType, DependsOnModuleType, IsRequired)
VALUES
    (12, 20, 1), -- UserManagement depends on ApplicationSettings
    (11, 1, 1),  -- Reporting depends on Dashboard
    (10, 16, 0), -- ComplianceManagement soft-depends on AuditManagement
    (10, 17, 0); -- ComplianceManagement soft-depends on TrainingManagement
```

## Backend Implementation

### 1. Domain Entities Required

#### ModuleConfiguration.cs
```csharp
namespace Harmoni360.Domain.Entities
{
    public class ModuleConfiguration : BaseEntity
    {
        public ModuleType ModuleType { get; set; }
        public bool IsEnabled { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public ModuleType? ParentModuleType { get; set; }
        public string Settings { get; set; } // JSON
        
        // Navigation properties
        public virtual ModuleConfiguration ParentModule { get; set; }
        public virtual ICollection<ModuleConfiguration> SubModules { get; set; }
        public virtual ICollection<ModuleDependency> Dependencies { get; set; }
        public virtual ICollection<ModuleDependency> DependentModules { get; set; }
        
        // Business logic
        public bool CanBeDisabled()
        {
            // Critical modules cannot be disabled
            return ModuleType != ModuleType.Dashboard &&
                   ModuleType != ModuleType.UserManagement &&
                   ModuleType != ModuleType.ApplicationSettings;
        }
        
        public List<string> GetDisableWarnings()
        {
            var warnings = new List<string>();
            
            if (SubModules?.Any() == true)
            {
                warnings.Add($"This will disable {SubModules.Count} sub-modules");
            }
            
            if (DependentModules?.Any() == true)
            {
                warnings.Add($"This may affect {DependentModules.Count} dependent modules");
            }
            
            return warnings;
        }
    }
}
```

#### ModuleDependency.cs
```csharp
namespace Harmoni360.Domain.Entities
{
    public class ModuleDependency : BaseEntity
    {
        public ModuleType ModuleType { get; set; }
        public ModuleType DependsOnModuleType { get; set; }
        public bool IsRequired { get; set; }
        
        // Navigation properties
        public virtual ModuleConfiguration Module { get; set; }
        public virtual ModuleConfiguration DependsOnModule { get; set; }
    }
}
```

### 2. Application Layer

#### DTOs
```csharp
public class ModuleConfigurationDto
{
    public ModuleType ModuleType { get; set; }
    public string ModuleName { get; set; }
    public bool IsEnabled { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public ModuleType? ParentModuleType { get; set; }
    public List<ModuleConfigurationDto> SubModules { get; set; }
    public Dictionary<string, object> Settings { get; set; }
    public bool CanBeDisabled { get; set; }
    public List<string> DisableWarnings { get; set; }
}

public class UpdateModuleConfigurationCommand
{
    public ModuleType ModuleType { get; set; }
    public bool IsEnabled { get; set; }
    public Dictionary<string, object> Settings { get; set; }
}
```

#### IModuleConfigurationService.cs
```csharp
public interface IModuleConfigurationService
{
    Task<List<ModuleConfigurationDto>> GetAllModuleConfigurationsAsync();
    Task<ModuleConfigurationDto> GetModuleConfigurationAsync(ModuleType moduleType);
    Task<bool> UpdateModuleConfigurationAsync(UpdateModuleConfigurationCommand command);
    Task<bool> IsModuleEnabledAsync(ModuleType moduleType);
    Task<List<ModuleType>> GetEnabledModulesForUserAsync(int userId);
    Task<Dictionary<ModuleType, bool>> GetModuleStatusMapAsync();
}
```

### 3. Enhanced Authorization Handler
```csharp
public class EnhancedModulePermissionHandler : AuthorizationHandler<ModulePermissionRequirement>
{
    private readonly IModuleConfigurationService _moduleConfigService;
    private readonly ModulePermissionMap _permissionMap;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModulePermissionRequirement requirement)
    {
        var user = context.User;
        
        // SuperAdmin always has access
        if (user.IsInRole("SuperAdmin") || user.IsInRole("Developer"))
        {
            context.Succeed(requirement);
            return;
        }

        // Check if module is enabled
        var isModuleEnabled = await _moduleConfigService.IsModuleEnabledAsync(requirement.Module);
        if (!isModuleEnabled)
        {
            context.Fail();
            return;
        }

        // Check user permissions using existing ModulePermissionMap
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => Enum.Parse<RoleType>(c.Value))
            .ToList();

        var hasPermission = userRoles.Any(role => 
            ModulePermissionMap.HasPermission(role, requirement.Module, requirement.Permission));
            
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
```

### 4. API Controller
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModuleConfigurationController : ControllerBase
{
    private readonly IModuleConfigurationService _service;
    private readonly ILogger<ModuleConfigurationController> _logger;
    private readonly IAuditService _auditService;
    private readonly IHubContext<ModuleConfigurationHub> _hubContext;

    [HttpGet]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<List<ModuleConfigurationDto>>> GetConfigurations()
    {
        var configurations = await _service.GetAllModuleConfigurationsAsync();
        return Ok(configurations);
    }

    [HttpPut("{moduleType}")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    public async Task<IActionResult> UpdateConfiguration(
        ModuleType moduleType,
        [FromBody] UpdateModuleConfigurationCommand command)
    {
        if (moduleType != command.ModuleType)
        {
            return BadRequest("Module type mismatch");
        }

        var result = await _service.UpdateModuleConfigurationAsync(command);
        
        if (result)
        {
            await _auditService.LogModuleConfigurationChangeAsync(
                command.ModuleType,
                command.IsEnabled ? "Enabled" : "Disabled",
                User.GetUserId());
                
            // Broadcast configuration change to connected clients
            await _hubContext.Clients.All.SendAsync("ModuleConfigurationChanged", command);
            
            return NoContent();
        }

        return BadRequest("Failed to update module configuration");
    }

    [HttpGet("status-map")]
    [Authorize] // Any authenticated user can check module status
    public async Task<ActionResult<Dictionary<ModuleType, bool>>> GetModuleStatusMap()
    {
        var statusMap = await _service.GetModuleStatusMapAsync();
        return Ok(statusMap);
    }
}
```

## Frontend Implementation

### 1. Redux State Management
```typescript
interface ModuleConfig {
  moduleType: ModuleType;
  moduleName: string;
  isEnabled: boolean;
  displayName: string;
  description: string;
  iconClass: string;
  displayOrder: number;
  parentModuleType?: ModuleType;
  subModules: ModuleConfig[];
  settings: Record<string, any>;
  canBeDisabled: boolean;
  disableWarnings: string[];
}

interface ModuleConfigState {
  configurations: ModuleConfig[];
  statusMap: Record<ModuleType, boolean>;
  loading: boolean;
  error: string | null;
}

const moduleConfigSlice = createSlice({
  name: 'moduleConfig',
  initialState,
  reducers: {
    setConfigurations: (state, action) => {
      state.configurations = action.payload;
      state.statusMap = createStatusMap(action.payload);
    },
    updateModuleStatus: (state, action) => {
      const { moduleType, isEnabled } = action.payload;
      updateConfigurationTree(state.configurations, moduleType, isEnabled);
      state.statusMap = createStatusMap(state.configurations);
    }
  }
});
```

### 2. Module Configuration UI Component
```tsx
import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Switch,
  Button,
  Tooltip,
  Tree,
  Modal,
  Alert,
  Input,
  Space,
  Tag
} from 'antd';

interface ModuleConfigurationProps {
  onConfigurationChange?: () => void;
}

const ModuleConfiguration: React.FC<ModuleConfigurationProps> = ({ 
  onConfigurationChange 
}) => {
  const dispatch = useAppDispatch();
  const { configurations, loading } = useAppSelector(state => state.moduleConfig);
  const { user } = useAppSelector(state => state.auth);
  const [searchText, setSearchText] = useState('');
  const [expandedKeys, setExpandedKeys] = useState<string[]>([]);

  const handleToggleModule = async (module: ModuleConfig) => {
    if (!module.canBeDisabled && module.isEnabled) {
      Modal.warning({
        title: 'Cannot Disable Module',
        content: 'This module is required for system operation and cannot be disabled.'
      });
      return;
    }

    if (module.isEnabled && module.disableWarnings.length > 0) {
      Modal.confirm({
        title: 'Confirm Module Disable',
        content: (
          <div>
            <p>Disabling this module will have the following effects:</p>
            <ul>
              {module.disableWarnings.map((warning, index) => (
                <li key={index}>{warning}</li>
              ))}
            </ul>
            <p>Are you sure you want to continue?</p>
          </div>
        ),
        onOk: () => updateModuleStatus(module, false)
      });
    } else {
      updateModuleStatus(module, !module.isEnabled);
    }
  };

  const updateModuleStatus = async (module: ModuleConfig, isEnabled: boolean) => {
    try {
      await moduleConfigApi.updateConfiguration({
        moduleType: module.moduleType,
        isEnabled,
        settings: module.settings
      });

      dispatch(updateModuleStatus({ 
        moduleType: module.moduleType, 
        isEnabled 
      }));

      message.success(`Module ${isEnabled ? 'enabled' : 'disabled'} successfully`);
      onConfigurationChange?.();
    } catch (error) {
      message.error('Failed to update module configuration');
    }
  };

  // Component implementation continues...
  
  return (
    <Card title="Module Configuration">
      <Alert
        message="Module Configuration Guidelines"
        description={
          <ul>
            <li>Only SuperAdmin and Developer roles can configure modules</li>
            <li>Disabling a parent module automatically disables all sub-modules</li>
            <li>SuperAdmin users have access to all modules regardless of configuration</li>
            <li>Changes take effect immediately without requiring application restart</li>
          </ul>
        }
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />
      {/* Tree component implementation */}
    </Card>
  );
};

export default ModuleConfiguration;
```

### 3. Enhanced Navigation Filtering
```typescript
export const filterNavigationByPermissions = (
  navigation: NavigationItem[],
  permissions: Permission[],
  userRoles: string[],
  moduleStatusMap: Record<ModuleType, boolean>
): NavigationItem[] => {
  const isSuperAdmin = userRoles.includes('SuperAdmin') || userRoles.includes('Developer');

  return navigation
    .map(item => {
      // For SuperAdmin, show all modules but indicate if disabled
      if (isSuperAdmin) {
        const isEnabled = moduleStatusMap[item.moduleType] ?? true;
        return {
          ...item,
          disabled: !isEnabled,
          badge: !isEnabled ? 'Disabled' : undefined,
          children: item.children ? 
            filterNavigationByPermissions(
              item.children, 
              permissions, 
              userRoles, 
              moduleStatusMap
            ) : undefined
        };
      }

      // For other users, check both module status and permissions
      const isModuleEnabled = moduleStatusMap[item.moduleType] ?? true;
      if (!isModuleEnabled) {
        return null;
      }

      // Check permissions using existing permission system
      const hasPermission = checkUserPermission(item, permissions, userRoles);
      if (!hasPermission) {
        return null;
      }

      return item;
    })
    .filter(Boolean) as NavigationItem[];
};
```

## Security Considerations

### 1. Access Control Matrix

| Scenario | Module Status | User Permission | SuperAdmin/Developer | Access Result |
|----------|--------------|-----------------|---------------------|---------------|
| 1 | Enabled | Has Permission | No | ✅ Allowed |
| 2 | Enabled | No Permission | No | ❌ Denied (403) |
| 3 | Disabled | Has Permission | No | ❌ Denied (Module Disabled) |
| 4 | Disabled | No Permission | No | ❌ Denied (Module Disabled) |
| 5 | Enabled | Any | Yes | ✅ Allowed |
| 6 | Disabled | Any | Yes | ✅ Allowed (with warning) |

### 2. Role-Based Configuration Security

```csharp
// Secure configuration endpoint
[HttpPut("{moduleType}")]
[RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateConfiguration(...)
{
    // Additional validation
    if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Developer"))
    {
        return Forbid("Only SuperAdmin and Developer can modify module configuration");
    }
    
    // Prevent disabling critical modules
    if (IsCriticalModule(moduleType) && !command.IsEnabled)
    {
        return BadRequest("Critical modules cannot be disabled");
    }
    
    // Validate dependencies
    if (!command.IsEnabled && HasDependentModules(moduleType))
    {
        return BadRequest("Cannot disable module with active dependencies");
    }
    
    // ... rest of implementation
}
```

## Implementation Roadmap

### Phase 1: Foundation (Week 1) - **CRITICAL PATH**
**Priority: HIGH | Dependencies: None**

#### 1.1 Database Infrastructure
- [ ] Create ModuleConfigurations table migration
- [ ] Create ModuleDependencies table migration  
- [ ] Create ModuleConfigurationAuditLogs table migration
- [ ] Generate and apply EF Core migrations
- [ ] Seed initial module configuration data

#### 1.2 Domain Layer
- [ ] Create ModuleConfiguration entity
- [ ] Create ModuleDependency entity
- [ ] Create ModuleConfigurationAuditLog entity
- [ ] Add domain events for configuration changes
- [ ] Implement business logic for hierarchy rules

#### 1.3 Application Layer
- [ ] Create ModuleConfigurationDto and command classes
- [ ] Implement IModuleConfigurationService interface
- [ ] Create CQRS commands and queries
- [ ] Add validation rules and business logic

### Phase 2: Backend Services (Week 1-2) - **CRITICAL PATH**
**Priority: HIGH | Dependencies: Phase 1**

#### 2.1 Service Implementation
- [ ] Implement ModuleConfigurationService
- [ ] Add caching layer (memory + distributed)
- [ ] Implement module discovery service
- [ ] Create audit service integration

#### 2.2 API Controller
- [ ] Create ModuleConfigurationController
- [ ] Implement all endpoints (GET, PUT, status-map)
- [ ] Add authorization attributes
- [ ] Integrate with existing auth system

#### 2.3 Enhanced Authorization
- [ ] Update ModulePermissionHandler for module state checks
- [ ] Integrate with existing ModulePermissionMap
- [ ] Add SuperAdmin bypass logic

#### 2.4 SignalR Integration
- [ ] Add ModuleConfigurationHub
- [ ] Implement real-time configuration updates
- [ ] Update client connection management

### Phase 3: Frontend Implementation (Week 2) - **HIGH PRIORITY**
**Priority: HIGH | Dependencies: Phase 2**

#### 3.1 Redux Integration
- [ ] Create moduleConfigSlice
- [ ] Add RTK Query endpoints for module configuration API
- [ ] Update existing ModuleStateContext integration
- [ ] Implement cache invalidation strategies

#### 3.2 Admin UI Components
- [ ] Create ModuleConfiguration.tsx component
- [ ] Build tree view with enable/disable switches
- [ ] Add confirmation dialogs for critical modules
- [ ] Implement search and filtering functionality
- [ ] Add bulk operations support

#### 3.3 Navigation Integration
- [ ] Update DefaultLayout to use Redux state
- [ ] Connect SignalR for real-time updates
- [ ] Add module status badges to navigation
- [ ] Implement SuperAdmin override indicators

#### 3.4 Route Protection
- [ ] Create ModuleRouteGuard component
- [ ] Update App.tsx routing with guards
- [ ] Add proper error pages for disabled modules

### Phase 4: Testing & Documentation (Week 3) - **MEDIUM PRIORITY**
**Priority: MEDIUM | Dependencies: Phase 3**

#### 4.1 Testing
- [ ] Unit tests for all services and handlers
- [ ] Integration tests for API endpoints
- [ ] Frontend component tests for module configuration
- [ ] E2E test scenarios for complete workflows

#### 4.2 Documentation
- [ ] API documentation for module configuration endpoints
- [ ] Admin user guide for module management
- [ ] Developer documentation for extending the system
- [ ] Migration guide for existing installations

### Timeline Summary
- **Week 1**: Backend infrastructure and services (Phases 1-2)
- **Week 2**: Frontend implementation and integration (Phase 3)
- **Week 3**: Testing, documentation, and deployment prep (Phase 4)
- **Total Duration**: 3 weeks (15 business days)

## Testing Strategy

### 1. Unit Tests
```csharp
[TestClass]
public class ModuleConfigurationServiceTests
{
    [TestMethod]
    public async Task DisablingParentModule_ShouldDisableAllSubModules()
    {
        // Arrange
        var service = new ModuleConfigurationService(/* dependencies */);
        var parentModule = ModuleType.SecurityManager;
        
        // Act
        await service.UpdateModuleConfigurationAsync(new UpdateModuleConfigurationCommand
        {
            ModuleType = parentModule,
            IsEnabled = false
        });
        
        // Assert
        var subModules = await service.GetSubModulesAsync(parentModule);
        Assert.IsTrue(subModules.All(m => !m.IsEnabled));
    }
    
    [TestMethod]
    public async Task SuperAdmin_ShouldAccessDisabledModules()
    {
        // Test SuperAdmin bypass functionality
    }
    
    [TestMethod]
    public async Task CriticalModules_CannotBeDisabled()
    {
        // Test that Dashboard, UserManagement, ApplicationSettings cannot be disabled
    }
}
```

### 2. Integration Tests
```csharp
[TestClass]
public class ModuleConfigurationIntegrationTests
{
    [TestMethod]
    public async Task ModuleConfiguration_EndToEnd_Test()
    {
        // 1. SuperAdmin disables a module
        // 2. Regular user cannot access the module
        // 3. SuperAdmin can still access with warning
        // 4. Module re-enabled
        // 5. Regular user can access again
    }
}
```

### 3. Frontend Tests
```typescript
describe('ModuleConfiguration Component', () => {
  it('should disable sub-modules when parent is disabled', async () => {
    const { getByTestId } = render(<ModuleConfiguration />);
    
    // Disable parent module
    const parentSwitch = getByTestId('module-switch-SecurityManager');
    fireEvent.click(parentSwitch);
    
    // Verify sub-modules are disabled
    await waitFor(() => {
      const subModuleSwitch = getByTestId('module-switch-SecurityOfficer');
      expect(subModuleSwitch).toBeDisabled();
    });
  });
  
  it('should show warning for modules with dependencies', async () => {
    // Test dependency warning system
  });
});
```

## Success Metrics

### Target Metrics
1. **Functionality**: All modules can be enabled/disabled with proper cascade effects
2. **Performance**: Configuration changes apply in < 1 second
3. **Security**: No unauthorized access to disabled modules
4. **Real-time Updates**: Instant synchronization across all connected clients
5. **User Experience**: 90%+ user satisfaction in configuration management
6. **Reliability**: Zero critical bugs in production after 30 days

### Business Value
- **Operational Flexibility**: Enable/disable modules without deployment
- **Risk Mitigation**: Quickly disable problematic modules
- **Cost Optimization**: License modules independently
- **User Experience**: Simplified interface based on available modules

## Conclusion

The Module Configuration system requires a **complete fresh start** with proper backend infrastructure. The current authorization system provides an excellent foundation, but the configuration layer must be built from scratch.

**Key Deliverables for Success:**
1. ✅ Robust role-based permission system (already exists)
2. ❌ Backend configuration API and persistence (needs implementation)
3. ❌ Admin UI for module management (needs implementation)  
4. ❌ Real-time synchronization (needs implementation)
5. ❌ Comprehensive testing (needs implementation)

**Recommended Approach:** Start with Phase 1 (Database Infrastructure) and work systematically through each phase. The existing authorization system should be preserved and integrated with the new configuration system.

**Timeline**: 3 weeks for complete implementation, with the first functional version available after 2 weeks.