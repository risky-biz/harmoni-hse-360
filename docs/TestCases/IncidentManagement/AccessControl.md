# Access Control - Manual Test Cases

## Test Case Overview
This document contains comprehensive test cases for role-based access control and security in the Harmoni360 incident management system.

---

### Test Case ID: INC-ACCESS-001
**Test Objective:** Verify Employee role permissions for incident management
**Priority:** High
**Prerequisites:** 
- Employee user account with basic incident permissions
- Existing incidents in system (some created by employee, some by others)

**Test Data:**
- Employee user: john.employee@company.com
- Incidents created by this employee
- Incidents created by other users

**Test Steps:**
1. Login as Employee user
2. Navigate to incident list page
3. Verify only own incidents are visible
4. Attempt to view incident created by another user
5. Try to create new incident
6. Attempt to edit own incident
7. Attempt to edit incident created by another user
8. Try to delete own incident
9. Attempt to assign investigator to incident

**Expected Results:**
- Employee can view only their own incidents
- Employee can create new incidents
- Employee can edit their own incidents (basic details only)
- Employee cannot edit incidents created by others
- Employee cannot delete incidents
- Employee cannot assign investigators
- Appropriate error messages for unauthorized actions

**Post-conditions:** Employee role permissions properly enforced

---

### Test Case ID: INC-ACCESS-002
**Test Objective:** Verify HSE Manager role permissions for incident management
**Priority:** High
**Prerequisites:** 
- HSE Manager user account
- Various incidents in different statuses
- Available investigator accounts

**Test Data:**
- HSE Manager user: sarah.manager@company.com
- Incidents in various statuses and severities

**Test Steps:**
1. Login as HSE Manager
2. Navigate to incident list
3. Verify can view all incidents
4. Open any incident for editing
5. Modify incident details and save
6. Assign investigator to incident
7. Change incident status
8. Add corrective actions
9. Close incident
10. Delete incident (if permitted)

**Expected Results:**
- HSE Manager can view all incidents
- HSE Manager can edit any incident
- HSE Manager can assign investigators
- HSE Manager can manage workflow status
- HSE Manager can add corrective actions
- HSE Manager can close incidents
- Delete permissions may be restricted

**Post-conditions:** HSE Manager role permissions working correctly

---

### Test Case ID: INC-ACCESS-003
**Test Objective:** Verify Admin role permissions for incident management
**Priority:** High
**Prerequisites:** 
- Admin user account
- Various incidents and system data

**Test Data:**
- Admin user: admin@company.com
- Complete incident dataset

**Test Steps:**
1. Login as Admin user
2. Verify full access to all incidents
3. Test all incident management functions
4. Access system configuration settings
5. Manage user roles and permissions
6. Override workflow restrictions (if applicable)
7. Access audit logs and reports
8. Perform bulk operations (if available)

**Expected Results:**
- Admin has full access to all incidents
- Admin can perform all incident operations
- Admin can access system configuration
- Admin can manage user permissions
- Admin can override business rules (if designed)
- Admin has access to all audit information

**Post-conditions:** Admin role has complete system access

---

### Test Case ID: INC-ACCESS-004
**Test Objective:** Verify investigator assignment and access permissions
**Priority:** High
**Prerequisites:** 
- HSE Manager account for assignment
- Investigator user account
- Incident requiring investigation

**Test Steps:**
1. Login as HSE Manager
2. Assign investigator to incident
3. Logout and login as assigned investigator
4. Verify incident appears in investigator's queue
5. Test investigator's ability to update incident
6. Add investigation notes and evidence
7. Change incident status to "Awaiting Action"
8. Attempt to access non-assigned incidents

**Expected Results:**
- Assigned incidents appear in investigator's queue
- Investigator can update assigned incidents
- Investigator can add investigation data
- Investigator can progress incident status
- Investigator cannot access non-assigned incidents

**Post-conditions:** Investigator permissions working correctly

---

### Test Case ID: INC-ACCESS-005
**Test Objective:** Verify unauthorized access prevention and error handling
**Priority:** High
**Prerequisites:** 
- Multiple user accounts with different roles
- Direct URL access attempts

**Test Steps:**
1. Login as Employee
2. Attempt direct URL access to admin functions
3. Try to access incident edit page for unauthorized incident
4. Attempt API calls for unauthorized operations
5. Test session timeout handling
6. Try accessing system after logout

**Expected Results:**
- Direct URL access to unauthorized pages is blocked
- Appropriate error messages displayed (403 Forbidden)
- API calls return proper error codes
- Session timeout redirects to login
- Logged out users cannot access protected resources

**Post-conditions:** Unauthorized access properly prevented

---

### Test Case ID: INC-ACCESS-006
**Test Objective:** Verify department-based access control (if implemented)
**Priority:** Medium
**Prerequisites:** 
- Users from different departments
- Department-based access configuration

**Test Steps:**
1. Login as user from Department A
2. Verify access to incidents from same department
3. Attempt to access incidents from Department B
4. Test cross-department incident visibility rules
5. Verify department filtering in incident lists

**Expected Results:**
- Department-based access rules are enforced
- Users see appropriate incidents based on department
- Cross-department access follows business rules
- Department filters work correctly

**Post-conditions:** Department-based access control working (if implemented)

---

### Test Case ID: INC-ACCESS-007
**Test Objective:** Verify attachment access control and permissions
**Priority:** High
**Prerequisites:** 
- Incidents with file attachments
- Users with different access levels

**Test Steps:**
1. Login as Employee (incident creator)
2. Verify can view/download own incident attachments
3. Attempt to access attachments from other incidents
4. Login as HSE Manager
5. Verify can access all incident attachments
6. Test attachment upload permissions
7. Test attachment deletion permissions

**Expected Results:**
- Attachment access follows incident access rules
- Employees can access attachments for their incidents
- HSE Managers can access all attachments
- Upload/delete permissions match incident edit permissions
- Unauthorized attachment access is blocked

**Post-conditions:** Attachment access control working correctly

---

### Test Case ID: INC-ACCESS-008
**Test Objective:** Verify API access control and authentication
**Priority:** High
**Prerequisites:** 
- API endpoints for incident management
- Different user tokens/credentials

**Test Steps:**
1. Test API access without authentication token
2. Test API access with invalid token
3. Test API access with expired token
4. Test API access with valid Employee token
5. Test API access with HSE Manager token
6. Test API access with Admin token
7. Verify API returns appropriate data based on user role

**Expected Results:**
- Unauthenticated API calls are rejected (401)
- Invalid tokens are rejected
- Expired tokens are rejected
- API returns data appropriate to user role
- API enforces same access rules as web interface

**Post-conditions:** API access control working correctly

---

### Test Case ID: INC-ACCESS-009
**Test Objective:** Verify password security and account lockout policies
**Priority:** Medium
**Prerequisites:** 
- User accounts for testing
- Password policy configuration

**Test Steps:**
1. Attempt login with incorrect password multiple times
2. Verify account lockout after failed attempts
3. Test password complexity requirements
4. Test password change functionality
5. Verify password history restrictions
6. Test account unlock procedures

**Expected Results:**
- Account locks after configured failed attempts
- Password complexity rules are enforced
- Password changes work correctly
- Password history prevents reuse
- Account unlock procedures work

**Post-conditions:** Password security policies properly enforced

---

### Test Case ID: INC-ACCESS-010
**Test Objective:** Verify session management and concurrent login handling
**Priority:** Medium
**Prerequisites:** 
- User account for testing
- Multiple browser sessions

**Test Steps:**
1. Login to system in Browser A
2. Login with same account in Browser B
3. Verify session handling (concurrent vs. single session)
4. Test session timeout in both browsers
5. Logout from one browser and test other session
6. Test session persistence across browser restarts

**Expected Results:**
- Session policy is enforced consistently
- Concurrent sessions handled per business rules
- Session timeouts work correctly
- Logout affects appropriate sessions
- Session persistence works as designed

**Post-conditions:** Session management working correctly

---

### Test Case ID: INC-ACCESS-011
**Test Objective:** Verify audit logging for access control events
**Priority:** Medium
**Prerequisites:** 
- Audit logging system enabled
- Various user accounts for testing

**Test Steps:**
1. Perform various login/logout activities
2. Attempt unauthorized access to incidents
3. Perform incident operations with different roles
4. Review audit logs for access events
5. Verify log entries contain required information

**Expected Results:**
- All access events are logged
- Unauthorized access attempts are recorded
- Log entries contain user, timestamp, action details
- Logs are tamper-evident and secure
- Log retention policies are followed

**Post-conditions:** Access control audit logging working correctly

---

### Test Case ID: INC-ACCESS-012
**Test Objective:** Verify role change impact and permission updates
**Priority:** Medium
**Prerequisites:** 
- Admin account for role management
- Test user account

**Test Steps:**
1. Login as test user with Employee role
2. Verify current permissions and access
3. Have admin change user role to HSE Manager
4. Logout and login again
5. Verify new permissions are active
6. Test access to previously restricted functions
7. Change role back to Employee and verify restrictions return

**Expected Results:**
- Role changes take effect immediately or after re-login
- New permissions are properly applied
- Previously restricted access becomes available
- Role downgrades properly restrict access
- No permission caching issues occur

**Post-conditions:** Role changes properly update user permissions
