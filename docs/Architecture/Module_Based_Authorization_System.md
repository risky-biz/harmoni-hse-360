# Module-Based Authorization System Documentation

## Overview

The Harmoni360 application implements a sophisticated module-based authorization system that provides fine-grained access control across all functional areas. This system replaces traditional role-based access control (RBAC) with a more flexible and scalable approach.

## Table of Contents

1. [System Architecture](#system-architecture)
2. [Role Hierarchy](#role-hierarchy)
3. [Module Structure](#module-structure)
4. [Permission Types](#permission-types)
5. [Permission Matrix](#permission-matrix)
6. [Implementation Details](#implementation-details)
7. [Usage Examples](#usage-examples)
8. [Migration Guide](#migration-guide)

## System Architecture

### Core Components

```
┌─────────────────────────────────────────────────────────────┐
│                    Authorization Flow                        │
├─────────────────────────────────────────────────────────────┤
│ 1. HTTP Request with JWT Token                              │
│ 2. ModulePermissionHandler validates claims                 │
│ 3. ModulePermissionMap checks role permissions             │
│ 4. Access granted/denied based on module + permission      │
└─────────────────────────────────────────────────────────────┘

┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Controllers   │───▶│  Authorization   │───▶│   Permission    │
│   (Endpoints)   │    │   Attributes     │    │    Handlers     │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                │                        │
                                ▼                        ▼
                       ┌──────────────────┐    ┌─────────────────┐
                       │   Role Types     │    │ Module-Permission│
                       │   (Enums)        │    │    Mappings     │
                       └──────────────────┘    └─────────────────┘
```

### Database Schema

```
┌─────────────────┐    ┌──────────────────────┐    ┌─────────────────┐
│     Roles       │    │ RoleModulePermissions│    │ModulePermissions│
├─────────────────┤    ├──────────────────────┤    ├─────────────────┤
│ Id              │◄──┤│ RoleId               │├──►│ Id              │
│ Name            │    │ ModulePermissionId   │    │ Module (enum)   │
│ RoleType (enum) │    │ IsActive             │    │ Permission (enum)│
│ IsActive        │    │ GrantedAt            │    │ Name            │
│ DisplayOrder    │    │ GrantedByUserId      │    │ Description     │
└─────────────────┘    │ GrantReason          │    │ IsActive        │
                       └──────────────────────┘    └─────────────────┘
```

## Role Hierarchy

### Role Structure (Hierarchical)

```
                            ┌─────────────┐
                            │ SuperAdmin  │ ← Full system access
                            │  (Level 1)  │
                            └─────────────┘
                                   │
                            ┌─────────────┐
                            │ Developer   │ ← Technical + Admin access
                            │  (Level 2)  │
                            └─────────────┘
                                   │
                            ┌─────────────┐
                            │    Admin    │ ← Administrative access
                            │  (Level 3)  │
                            └─────────────┘
                                   │
                ┌──────────────────┼──────────────────┐
                │                  │                  │
         ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
         │IncidentMgr  │    │ RiskManager │    │ PPEManager  │
         │  (Level 4)  │    │  (Level 4)  │    │  (Level 4)  │
         └─────────────┘    └─────────────┘    └─────────────┘
                │                  │                  │
         ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
         │HealthMonitor│    │   Reporter  │    │   Viewer    │
         │  (Level 4)  │    │  (Level 5)  │    │  (Level 6)  │
         └─────────────┘    └─────────────┘    └─────────────┘
```

### Role Definitions

| Role | Level | Description | Primary Modules |
|------|-------|-------------|-----------------|
| **SuperAdmin** | 1 | Complete system control, all permissions | All modules with full access |
| **Developer** | 2 | Technical administration + debugging access | All modules, system configuration |
| **Admin** | 3 | General administrative oversight | User Management, Application Settings |
| **IncidentManager** | 4 | Incident response and management | Incident Management, Reporting |
| **RiskManager** | 4 | Risk assessment and hazard management | Risk Management, Reporting |
| **PPEManager** | 4 | Personal protective equipment oversight | PPE Management, Reporting |
| **HealthMonitor** | 4 | Health and safety monitoring | Health Monitoring, Reporting |
| **Reporter** | 5 | Incident and hazard reporting | Limited reporting capabilities |
| **Viewer** | 6 | Read-only access to assigned modules | Dashboard viewing only |

## Module Structure

### System Modules

```
┌─────────────────────────────────────────────────────────────┐
│                        System Modules                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Dashboard  │  │ Incident     │  │ Risk         │      │
│  │              │  │ Management   │  │ Management   │      │
│  │ • Overview   │  │              │  │              │      │
│  │ • Metrics    │  │ • Reports    │  │ • Hazards    │      │
│  │ • Analytics  │  │ • Tracking   │  │ • Assessments│      │
│  └──────────────┘  │ • Workflow   │  │ • Mitigation │      │
│                    └──────────────┘  └──────────────┘      │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ PPE          │  │ Health       │  │ Reporting    │      │
│  │ Management   │  │ Monitoring   │  │              │      │
│  │              │  │              │  │ • Analytics  │      │
│  │ • Equipment  │  │ • Records    │  │ • Export     │      │
│  │ • Assignments│  │ • Medical    │  │ • Dashboard  │      │
│  │ • Compliance │  │ • Emergency  │  └──────────────┘      │
│  └──────────────┘  └──────────────┘                        │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐                        │
│  │ User         │  │ Application  │                        │
│  │ Management   │  │ Settings     │                        │
│  │              │  │              │                        │
│  │ • Users      │  │ • Config     │                        │
│  │ • Roles      │  │ • Notifications                      │
│  │ • Permissions│  │ • System     │                        │
│  └──────────────┘  └──────────────┘                        │
└─────────────────────────────────────────────────────────────┘
```

### Module Descriptions

| Module | Purpose | Key Features |
|--------|---------|--------------|
| **Dashboard** | System overview and metrics | Real-time statistics, KPIs, trends |
| **IncidentManagement** | Safety incident tracking | Report creation, workflow, analysis |
| **RiskManagement** | Hazard identification and mitigation | Risk assessments, hazard mapping |
| **PPEManagement** | Equipment lifecycle management | Inventory, assignments, compliance |
| **HealthMonitoring** | Personnel health and safety | Medical records, emergency contacts |
| **Reporting** | Data analytics and export | Custom reports, data visualization |
| **UserManagement** | User and role administration | Account management, permissions |
| **ApplicationSettings** | System configuration | Notifications, preferences, settings |

## Permission Types

### Permission Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│                    Permission Levels                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ │
│ │  Read   │ │ Create  │ │ Update  │ │ Delete  │ │ Export  │ │
│ │ (Level1)│ │(Level2) │ │(Level3) │ │(Level4) │ │(Level4) │ │
│ └─────────┘ └─────────┘ └─────────┘ └─────────┘ └─────────┘ │
│                                                             │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐                         │
│ │Configure│ │ Approve │ │ Assign  │                         │
│ │(Level5) │ │(Level5) │ │(Level5) │                         │
│ └─────────┘ └─────────┘ └─────────┘                         │
└─────────────────────────────────────────────────────────────┘
```

### Permission Definitions

| Permission | Level | Description | Usage Examples |
|------------|-------|-------------|----------------|
| **Read** | 1 | View and access data | Dashboard viewing, report reading |
| **Create** | 2 | Add new records | Creating incidents, adding equipment |
| **Update** | 3 | Modify existing data | Editing reports, updating status |
| **Delete** | 4 | Remove records | Deleting obsolete data |
| **Export** | 4 | Data extraction | Generating reports, data downloads |
| **Configure** | 5 | System administration | Settings, advanced features |
| **Approve** | 5 | Workflow authorization | Approving requests, validating data |
| **Assign** | 5 | Resource allocation | User assignments, equipment distribution |

## Permission Matrix

### Complete Role-Module-Permission Matrix

| Role → | SuperAdmin | Developer | Admin | IncidentMgr | RiskMgr | PPEMgr | HealthMonitor | Reporter | Viewer |
|--------|:----------:|:---------:|:-----:|:-----------:|:-------:|:------:|:-------------:|:--------:|:------:|
| **Dashboard** | | | | | | | | | |
| Read | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Configure | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **IncidentManagement** | | | | | | | | | |
| Read | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |
| Create | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ |
| Update | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Delete | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Export | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Configure | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Approve | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **RiskManagement** | | | | | | | | | |
| Read | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |
| Create | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ✅ | ❌ |
| Update | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Delete | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Export | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Configure | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Approve | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **PPEManagement** | | | | | | | | | |
| Read | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Create | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Update | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Delete | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Export | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Configure | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Assign | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| **HealthMonitoring** | | | | | | | | | |
| Read | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Create | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Update | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Delete | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Export | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Configure | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| **Reporting** | | | | | | | | | |
| Read | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Export | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Configure | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **UserManagement** | | | | | | | | | |
| Read | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Create | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Update | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Delete | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Assign | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **ApplicationSettings** | | | | | | | | | |
| Read | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Update | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Configure | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

### Permission Summary by Role

| Role | Total Permissions | Modules Accessible | Primary Capabilities |
|------|:-----------------:|:------------------:|---------------------|
| **SuperAdmin** | 64 (All) | 8 (All) | Complete system control |
| **Developer** | 58 | 8 (All) | Technical administration |
| **Admin** | 32 | 6 (Dashboard, User Mgmt, App Settings, Partial others) | Administrative oversight |
| **IncidentManager** | 18 | 3 (Dashboard, Incident Mgmt, Reporting) | Incident response |
| **RiskManager** | 18 | 3 (Dashboard, Risk Mgmt, Reporting) | Risk assessment |
| **PPEManager** | 17 | 3 (Dashboard, PPE Mgmt, Reporting) | Equipment management |
| **HealthMonitor** | 17 | 3 (Dashboard, Health Monitoring, Reporting) | Health oversight |
| **Reporter** | 9 | 4 (Dashboard, Incident/Risk reporting, Reporting) | Data collection |
| **Viewer** | 3 | 3 (Dashboard, Limited reporting) | Read-only access |

## Implementation Details

### Authorization Attributes

```csharp
// Basic module permission requirement
[RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
public async Task<ActionResult<IncidentDto>> GetIncident(int id)

// Multiple permission requirements (OR logic)
[RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
[RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Create)]
public async Task<ActionResult> ManageIncident([FromBody] IncidentRequest request)

// Role-based fallback for special cases
[RequireRoles(RoleType.SuperAdmin, RoleType.Developer)]
public async Task<ActionResult> SystemMaintenance()
```

### Permission Checking Logic

```csharp
// Runtime permission checking
public bool HasPermission(RoleType role, ModuleType module, PermissionType permission)
{
    var rolePermissions = GetRoleModulePermissions();
    
    if (!rolePermissions.ContainsKey(role))
        return false;
        
    var modulePermissions = rolePermissions[role];
    
    return modulePermissions.ContainsKey(module) && 
           modulePermissions[module].Contains(permission);
}

// Role hierarchy checking
public bool HasRoleAccess(RoleType userRole, RoleType requiredRole)
{
    return GetRoleHierarchyLevel(userRole) <= GetRoleHierarchyLevel(requiredRole);
}
```

### Database Integration

```csharp
// Seed default module permissions
public static void SeedModulePermissions(ApplicationDbContext context)
{
    var modulePermissions = new List<ModulePermission>();
    
    foreach (var module in Enum.GetValues<ModuleType>())
    {
        foreach (var permission in Enum.GetValues<PermissionType>())
        {
            modulePermissions.Add(ModulePermission.Create(
                module, 
                permission, 
                $"{module}.{permission}",
                $"Allows {permission} access to {module} module"
            ));
        }
    }
    
    context.ModulePermissions.AddRange(modulePermissions);
    context.SaveChanges();
}
```

## Usage Examples

### Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Basic authentication required
public class IncidentController : ControllerBase
{
    [HttpGet]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> GetIncidents()
    {
        // Implementation
    }
    
    [HttpPost]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Create)]
    public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] CreateIncidentRequest request)
    {
        // Implementation
    }
    
    [HttpDelete("{id}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Delete)]
    public async Task<ActionResult> DeleteIncident(int id)
    {
        // Implementation
    }
}
```

### Frontend Permission Checking

```typescript
// Permission utility functions
export const hasModulePermission = (
  userRoles: string[],
  module: ModuleType,
  permission: PermissionType
): boolean => {
  return userRoles.some(role => 
    modulePermissionMap[role]?.[module]?.includes(permission)
  );
};

// Component usage
const IncidentListComponent = () => {
  const { userRoles } = useAuth();
  const canCreate = hasModulePermission(userRoles, 'IncidentManagement', 'Create');
  const canDelete = hasModulePermission(userRoles, 'IncidentManagement', 'Delete');
  
  return (
    <div>
      {canCreate && <CreateIncidentButton />}
      {canDelete && <DeleteIncidentButton />}
    </div>
  );
};
```

## Migration Guide

### From Legacy Role-Based to Module-Based

#### Step 1: Update Database Schema
```sql
-- Add new columns to Roles table
ALTER TABLE "Roles" ADD COLUMN "RoleType" INTEGER NOT NULL DEFAULT 9;
ALTER TABLE "Roles" ADD COLUMN "IsActive" BOOLEAN NOT NULL DEFAULT TRUE;
ALTER TABLE "Roles" ADD COLUMN "DisplayOrder" INTEGER NOT NULL DEFAULT 0;

-- Create new tables
CREATE TABLE "ModulePermissions" (...);
CREATE TABLE "RoleModulePermissions" (...);
```

#### Step 2: Map Existing Roles
```sql
-- Update existing roles with appropriate RoleType values
UPDATE "Roles" SET "RoleType" = 1 WHERE "Name" = 'SuperAdmin';
UPDATE "Roles" SET "RoleType" = 2 WHERE "Name" = 'Developer';
UPDATE "Roles" SET "RoleType" = 3 WHERE "Name" IN ('Admin', 'Administrator');
-- ... continue for all roles
```

#### Step 3: Replace Authorization Attributes
```csharp
// Old approach
[Authorize(Roles = "SuperAdmin,Developer,HealthManager,Administrator")]

// New approach
[RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
```

#### Step 4: Update Frontend Components
```typescript
// Old approach
const canAccess = userRoles.includes('HealthManager');

// New approach  
const canAccess = hasModulePermission(userRoles, 'HealthMonitoring', 'Read');
```

### Backward Compatibility

The system maintains backward compatibility through:

1. **Legacy role support**: Old role names are mapped to new RoleType enums
2. **Gradual migration**: Both systems can coexist during transition
3. **Fallback mechanisms**: Legacy role checks as secondary authorization
4. **Database preservation**: Existing role data is preserved and enhanced

## Best Practices

### Security Guidelines

1. **Principle of Least Privilege**: Grant minimal required permissions
2. **Role Segregation**: Separate functional responsibilities clearly
3. **Regular Audits**: Review role assignments and permissions periodically
4. **Documentation**: Maintain current permission matrix documentation
5. **Testing**: Validate authorization for all critical operations

### Performance Considerations

1. **Caching**: Permission maps are cached for performance
2. **Indexing**: Database indexes on role and module fields
3. **Lazy Loading**: Load permissions only when needed
4. **Bulk Operations**: Use efficient bulk permission checks

### Maintenance

1. **Version Control**: Track permission changes in source control
2. **Environment Consistency**: Ensure permissions match across environments
3. **Migration Scripts**: Automate permission updates during deployments
4. **Monitoring**: Log authorization failures for security analysis

## Troubleshooting

### Common Issues

1. **403 Forbidden Errors**: Check role assignments and module permissions
2. **Missing Permissions**: Verify ModulePermissionMap includes required permissions
3. **Role Hierarchy Issues**: Ensure RoleType enum values reflect hierarchy
4. **Database Sync**: Confirm migration scripts have been applied

### Debugging Tools

```csharp
// Enable detailed authorization logging
services.Configure<AuthorizationOptions>(options =>
{
    options.AddPolicy("Debug", policy =>
        policy.Requirements.Add(new DebugAuthorizationRequirement()));
});

// Check user permissions programmatically
public async Task<IActionResult> DebugPermissions()
{
    var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    var permissions = new Dictionary<string, object>();
    
    foreach (var role in userRoles)
    {
        if (Enum.TryParse<RoleType>(role, out var roleType))
        {
            permissions[role] = ModulePermissionMap.GetRolePermissions(roleType);
        }
    }
    
    return Ok(permissions);
}
```

## Conclusion

The module-based authorization system provides Harmoni360 with:

- **Scalability**: Easy addition of new modules and permissions
- **Flexibility**: Fine-grained control over user access
- **Security**: Comprehensive protection across all functionality
- **Maintainability**: Clear separation of concerns and documentation
- **User Experience**: Role-appropriate interfaces and workflows

This system serves as the foundation for secure, scalable access control throughout the application lifecycle.