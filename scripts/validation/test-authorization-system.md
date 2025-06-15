# Authorization System Testing & Validation Guide

## Phase 5: Database Migration and Testing Validation

### Migration Status Check

**Expected State After Migration:**
- ✅ `ModulePermissions` table created with 64 records (8 modules × 8 permissions)
- ✅ `RoleModulePermissions` table created with appropriate role-permission mappings
- ✅ `Roles` table updated with `RoleType`, `IsActive`, `DisplayOrder` columns
- ✅ 9 demo users created for all role types

### 1. Database Validation Commands

```sql
-- Check if migration was applied
SELECT "MigrationId" FROM "__EFMigrationsHistory" 
WHERE "MigrationId" = '20250606143400_AddModuleBasedAuthorization';

-- Verify ModulePermissions table
SELECT COUNT(*) as total_module_permissions FROM "ModulePermissions";
SELECT "Module", "Permission", "Name" FROM "ModulePermissions" ORDER BY "Module", "Permission";

-- Verify RoleModulePermissions table
SELECT COUNT(*) as total_role_permissions FROM "RoleModulePermissions";

-- Check Role updates
SELECT "Id", "Name", "RoleType", "IsActive", "DisplayOrder" FROM "Roles" ORDER BY "DisplayOrder";

-- Verify user roles
SELECT u."Name", u."Email", r."Name" as RoleName, r."RoleType" 
FROM "Users" u
JOIN "UserRoles" ur ON u."Id" = ur."UserId"
JOIN "Roles" r ON ur."RoleId" = r."Id"
ORDER BY r."RoleType";
```

### 2. Demo User Testing Matrix

| Username | Email | Role | Expected Modules | Test Login |
|----------|-------|------|------------------|------------|
| SuperAdmin | superadmin@harmoni360.com | SuperAdmin | ALL + Settings | ✅ |
| Developer | developer@harmoni360.com | Developer | ALL + Settings | ✅ |
| Admin | admin@harmoni360.com | Admin | ALL Functional | ✅ |
| IncidentMgr | incident.manager@harmoni360.com | IncidentManager | Incident Only | ✅ |
| RiskMgr | risk.manager@harmoni360.com | RiskManager | Risk Only | ✅ |
| PPEMgr | ppe.manager@harmoni360.com | PPEManager | PPE Only | ✅ |
| HealthMon | health.monitor@harmoni360.com | HealthMonitor | Health Only | ✅ |
| Reporter | reporter@harmoni360.com | Reporter | Read-Only + Reports | ✅ |
| Viewer | viewer@harmoni360.com | Viewer | Dashboard Only | ✅ |

### 3. Module Access Testing

#### SuperAdmin/Developer Access Test
**Expected:** Full access to all modules
- ✅ Dashboard (Read)
- ✅ Incident Management (Full CRUD)
- ✅ Risk Management (Full CRUD)
- ✅ PPE Management (Full CRUD)
- ✅ Health Monitoring (Full CRUD)
- ✅ Reporting (Full CRUD + Export)
- ✅ User Management (Full CRUD)
- ✅ Application Settings (Configure)

#### Admin Access Test
**Expected:** All functional modules, no system settings
- ✅ Dashboard (Read)
- ✅ Incident Management (Full CRUD)
- ✅ Risk Management (Full CRUD)
- ✅ PPE Management (Full CRUD)
- ✅ Health Monitoring (Full CRUD)
- ✅ Reporting (Full CRUD + Export)
- ❌ User Management (No Access)
- ❌ Application Settings (No Access)

#### IncidentManager Access Test
**Expected:** Only Incident Management module
- ✅ Dashboard (Read)
- ✅ Incident Management (Full CRUD + Export)
- ❌ Risk Management (No Access)
- ❌ PPE Management (No Access)
- ❌ Health Monitoring (No Access)
- ✅ Reporting (Read + Export for incidents)
- ❌ User Management (No Access)
- ❌ Application Settings (No Access)

#### PPEManager Access Test
**Expected:** Only PPE Management module
- ✅ Dashboard (Read)
- ❌ Incident Management (No Access)
- ❌ Risk Management (No Access)
- ✅ PPE Management (Full CRUD + Export + Configure)
- ❌ Health Monitoring (No Access)
- ✅ Reporting (Read + Export for PPE)
- ❌ User Management (No Access)
- ❌ Application Settings (No Access)

#### Viewer Access Test
**Expected:** Dashboard only
- ✅ Dashboard (Read)
- ❌ Incident Management (No Access)
- ❌ Risk Management (No Access)
- ❌ PPE Management (No Access)
- ❌ Health Monitoring (No Access)
- ❌ Reporting (No Access)
- ❌ User Management (No Access)
- ❌ Application Settings (No Access)

### 4. API Endpoint Testing

#### Test Cases for HTTP Status Codes

```bash
# Test SuperAdmin access (should get 200)
curl -H "Authorization: Bearer <superadmin_token>" \
  http://localhost:5000/api/incidents

# Test Viewer access to incidents (should get 403)
curl -H "Authorization: Bearer <viewer_token>" \
  http://localhost:5000/api/incidents

# Test IncidentManager access to PPE (should get 403)
curl -H "Authorization: Bearer <incident_manager_token>" \
  http://localhost:5000/api/ppe

# Test unauthorized access (should get 401)
curl http://localhost:5000/api/incidents
```

### 5. Frontend UI Testing

#### Navigation Menu Testing
1. **Login as SuperAdmin**: Should see all menu items
2. **Login as PPEManager**: Should only see Dashboard, PPE sections
3. **Login as Viewer**: Should only see Dashboard
4. **Login as Admin**: Should see all functional modules, no admin sections

#### Button Visibility Testing
1. **Create Incident Button**: 
   - ✅ Visible for: SuperAdmin, Developer, Admin, IncidentManager
   - ❌ Hidden for: RiskManager, PPEManager, HealthMonitor, Reporter, Viewer

2. **PPE Management Button**:
   - ✅ Visible for: SuperAdmin, Developer, Admin, PPEManager
   - ❌ Hidden for: IncidentManager, RiskManager, HealthMonitor, Reporter, Viewer

3. **System Settings**:
   - ✅ Visible for: SuperAdmin, Developer
   - ❌ Hidden for: All other roles

### 6. Performance Testing

#### Authorization Overhead Measurement
```sql
-- Measure query performance for permission checking
EXPLAIN ANALYZE 
SELECT u."Id", u."Name", 
       array_agg(CONCAT(mp."Module", '.', mp."Permission")) as permissions
FROM "Users" u
JOIN "UserRoles" ur ON u."Id" = ur."UserId"
JOIN "Roles" r ON ur."RoleId" = r."Id"
JOIN "RoleModulePermissions" rmp ON r."Id" = rmp."RoleId"
JOIN "ModulePermissions" mp ON rmp."ModulePermissionId" = mp."Id"
WHERE u."Id" = 1 AND rmp."IsActive" = true AND mp."IsActive" = true
GROUP BY u."Id", u."Name";
```

**Expected Performance:**
- ✅ Permission query < 10ms
- ✅ Login with permissions < 200ms
- ✅ Page load with authorization < 50ms additional overhead

### 7. Security Validation

#### Privilege Escalation Tests
1. **Role Modification**: Ensure users cannot modify their own roles
2. **Permission Bypass**: Verify no API endpoints skip authorization
3. **JWT Token**: Validate tokens include correct role/permission claims
4. **Session Security**: Ensure permissions are re-validated on each request

#### Cross-Module Access Tests
1. **PPEManager → Incidents**: Should be denied
2. **IncidentManager → Health**: Should be denied  
3. **Reporter → Administrative functions**: Should be denied
4. **Viewer → Any CRUD operations**: Should be denied

### 8. Expected Test Results

#### Database State After Seeding
- **Roles**: 9 roles with proper hierarchy
- **ModulePermissions**: 64 permission records
- **RoleModulePermissions**: ~200+ mappings
- **Users**: 9 demo users with appropriate role assignments

#### Frontend Behavior
- **Dynamic Navigation**: Menus filtered by user permissions
- **Conditional Rendering**: Buttons/actions based on permissions
- **Error Handling**: Clear 403 messages for unauthorized access
- **Route Protection**: Redirect to unauthorized page when needed

#### API Security
- **Proper HTTP Status Codes**: 401 for authentication, 403 for authorization
- **Consistent Authorization**: All endpoints properly secured
- **Performance**: Minimal overhead from authorization checks

### 9. Manual Testing Checklist

- [ ] All 9 demo users can log in successfully
- [ ] SuperAdmin sees all modules and administrative features
- [ ] Specialized managers only see their assigned modules
- [ ] Viewer only sees dashboard with read-only access
- [ ] Navigation menus are filtered correctly per role
- [ ] Create/Edit/Delete buttons are hidden appropriately
- [ ] API calls return proper 403 errors for unauthorized access
- [ ] System maintains good performance under authorization load

### 10. Validation Scripts

```bash
#!/bin/bash
# Run this script to validate the authorization system

echo "Testing Authorization System..."

# Test 1: Database structure
echo "1. Checking database structure..."
# Add database connection and validation queries

# Test 2: Demo user login
echo "2. Testing demo user authentication..."
# Add authentication API calls

# Test 3: Permission enforcement
echo "3. Testing permission enforcement..."
# Add API permission tests

# Test 4: UI authorization
echo "4. Testing UI authorization..."
# Add UI automation tests

echo "Authorization system validation completed."
```

This comprehensive testing plan validates that our module-based authorization system is working correctly across all layers of the application.