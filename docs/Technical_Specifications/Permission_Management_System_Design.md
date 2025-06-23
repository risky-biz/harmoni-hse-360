# Permission Management System Design
## Harmoni360 HSSE Application - Enhanced RBAC Implementation

### Overview

This document details the comprehensive design for an enhanced Role-Based Access Control (RBAC) system for the Harmoni360 HSSE application. The design builds upon the existing ModulePermissionMap architecture while introducing advanced features for dynamic permission assignment, inheritance, and performance optimization.

### Current Permission Architecture Analysis

#### Existing Role Structure
The current system defines 7 primary roles with specific module access patterns:

1. **SuperAdmin** - Complete system access including ALL modules and application settings
2. **Developer** - Full development and configuration access
3. **Admin** - Comprehensive access to all modules except application settings
4. **SecurityManager** - Complete access to all Security modules (Physical, Information, Personnel, Incident)
5. **IncidentManager** - Restricted access to Incident Management module only
6. **RiskManager** - Restricted access to Risk Management module only
7. **PPEManager** - Restricted access to PPE Management module only

#### Current Module Types
```csharp
public enum ModuleType
{
    Dashboard = 1,
    IncidentManagement = 2,
    RiskManagement = 3,
    PPEManagement = 4,
    HealthMonitoring = 5,
    PhysicalSecurity = 6,
    InformationSecurity = 7,
    PersonnelSecurity = 8,
    SecurityIncidentManagement = 9,
    ComplianceManagement = 10,
    Reporting = 11,
    UserManagement = 12,
    WorkPermitManagement = 14,
    InspectionManagement = 15,
    AuditManagement = 16,
    TrainingManagement = 17,
    LicenseManagement = 18,
    WasteManagement = 19,
    ApplicationSettings = 20
}
```

#### Current Permission Types
```csharp
public enum PermissionType
{
    Create = 1,
    Read = 2,
    Update = 3,
    Delete = 4,
    Export = 5,
    Configure = 6,
    Approve = 7,
    Assign = 8
}
```

### Enhanced Permission Management Design

#### Role Hierarchy Implementation

**Hierarchical Role Structure:**
```
SuperAdmin (Level 0)
├── Developer (Level 1)
├── Admin (Level 1)
│   ├── SecurityManager (Level 2)
│   ├── ComplianceOfficer (Level 2)
│   └── InspectionManager (Level 2)
├── Functional Managers (Level 2)
│   ├── IncidentManager
│   ├── RiskManager
│   ├── PPEManager
│   └── HealthMonitor
└── Operational Users (Level 3)
    ├── Reporter
    └── Viewer
```

**Role Hierarchy Benefits:**
- Automatic permission inheritance from parent roles
- Simplified role management and assignment
- Clear escalation paths for access requests
- Reduced complexity in permission matrix

#### Enhanced Permission Types

**Granular Permission Categories:**
```csharp
public enum PermissionType
{
    // Basic CRUD
    Create = 1,
    Read = 2,
    Update = 3,
    Delete = 4,
    
    // Data Operations
    Export = 5,
    Import = 6,
    BulkEdit = 7,
    
    // Administrative
    Configure = 8,
    Approve = 9,
    Assign = 10,
    Delegate = 11,
    
    // Workflow
    Submit = 12,
    Review = 13,
    Reject = 14,
    Close = 15,
    
    // Reporting
    ViewReports = 16,
    CreateReports = 17,
    ScheduleReports = 18,
    
    // Security
    ViewAuditLog = 19,
    ManageUsers = 20,
    ManageRoles = 21,
    
    // Emergency
    EmergencyAccess = 22,
    OverrideApproval = 23
}
```

#### Context-Aware Permissions

**Data Ownership Permissions:**
```csharp
public class ContextualPermission
{
    public PermissionType Permission { get; set; }
    public PermissionScope Scope { get; set; }
    public List<PermissionCondition> Conditions { get; set; }
}

public enum PermissionScope
{
    Global,           // Access to all records
    Department,       // Access to department records only
    Team,            // Access to team records only
    Own,             // Access to own records only
    Assigned,        // Access to assigned records only
    Supervised       // Access to supervised user records
}

public class PermissionCondition
{
    public string Field { get; set; }
    public ConditionOperator Operator { get; set; }
    public object Value { get; set; }
}
```

**Location-Based Access Control:**
```csharp
public class LocationPermission
{
    public int UserId { get; set; }
    public string LocationCode { get; set; }
    public ModuleType Module { get; set; }
    public List<PermissionType> Permissions { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
}
```

#### Dynamic Permission Assignment

**Temporary Permission Elevation:**
```csharp
public class TemporaryPermission
{
    public int UserId { get; set; }
    public ModuleType Module { get; set; }
    public PermissionType Permission { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int GrantedBy { get; set; }
    public string Justification { get; set; }
    public bool RequiresApproval { get; set; }
    public TemporaryPermissionStatus Status { get; set; }
}

public enum TemporaryPermissionStatus
{
    Pending,
    Approved,
    Rejected,
    Active,
    Expired,
    Revoked
}
```

**Project-Based Permissions:**
```csharp
public class ProjectPermission
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public ProjectRole Role { get; set; }
    public List<ModulePermission> ModulePermissions { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
}

public enum ProjectRole
{
    ProjectManager,
    TeamLead,
    TeamMember,
    Observer,
    Consultant
}
```

#### Permission Inheritance System

**Inheritance Rules:**
1. **Role Hierarchy Inheritance**: Child roles inherit all permissions from parent roles
2. **Organizational Inheritance**: Users inherit permissions based on organizational structure
3. **Project Team Inheritance**: Project team members inherit base project permissions
4. **Delegation Inheritance**: Delegated permissions are inherited temporarily

**Inheritance Resolution Algorithm:**
```csharp
public class PermissionResolver
{
    public async Task<UserPermissions> ResolveUserPermissions(int userId)
    {
        var permissions = new UserPermissions();
        
        // 1. Get direct role permissions
        var rolePermissions = await GetRolePermissions(userId);
        permissions.AddRange(rolePermissions);
        
        // 2. Apply role hierarchy inheritance
        var inheritedPermissions = await GetInheritedPermissions(userId);
        permissions.AddRange(inheritedPermissions);
        
        // 3. Add contextual permissions
        var contextualPermissions = await GetContextualPermissions(userId);
        permissions.AddRange(contextualPermissions);
        
        // 4. Add temporary permissions
        var temporaryPermissions = await GetActiveTemporaryPermissions(userId);
        permissions.AddRange(temporaryPermissions);
        
        // 5. Apply permission restrictions
        var restrictions = await GetPermissionRestrictions(userId);
        permissions.ApplyRestrictions(restrictions);
        
        return permissions;
    }
}
```

#### Performance Optimization Strategy

**Multi-Level Caching:**
```csharp
public class PermissionCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<PermissionCacheService> _logger;
    
    // L1 Cache: In-memory (1 minute TTL)
    public async Task<UserPermissions?> GetFromMemoryCache(int userId)
    {
        return _memoryCache.Get<UserPermissions>($"permissions:user:{userId}");
    }
    
    // L2 Cache: Redis (15 minutes TTL)
    public async Task<UserPermissions?> GetFromDistributedCache(int userId)
    {
        var cached = await _distributedCache.GetStringAsync($"permissions:user:{userId}");
        return cached != null ? JsonSerializer.Deserialize<UserPermissions>(cached) : null;
    }
    
    // Cache invalidation strategies
    public async Task InvalidateUserPermissions(int userId)
    {
        _memoryCache.Remove($"permissions:user:{userId}");
        await _distributedCache.RemoveAsync($"permissions:user:{userId}");
        
        // Invalidate related caches
        await InvalidateRolePermissions(userId);
        await InvalidateTeamPermissions(userId);
    }
}
```

**Permission Precomputation:**
```csharp
public class PermissionPrecomputeService
{
    public async Task PrecomputeUserPermissions(int userId)
    {
        var permissions = await _permissionResolver.ResolveUserPermissions(userId);
        var serialized = JsonSerializer.Serialize(permissions);
        
        await _distributedCache.SetStringAsync(
            $"permissions:user:{userId}",
            serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });
    }
    
    public async Task PrecomputeAllActiveUserPermissions()
    {
        var activeUsers = await _userRepository.GetActiveUsersAsync();
        var tasks = activeUsers.Select(user => PrecomputeUserPermissions(user.Id));
        await Task.WhenAll(tasks);
    }
}
```

#### Database Schema Design

**Enhanced Permission Tables:**
```sql
-- Role hierarchy table
CREATE TABLE RoleHierarchy (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ParentRoleId INT NOT NULL,
    ChildRoleId INT NOT NULL,
    Level INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ParentRoleId) REFERENCES Roles(Id),
    FOREIGN KEY (ChildRoleId) REFERENCES Roles(Id),
    UNIQUE(ParentRoleId, ChildRoleId)
);

-- Contextual permissions table
CREATE TABLE ContextualPermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ModuleType INT NOT NULL,
    PermissionType INT NOT NULL,
    Scope NVARCHAR(20) NOT NULL,
    Conditions NVARCHAR(MAX) NULL, -- JSON
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

-- Temporary permissions table
CREATE TABLE TemporaryPermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ModuleType INT NOT NULL,
    PermissionType INT NOT NULL,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    GrantedBy INT NOT NULL,
    Justification NVARCHAR(500) NOT NULL,
    RequiresApproval BIT NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0,
    ApprovedBy INT NULL,
    ApprovedAt DATETIME2 NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (GrantedBy) REFERENCES Users(Id),
    FOREIGN KEY (ApprovedBy) REFERENCES Users(Id)
);

-- Location-based permissions table
CREATE TABLE LocationPermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    LocationCode NVARCHAR(50) NOT NULL,
    ModuleType INT NOT NULL,
    Permissions NVARCHAR(MAX) NOT NULL, -- JSON array
    ValidFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ValidUntil DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

-- Permission audit log
CREATE TABLE PermissionAuditLog (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- Grant, Revoke, Modify, Inherit
    PermissionType INT NOT NULL,
    ModuleType INT NOT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    PerformedBy INT NOT NULL,
    PerformedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Reason NVARCHAR(500) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PerformedBy) REFERENCES Users(Id)
);

-- Indexes for performance
CREATE INDEX IX_ContextualPermissions_UserId ON ContextualPermissions(UserId);
CREATE INDEX IX_TemporaryPermissions_UserId_Status ON TemporaryPermissions(UserId, Status);
CREATE INDEX IX_TemporaryPermissions_ExpiresAt ON TemporaryPermissions(ExpiresAt);
CREATE INDEX IX_LocationPermissions_UserId_LocationCode ON LocationPermissions(UserId, LocationCode);
CREATE INDEX IX_PermissionAuditLog_UserId_PerformedAt ON PermissionAuditLog(UserId, PerformedAt);
```

#### API Implementation

**Permission Management Controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionManagementController : ControllerBase
{
    [HttpGet("users/{userId}/permissions")]
    [RequirePermission(ModuleType.UserManagement, PermissionType.Read)]
    public async Task<ActionResult<UserPermissionsDto>> GetUserPermissions(int userId)

    [HttpPost("users/{userId}/temporary-permissions")]
    [RequirePermission(ModuleType.UserManagement, PermissionType.Assign)]
    public async Task<ActionResult> GrantTemporaryPermission(int userId, GrantTemporaryPermissionCommand command)

    [HttpDelete("temporary-permissions/{permissionId}")]
    [RequirePermission(ModuleType.UserManagement, PermissionType.Assign)]
    public async Task<ActionResult> RevokeTemporaryPermission(int permissionId)

    [HttpPost("users/{userId}/contextual-permissions")]
    [RequirePermission(ModuleType.UserManagement, PermissionType.Configure)]
    public async Task<ActionResult> SetContextualPermissions(int userId, SetContextualPermissionsCommand command)

    [HttpGet("roles/{roleId}/effective-permissions")]
    [RequirePermission(ModuleType.UserManagement, PermissionType.Read)]
    public async Task<ActionResult<RolePermissionsDto>> GetEffectiveRolePermissions(int roleId)

    [HttpPost("permissions/bulk-assign")]
    [RequirePermission(ModuleType.UserManagement, PermissionType.Assign)]
    public async Task<ActionResult> BulkAssignPermissions(BulkAssignPermissionsCommand command)
}
```

**Permission Checking Middleware:**
```csharp
public class PermissionCheckingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionCheckingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var permissionAttribute = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();

        if (permissionAttribute != null)
        {
            var userId = context.User.GetUserId();
            var hasPermission = await _permissionService.HasPermissionAsync(
                userId,
                permissionAttribute.Module,
                permissionAttribute.Permission,
                context);

            if (!hasPermission)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Access denied");
                return;
            }
        }

        await _next(context);
    }
}
```

#### Frontend Permission Components

**Permission Guard Hook:**
```typescript
export const usePermissions = () => {
  const { user } = useAuth();
  const { data: permissions } = useQuery(
    ['permissions', user?.id],
    () => permissionApi.getUserPermissions(user!.id),
    { enabled: !!user?.id }
  );

  const hasPermission = useCallback((
    module: ModuleType,
    permission: PermissionType,
    context?: PermissionContext
  ) => {
    if (!permissions) return false;

    // Check direct permissions
    const directPermission = permissions.modulePermissions
      .find(mp => mp.module === module)
      ?.permissions.includes(permission);

    if (directPermission) return true;

    // Check contextual permissions
    if (context) {
      return checkContextualPermission(permissions, module, permission, context);
    }

    return false;
  }, [permissions]);

  const hasAnyPermission = useCallback((
    checks: Array<{ module: ModuleType; permission: PermissionType }>
  ) => {
    return checks.some(check => hasPermission(check.module, check.permission));
  }, [hasPermission]);

  return {
    permissions,
    hasPermission,
    hasAnyPermission,
    isLoading: !permissions
  };
};
```

**Permission Matrix Component:**
```typescript
interface PermissionMatrixProps {
  userId?: number;
  roleId?: number;
  showInherited?: boolean;
  editable?: boolean;
}

export const PermissionMatrix: React.FC<PermissionMatrixProps> = ({
  userId,
  roleId,
  showInherited = true,
  editable = false
}) => {
  const { data: permissions } = useQuery(
    ['permission-matrix', userId, roleId],
    () => userId
      ? permissionApi.getUserPermissions(userId)
      : permissionApi.getRolePermissions(roleId!)
  );

  const modules = Object.values(ModuleType);
  const permissionTypes = Object.values(PermissionType);

  return (
    <div className="permission-matrix">
      <table className="matrix-table">
        <thead>
          <tr>
            <th>Module</th>
            {permissionTypes.map(permission => (
              <th key={permission}>{permission}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {modules.map(module => (
            <tr key={module}>
              <td>{module}</td>
              {permissionTypes.map(permission => (
                <td key={`${module}-${permission}`}>
                  <PermissionCell
                    module={module}
                    permission={permission}
                    hasPermission={hasPermission(module, permission)}
                    isInherited={isInherited(module, permission)}
                    editable={editable}
                    onChange={handlePermissionChange}
                  />
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
```

#### Security and Compliance Features

**Permission Audit Service:**
```csharp
public class PermissionAuditService
{
    public async Task LogPermissionChange(
        int userId,
        string action,
        PermissionType permissionType,
        ModuleType moduleType,
        object oldValue,
        object newValue,
        int performedBy,
        string reason = null)
    {
        var auditLog = new PermissionAuditLog
        {
            UserId = userId,
            Action = action,
            PermissionType = permissionType,
            ModuleType = moduleType,
            OldValue = JsonSerializer.Serialize(oldValue),
            NewValue = JsonSerializer.Serialize(newValue),
            PerformedBy = performedBy,
            PerformedAt = DateTime.UtcNow,
            Reason = reason
        };

        await _context.PermissionAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();

        // Send real-time notification
        await _notificationService.NotifyPermissionChange(auditLog);
    }
}
```

**Compliance Reporting:**
```csharp
public class PermissionComplianceService
{
    public async Task<ComplianceReport> GenerateAccessReviewReport(DateTime fromDate, DateTime toDate)
    {
        var report = new ComplianceReport
        {
            Period = new DateRange(fromDate, toDate),
            UserPermissionChanges = await GetUserPermissionChanges(fromDate, toDate),
            RoleAssignmentChanges = await GetRoleAssignmentChanges(fromDate, toDate),
            TemporaryPermissions = await GetTemporaryPermissions(fromDate, toDate),
            SecurityViolations = await GetSecurityViolations(fromDate, toDate)
        };

        return report;
    }

    public async Task<List<PermissionAnomaly>> DetectPermissionAnomalies()
    {
        var anomalies = new List<PermissionAnomaly>();

        // Detect users with excessive permissions
        anomalies.AddRange(await DetectExcessivePermissions());

        // Detect dormant accounts with active permissions
        anomalies.AddRange(await DetectDormantAccountPermissions());

        // Detect permission escalation patterns
        anomalies.AddRange(await DetectPermissionEscalation());

        return anomalies;
    }
}
```

### Implementation Roadmap

#### Phase 1: Enhanced Permission Infrastructure (Week 1-2)
- Database schema updates for new permission tables
- Enhanced permission entities and configurations
- Basic permission resolution service
- Permission caching infrastructure

#### Phase 2: Role Hierarchy and Inheritance (Week 3-4)
- Role hierarchy implementation
- Permission inheritance logic
- Enhanced ModulePermissionMap with hierarchy support
- Role management API updates

#### Phase 3: Dynamic and Contextual Permissions (Week 5-6)
- Temporary permission system
- Contextual permission implementation
- Location-based access control
- Project-based permission system

#### Phase 4: Performance and Security (Week 7-8)
- Multi-level caching implementation
- Permission precomputation service
- Security audit and compliance features
- Performance optimization and testing

### Conclusion

This enhanced Permission Management System design provides a robust, scalable, and secure foundation for managing user access in the Harmoni360 HSSE application. The system addresses enterprise requirements while maintaining compatibility with the existing architecture and providing clear migration paths for implementation.
