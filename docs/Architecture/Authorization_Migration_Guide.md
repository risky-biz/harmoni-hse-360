# Authorization System Migration Guide

## Overview

This guide provides step-by-step instructions for migrating from the legacy role-based authorization system to the new module-based authorization system in Harmoni360.

## Migration Timeline

### Phase 1: Database Schema Updates ✅ **COMPLETED**
- [x] Create new authorization entities
- [x] Add RoleType enum to existing roles
- [x] Create module permission tables
- [x] Implement role-module-permission relationships

### Phase 2: Authorization Framework ✅ **COMPLETED**
- [x] Implement custom authorization handlers
- [x] Create module permission attributes
- [x] Build permission mapping system
- [x] Add role hierarchy support

### Phase 3: Controller Updates ✅ **COMPLETED**
- [x] Replace legacy role-based attributes
- [x] Apply module-based permissions
- [x] Test authorization endpoints
- [x] Validate permission enforcement

### Phase 4: Data Migration 🔄 **IN PROGRESS**
- [ ] Create data seeding scripts
- [ ] Populate module permissions
- [ ] Assign role permissions
- [ ] Migrate existing user assignments

## Pre-Migration Checklist

### System Backup
```bash
# Backup database
pg_dump -h localhost -U postgres -d harmoni360 > backup_pre_migration.sql

# Backup application files
tar -czf app_backup_$(date +%Y%m%d).tar.gz /path/to/application
```

### Environment Verification
```bash
# Check .NET version
dotnet --version

# Verify Entity Framework tools
dotnet ef --version

# Test database connection
dotnet ef database update --dry-run
```

## Migration Steps

### Step 1: Apply Database Migration

```bash
# Navigate to infrastructure project
cd src/Harmoni360.Infrastructure

# Generate migration (if not exists)
dotnet ef migrations add AddModuleBasedAuthorization

# Apply migration to database
dotnet ef database update

# Verify migration success
dotnet ef migrations list
```

### Step 2: Seed Module Permissions

Create and run the seeding script:

```csharp
// File: Scripts/SeedModulePermissions.cs
public static class ModulePermissionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Create all possible module-permission combinations
        var modulePermissions = new List<ModulePermission>();
        
        foreach (var module in Enum.GetValues<ModuleType>())
        {
            foreach (var permission in Enum.GetValues<PermissionType>())
            {
                var modulePermission = ModulePermission.Create(
                    module,
                    permission,
                    $"{module}.{permission}",
                    $"Allows {permission} access to {module} module"
                );
                
                modulePermissions.Add(modulePermission);
            }
        }
        
        // Add to database if not exists
        foreach (var mp in modulePermissions)
        {
            var exists = await context.ModulePermissions
                .AnyAsync(x => x.Module == mp.Module && x.Permission == mp.Permission);
                
            if (!exists)
            {
                context.ModulePermissions.Add(mp);
            }
        }
        
        await context.SaveChangesAsync();
    }
}
```

Run seeding:
```bash
# Add seeding to Program.cs startup
# Or run as separate console application
dotnet run --project SeedingConsole
```

### Step 3: Update Existing Roles

```sql
-- Map existing roles to new RoleType enum values
UPDATE "Roles" SET "RoleType" = 1, "DisplayOrder" = 1 WHERE "Name" = 'SuperAdmin';
UPDATE "Roles" SET "RoleType" = 2, "DisplayOrder" = 2 WHERE "Name" = 'Developer';
UPDATE "Roles" SET "RoleType" = 3, "DisplayOrder" = 3 WHERE "Name" IN ('Admin', 'Administrator');
UPDATE "Roles" SET "RoleType" = 4, "DisplayOrder" = 4 WHERE "Name" IN ('IncidentManager', 'HSEManager');
UPDATE "Roles" SET "RoleType" = 5, "DisplayOrder" = 5 WHERE "Name" IN ('RiskManager', 'SafetyManager');
UPDATE "Roles" SET "RoleType" = 6, "DisplayOrder" = 6 WHERE "Name" IN ('PPEManager', 'EquipmentManager');
UPDATE "Roles" SET "RoleType" = 7, "DisplayOrder" = 7 WHERE "Name" IN ('HealthMonitor', 'Nurse', 'HealthManager');
UPDATE "Roles" SET "RoleType" = 8, "DisplayOrder" = 8 WHERE "Name" IN ('Reporter', 'Employee', 'Staff');
UPDATE "Roles" SET "RoleType" = 9, "DisplayOrder" = 9 WHERE "Name" IN ('Viewer', 'Guest', 'ReadOnly');

-- Activate all roles
UPDATE "Roles" SET "IsActive" = true WHERE "IsActive" IS NULL;
```

### Step 4: Assign Module Permissions to Roles

```csharp
// File: Scripts/AssignRolePermissions.cs
public static class RolePermissionSeeder
{
    public static async Task AssignPermissionsAsync(ApplicationDbContext context)
    {
        var rolePermissionMap = ModulePermissionMap.GetRoleModulePermissions();
        
        foreach (var (roleType, modulePermissions) in rolePermissionMap)
        {
            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleType == roleType);
                
            if (role == null) continue;
            
            foreach (var (module, permissions) in modulePermissions)
            {
                foreach (var permission in permissions)
                {
                    var modulePermission = await context.ModulePermissions
                        .FirstOrDefaultAsync(mp => 
                            mp.Module == module && mp.Permission == permission);
                            
                    if (modulePermission == null) continue;
                    
                    // Check if assignment already exists
                    var exists = await context.RoleModulePermissions
                        .AnyAsync(rmp => 
                            rmp.RoleId == role.Id && 
                            rmp.ModulePermissionId == modulePermission.Id);
                            
                    if (!exists)
                    {
                        var assignment = RoleModulePermission.Create(
                            role.Id,
                            modulePermission.Id,
                            null, // System assignment
                            "Initial migration assignment"
                        );
                        
                        context.RoleModulePermissions.Add(assignment);
                    }
                }
            }
        }
        
        await context.SaveChangesAsync();
    }
}
```

### Step 5: Validate Migration

```csharp
// File: Scripts/ValidationScript.cs
public static class MigrationValidator
{
    public static async Task ValidateAsync(ApplicationDbContext context)
    {
        var results = new ValidationResults();
        
        // 1. Check all roles have RoleType assigned
        var rolesWithoutType = await context.Roles
            .Where(r => r.RoleType == 0 || r.RoleType == null)
            .CountAsync();
            
        results.Add("Roles without RoleType", rolesWithoutType);
        
        // 2. Check all module permissions exist
        var expectedPermissions = Enum.GetValues<ModuleType>().Length * 
                                Enum.GetValues<PermissionType>().Length;
        var actualPermissions = await context.ModulePermissions.CountAsync();
        
        results.Add("Module permissions", $"{actualPermissions}/{expectedPermissions}");
        
        // 3. Check role-permission assignments
        foreach (var roleType in Enum.GetValues<RoleType>())
        {
            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleType == roleType);
                
            if (role != null)
            {
                var assignmentCount = await context.RoleModulePermissions
                    .Where(rmp => rmp.RoleId == role.Id && rmp.IsActive)
                    .CountAsync();
                    
                results.Add($"{roleType} permissions", assignmentCount);
            }
        }
        
        // 4. Test authorization for sample endpoints
        // ... implementation
        
        return results;
    }
}
```

## Frontend Migration

### Step 1: Update Permission Utilities

```typescript
// File: src/utils/permissions.ts
import { ModuleType, PermissionType, RoleType } from '@/types/auth';

// Legacy permission checking (keep for transition)
export const hasLegacyRole = (userRoles: string[], requiredRole: string): boolean => {
  return userRoles.includes(requiredRole);
};

// New module-based permission checking
export const hasModulePermission = (
  userRoles: string[],
  module: ModuleType,
  permission: PermissionType
): boolean => {
  return userRoles.some(role => {
    const roleType = role as RoleType;
    return modulePermissionMap[roleType]?.[module]?.includes(permission);
  });
};

// Hybrid checking during migration
export const hasPermission = (
  userRoles: string[],
  module: ModuleType,
  permission: PermissionType,
  fallbackRoles?: string[]
): boolean => {
  // Try new system first
  if (hasModulePermission(userRoles, module, permission)) {
    return true;
  }
  
  // Fallback to legacy system
  if (fallbackRoles) {
    return fallbackRoles.some(role => hasLegacyRole(userRoles, role));
  }
  
  return false;
};
```

### Step 2: Update Components Gradually

```typescript
// Before migration
const IncidentList = () => {
  const { userRoles } = useAuth();
  const canCreate = userRoles.includes('IncidentManager') || userRoles.includes('Admin');
  
  return (
    <div>
      {canCreate && <CreateButton />}
    </div>
  );
};

// During migration (hybrid approach)
const IncidentList = () => {
  const { userRoles } = useAuth();
  
  const canCreate = hasPermission(
    userRoles,
    ModuleType.IncidentManagement,
    PermissionType.Create,
    ['IncidentManager', 'Admin'] // Legacy fallback
  );
  
  return (
    <div>
      {canCreate && <CreateButton />}
    </div>
  );
};

// After migration (new system only)
const IncidentList = () => {
  const { userRoles } = useAuth();
  const canCreate = hasModulePermission(userRoles, ModuleType.IncidentManagement, PermissionType.Create);
  
  return (
    <div>
      {canCreate && <CreateButton />}
    </div>
  );
};
```

## Testing Strategy

### Unit Tests

```csharp
[Test]
public async Task ModulePermissionHandler_ValidRole_GrantsAccess()
{
    // Arrange
    var handler = new ModulePermissionHandler(_logger);
    var requirement = new ModulePermissionRequirement(
        ModuleType.IncidentManagement, 
        PermissionType.Read);
    
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, "123"),
        new Claim(ClaimTypes.Role, "IncidentManager")
    };
    
    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        new ClaimsPrincipal(new ClaimsIdentity(claims)),
        null);
    
    // Act
    await handler.HandleAsync(context);
    
    // Assert
    Assert.IsTrue(context.HasSucceeded);
}
```

### Integration Tests

```csharp
[Test]
public async Task IncidentController_CreateIncident_RequiresCreatePermission()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", _reporterToken); // Reporter role
    
    var incident = new CreateIncidentRequest { /* ... */ };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/incident", incident);
    
    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); // Reporter can create
}

[Test]
public async Task IncidentController_DeleteIncident_RequiresDeletePermission()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", _reporterToken); // Reporter role
    
    // Act
    var response = await client.DeleteAsync("/api/incident/1");
    
    // Assert
    Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode); // Reporter cannot delete
}
```

### End-to-End Tests

```typescript
describe('Authorization Migration', () => {
  it('should maintain access for existing users', async () => {
    // Login with existing user
    await loginAs('incident.manager@company.com');
    
    // Test access to incident management
    await visit('/incidents');
    expect(page.locator('[data-testid="create-incident"]')).toBeVisible();
    
    // Test restricted access
    await visit('/admin/users');
    expect(page.locator('[data-testid="access-denied"]')).toBeVisible();
  });
  
  it('should work with new permission system', async () => {
    // Create user with new role system
    const user = await createUserWithRole(RoleType.IncidentManager);
    await loginAs(user.email);
    
    // Test module-based permissions
    const hasCreatePermission = await evaluatePermission(
      ModuleType.IncidentManagement,
      PermissionType.Create
    );
    
    expect(hasCreatePermission).toBe(true);
  });
});
```

## Rollback Plan

### Emergency Rollback

If critical issues are discovered:

```sql
-- 1. Disable new authorization middleware
-- Update appsettings.json: "EnableModuleBasedAuth": false

-- 2. Revert to legacy role checking
-- Keep legacy [Authorize(Roles=...)] attributes as fallback

-- 3. Database rollback (if necessary)
-- Restore from backup
pg_restore -h localhost -U postgres -d harmoni360 backup_pre_migration.sql
```

### Gradual Rollback

```csharp
// Add feature flag for gradual rollback
public class AuthorizationOptions
{
    public bool UseModuleBasedAuth { get; set; } = true;
    public bool FallbackToLegacyAuth { get; set; } = true;
}

// In authorization handler
if (!_options.UseModuleBasedAuth && _options.FallbackToLegacyAuth)
{
    // Use legacy authorization logic
    return await LegacyAuthorizationHandler.HandleAsync(context, requirement);
}
```

## Post-Migration Tasks

### 1. Monitoring

```csharp
// Add authorization metrics
services.AddSingleton<IAuthorizationMetrics, AuthorizationMetrics>();

public class AuthorizationMetrics
{
    private readonly IMetricsLogger _logger;
    
    public void LogAuthorizationSuccess(string role, string module, string permission)
    {
        _logger.LogMetric("Authorization.Success", 1, new Dictionary<string, object>
        {
            ["Role"] = role,
            ["Module"] = module,
            ["Permission"] = permission
        });
    }
    
    public void LogAuthorizationFailure(string role, string module, string permission, string reason)
    {
        _logger.LogMetric("Authorization.Failure", 1, new Dictionary<string, object>
        {
            ["Role"] = role,
            ["Module"] = module,
            ["Permission"] = permission,
            ["Reason"] = reason
        });
    }
}
```

### 2. Documentation Updates

- [ ] Update user guides
- [ ] Create role assignment procedures
- [ ] Document troubleshooting steps
- [ ] Update API documentation

### 3. Training

- [ ] Train administrators on new role management
- [ ] Educate developers on new authorization patterns
- [ ] Update security procedures
- [ ] Create permission management guides

### 4. Cleanup

After migration stability is confirmed:

```sql
-- Remove legacy role-permission junction tables (if any)
DROP TABLE IF EXISTS "RolePermissions";

-- Remove unused permission columns
-- (Only after confirming new system works)
```

## Common Issues and Solutions

### Issue: Users Cannot Access Previously Available Features

**Symptoms:**
- 403 Forbidden errors
- Missing UI elements
- Workflow disruptions

**Diagnosis:**
```sql
-- Check user's role assignments
SELECT u."Email", r."Name", r."RoleType" 
FROM "Users" u
JOIN "UserRoles" ur ON u."Id" = ur."UserId"
JOIN "Roles" r ON ur."RoleId" = r."Id"
WHERE u."Email" = 'user@company.com';

-- Check role's permissions
SELECT r."Name", mp."Module", mp."Permission"
FROM "Roles" r
JOIN "RoleModulePermissions" rmp ON r."Id" = rmp."RoleId"
JOIN "ModulePermissions" mp ON rmp."ModulePermissionId" = mp."Id"
WHERE r."RoleType" = 4 AND rmp."IsActive" = true;
```

**Solutions:**
1. Assign appropriate role to user
2. Grant additional permissions to role
3. Check module permission mappings

### Issue: Performance Degradation

**Symptoms:**
- Slow page loads
- Authorization timeouts
- High database load

**Solutions:**
1. Enable permission caching
2. Optimize database queries
3. Add database indexes
4. Use bulk permission checks

### Issue: Authorization Inconsistencies

**Symptoms:**
- Different behavior across environments
- Intermittent access issues
- Conflicting permissions

**Solutions:**
1. Verify migration scripts ran completely
2. Check environment configuration
3. Validate data consistency
4. Review cache invalidation

## Success Criteria

The migration is considered successful when:

- [ ] All existing users maintain appropriate access levels
- [ ] No critical functionality is broken
- [ ] Performance is maintained or improved
- [ ] Security is enhanced with finer-grained control
- [ ] System passes all authorization tests
- [ ] Monitoring shows stable operation
- [ ] User feedback is positive
- [ ] Documentation is complete and accurate

## Support and Escalation

### During Migration

- **Technical Lead**: Authorization architecture questions
- **Database Admin**: Migration and rollback procedures  
- **Security Team**: Permission validation and compliance
- **Product Owner**: Business requirement clarification

### Post-Migration

- **Help Desk**: User access issues
- **System Admin**: Role assignment and management
- **Development Team**: Bug fixes and enhancements
- **Security Team**: Ongoing compliance and auditing