# RBAC Quick Reference Guide

## Role Overview

| Role | Code | Level | Primary Function | Key Modules |
|------|------|-------|------------------|-------------|
| SuperAdmin | `SA` | 1 | System administration | All (full access) |
| Developer | `DEV` | 2 | Technical support | All (admin access) |
| Admin | `ADM` | 3 | General administration | User Mgmt, Settings |
| IncidentManager | `IM` | 4 | Incident response | Incident Mgmt |
| RiskManager | `RM` | 4 | Risk assessment | Risk Mgmt |
| PPEManager | `PM` | 4 | Equipment management | PPE Mgmt |
| HealthMonitor | `HM` | 4 | Health oversight | Health Monitoring |
| Reporter | `REP` | 5 | Data collection | Limited reporting |
| Viewer | `VIEW` | 6 | Read-only access | Dashboard only |

## Module Access Matrix

### Legend
- ✅ Full Access (CRUD + Configure)
- 🔵 Manage Access (CRUD)
- 🟡 Limited Access (Read + Create)
- 🟠 Read Only
- ❌ No Access

| Module | SA | DEV | ADM | IM | RM | PM | HM | REP | VIEW |
|--------|:--:|:---:|:---:|:--:|:--:|:--:|:--:|:---:|:----:|
| **Dashboard** | ✅ | ✅ | ✅ | 🟠 | 🟠 | 🟠 | 🟠 | 🟠 | 🟠 |
| **Incident Management** | ✅ | ✅ | 🔵 | ✅ | 🟡 | ❌ | ❌ | 🟡 | ❌ |
| **Risk Management** | ✅ | ✅ | 🔵 | 🟡 | ✅ | ❌ | ❌ | 🟡 | ❌ |
| **PPE Management** | ✅ | ✅ | 🔵 | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| **Health Monitoring** | ✅ | ✅ | 🔵 | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| **Reporting** | ✅ | ✅ | 🔵 | 🟠 | 🟠 | 🟠 | 🟠 | 🟠 | 🟠 |
| **User Management** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Application Settings** | ✅ | ✅ | 🔵 | 🟠 | 🟠 | 🟠 | 🟠 | 🟠 | 🟠 |

## Permission Codes

| Permission | Code | Description | Access Level |
|------------|------|-------------|--------------|
| Read | `R` | View data | 1 (Basic) |
| Create | `C` | Add new records | 2 (Standard) |
| Update | `U` | Modify existing data | 3 (Standard) |
| Delete | `D` | Remove records | 4 (Elevated) |
| Export | `E` | Data extraction | 4 (Elevated) |
| Configure | `CF` | System administration | 5 (Admin) |
| Approve | `A` | Workflow authorization | 5 (Admin) |
| Assign | `AS` | Resource allocation | 5 (Admin) |

## Quick Permission Lookup

### By Role Type

#### SuperAdmin / Developer
```
✅ ALL PERMISSIONS ON ALL MODULES
- Complete system control
- All CRUD operations
- System configuration
- User management
```

#### Admin
```
Dashboard: R, CF
IncidentManagement: R, C, U, D, E, A
RiskManagement: R, C, U, D, E, A  
PPEManagement: R, C, U, D, E, AS
HealthMonitoring: R, C, U, D, E
Reporting: R, E, CF
UserManagement: R, C, U, D, AS
ApplicationSettings: R, U, CF
```

#### Specialized Managers (IM/RM/PM/HM)
```
Dashboard: R
[Specialized Module]: R, C, U, D, E, CF, A, AS
[Other Incident/Risk]: R, C (reporting only)
Reporting: R, E
ApplicationSettings: R
```

#### Reporter
```
Dashboard: R
IncidentManagement: R, C (own reports)
RiskManagement: R, C (hazard reporting)  
Reporting: R
ApplicationSettings: R
```

#### Viewer
```
Dashboard: R
Reporting: R (limited)
ApplicationSettings: R
```

## Common Authorization Patterns

### Controller Attributes
```csharp
// Read access
[RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]

// Create access  
[RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Create)]

// Admin operations
[RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Configure)]

// Multiple roles (fallback)
[RequireRoles(RoleType.SuperAdmin, RoleType.Developer)]
```

### Frontend Checks
```typescript
// Permission checking
hasModulePermission(userRoles, 'IncidentManagement', 'Create')

// Role checking
hasRole(userRoles, 'IncidentManager')

// UI conditional rendering
{canCreate && <CreateButton />}
{canDelete && <DeleteButton />}
```

## Hierarchy Rules

### Role Inheritance
- **Higher roles inherit lower role permissions**
- **SuperAdmin**: All permissions
- **Developer**: All non-SuperAdmin permissions  
- **Admin**: Administrative permissions across modules
- **Managers**: Full access to specialized modules
- **Reporter**: Limited create/read permissions
- **Viewer**: Read-only access

### Permission Levels
1. **Read** (Everyone with module access)
2. **Create** (Reporters and above)
3. **Update** (Managers and above)
4. **Delete** (Managers and above)
5. **Configure** (Admins and above)

## Emergency Access

### Break-Glass Procedures
```csharp
// Emergency SuperAdmin access
[RequireRoles(RoleType.SuperAdmin)]
[EmergencyAccess] // Custom attribute for auditing
public async Task<IActionResult> EmergencyOverride()
```

### Temporary Role Elevation
```sql
-- Grant temporary permissions (with expiration)
INSERT INTO RoleModulePermissions (RoleId, ModulePermissionId, IsActive, GrantedAt, ExpiresAt)
VALUES (@RoleId, @PermissionId, true, NOW(), NOW() + INTERVAL '24 HOURS');
```

## Troubleshooting

### 403 Forbidden
1. Check user's role assignments
2. Verify module permission exists
3. Confirm role hierarchy level
4. Review permission matrix

### Missing Features
1. Check user's role level
2. Verify module access
3. Confirm feature requires appropriate permission
4. Review UI conditional rendering

### Performance Issues
1. Check permission caching
2. Review database indexes
3. Optimize permission queries
4. Consider bulk permission checks

## Implementation Checklist

### New Feature Development
- [ ] Define required permissions
- [ ] Add authorization attributes
- [ ] Update permission matrix
- [ ] Implement frontend checks
- [ ] Test with different roles
- [ ] Document access requirements

### Role Assignment
- [ ] Identify user responsibilities
- [ ] Select appropriate role level
- [ ] Verify module access needs
- [ ] Assign role in system
- [ ] Test user access
- [ ] Document assignment reasoning

### Security Review
- [ ] Audit role assignments
- [ ] Review permission grants
- [ ] Check for over-privileged accounts
- [ ] Validate role segregation
- [ ] Document exceptions
- [ ] Plan regular reviews