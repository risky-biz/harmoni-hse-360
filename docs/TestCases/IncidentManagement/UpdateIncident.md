# Update Incident - Manual Test Cases

## Test Case Overview
This document contains comprehensive test cases for incident update and modification functionality in the Harmoni360 system.

---

### Test Case ID: INC-UPDATE-001
**Test Objective:** Verify successful update of incident basic details
**Priority:** High
**Prerequisites:** 
- User logged in with incident update permissions
- Existing incident available for modification
- User has appropriate role (HSE Manager or Admin)

**Test Data:**
- Original Title: "Equipment malfunction"
- Updated Title: "Forklift hydraulic system failure - resolved"
- Original Description: "Equipment not working"
- Updated Description: "Forklift hydraulic system failed during operation. Maintenance team replaced faulty hydraulic pump. Equipment tested and returned to service."

**Test Steps:**
1. Navigate to incident list page
2. Select an existing incident to edit
3. Click "Edit" or "Update" button
4. Modify the incident title
5. Update the description with more detailed information
6. Click "Save Changes" button
7. Verify updated information displays correctly

**Expected Results:**
- Edit form loads with current incident data
- All fields are editable and accept new input
- Changes save successfully without errors
- Updated information displays in incident details
- Last modified timestamp updates
- Audit trail records the changes

**Post-conditions:** Incident updated with new information and audit trail entry created

---

### Test Case ID: INC-UPDATE-002
**Test Objective:** Verify incident status transition workflow
**Priority:** High
**Prerequisites:** 
- User with investigation permissions (HSE Manager or Admin)
- Incident in "Reported" status

**Test Data:**
- Incident ID: [Existing incident]
- Status transitions: Reported → Under Investigation → Awaiting Action → Resolved → Closed

**Test Steps:**
1. Open incident in "Reported" status
2. Change status to "Under Investigation"
3. Assign an investigator (if required)
4. Save changes and verify status update
5. Add investigation notes
6. Change status to "Awaiting Action"
7. Add corrective actions (if required)
8. Change status to "Resolved"
9. Add resolution notes
10. Change status to "Closed"
11. Add closure notes

**Expected Results:**
- Each status transition is allowed and processes correctly
- Appropriate fields become required at each stage
- Status history is maintained
- Notifications sent to relevant stakeholders
- Workflow rules are enforced (e.g., cannot skip required steps)

**Post-conditions:** Incident progressed through complete workflow with proper documentation

---

### Test Case ID: INC-UPDATE-003
**Test Objective:** Verify investigator assignment functionality
**Priority:** High
**Prerequisites:** 
- User with investigator assignment permissions
- Available investigators in the system
- Incident in appropriate status for assignment

**Test Data:**
- Incident requiring investigation
- Available investigator user accounts

**Test Steps:**
1. Open incident details page
2. Click "Assign Investigator" button
3. Select investigator from dropdown list
4. Add assignment notes (if available)
5. Save the assignment
6. Verify investigator receives notification
7. Login as assigned investigator
8. Verify incident appears in investigator's queue

**Expected Results:**
- Investigator dropdown shows available users with investigation permissions
- Assignment saves successfully
- Incident status automatically changes to "Under Investigation"
- Investigator receives email/system notification
- Incident appears in investigator's assigned incidents list

**Post-conditions:** Incident assigned to investigator with proper notifications sent

---

### Test Case ID: INC-UPDATE-004
**Test Objective:** Verify severity level modification and impact
**Priority:** Medium
**Prerequisites:** 
- User with incident modification permissions
- Existing incident with current severity level

**Test Data:**
- Incident with "Minor" severity to be upgraded to "Critical"
- Incident with "Critical" severity to be downgraded to "Moderate"

**Test Steps:**
1. Open incident with "Minor" severity
2. Change severity to "Critical"
3. Add justification for severity change
4. Save changes
5. Verify escalation procedures trigger
6. Open incident with "Critical" severity
7. Change severity to "Moderate"
8. Add justification for downgrade
9. Save changes

**Expected Results:**
- Severity changes are accepted and saved
- Upgrading to Critical triggers immediate notifications
- Downgrading requires appropriate justification
- Audit trail captures severity changes with reasons
- Appropriate stakeholders are notified of changes

**Post-conditions:** Incident severity updated with proper escalation handling

---

### Test Case ID: INC-UPDATE-005
**Test Objective:** Verify location and GPS coordinate updates
**Priority:** Medium
**Prerequisites:** 
- User with update permissions
- Incident with existing location data

**Test Data:**
- Original Location: "Building A - Floor 1"
- Updated Location: "Building A - Floor 1 - Conference Room 101"
- GPS Coordinates: Updated coordinates if location changed

**Test Steps:**
1. Open incident for editing
2. Modify the location description
3. Update GPS coordinates (if applicable)
4. Verify location on map display (if available)
5. Save the changes
6. Verify updated location displays correctly

**Expected Results:**
- Location field accepts updated information
- GPS coordinates update correctly
- Map display reflects new location (if implemented)
- Location history is maintained
- Changes save without errors

**Post-conditions:** Incident location updated with accurate information

---

### Test Case ID: INC-UPDATE-006
**Test Objective:** Verify witness information and immediate actions updates
**Priority:** Medium
**Prerequisites:** 
- User with update permissions
- Incident with existing witness/action data

**Test Data:**
- Additional witness: "Michael Brown - Security Guard"
- Updated immediate actions: "Additional safety measures implemented, area cordoned off"

**Test Steps:**
1. Open incident for editing
2. Add additional witness information
3. Update immediate actions taken
4. Save the changes
5. Verify all witness information is preserved
6. Verify updated actions display correctly

**Expected Results:**
- Witness information field accepts additional data
- Immediate actions field allows updates
- All previous information is preserved
- Updated information displays correctly
- Changes are saved successfully

**Post-conditions:** Incident updated with complete witness and action information

---

### Test Case ID: INC-UPDATE-007
**Test Objective:** Verify concurrent update handling (multiple users editing)
**Priority:** Medium
**Prerequisites:** 
- Two users with update permissions
- Same incident accessible to both users

**Test Steps:**
1. User A opens incident for editing
2. User B opens same incident for editing
3. User A makes changes and saves
4. User B attempts to save different changes
5. Verify system handles concurrent updates appropriately

**Expected Results:**
- System detects concurrent editing
- Appropriate warning/error message displayed
- Data integrity is maintained
- Last save wins OR conflict resolution mechanism provided
- No data corruption occurs

**Post-conditions:** Incident data integrity maintained despite concurrent access

---

### Test Case ID: INC-UPDATE-008
**Test Objective:** Verify update permissions and access control
**Priority:** High
**Prerequisites:** 
- Users with different role levels (Employee, HSE Manager, Admin)
- Existing incidents created by different users

**Test Steps:**
1. Login as Employee user
2. Attempt to edit own incident
3. Attempt to edit incident created by another user
4. Login as HSE Manager
5. Attempt to edit any incident
6. Verify permission restrictions are enforced

**Expected Results:**
- Employees can edit only their own incidents
- HSE Managers can edit any incident
- Admins have full edit access
- Appropriate error messages for unauthorized access
- Edit buttons/options hidden for unauthorized users

**Post-conditions:** Access control properly enforced for incident updates

---

### Test Case ID: INC-UPDATE-009
**Test Objective:** Verify incident closure with corrective actions validation
**Priority:** High
**Prerequisites:** 
- Incident with pending corrective actions
- User with closure permissions

**Test Steps:**
1. Open incident with pending corrective actions
2. Attempt to close incident without completing actions
3. Verify system prevents closure
4. Complete all corrective actions
5. Attempt to close incident again
6. Add closure notes
7. Confirm incident closure

**Expected Results:**
- System prevents closure with pending actions
- Clear error message explains closure requirements
- Closure allowed only after all actions completed
- Closure notes are required
- Incident status changes to "Closed"

**Post-conditions:** Incident properly closed with all requirements met

---

### Test Case ID: INC-UPDATE-010
**Test Objective:** Verify update validation and error handling
**Priority:** Medium
**Prerequisites:** User with update permissions

**Test Steps:**
1. Open incident for editing
2. Clear required fields (title, description)
3. Attempt to save changes
4. Enter invalid data (e.g., future incident date)
5. Attempt to save with invalid data
6. Enter data exceeding field limits
7. Verify all validation errors are handled

**Expected Results:**
- Required field validation prevents save
- Clear error messages for each validation failure
- Invalid data is rejected with appropriate messages
- Field length limits are enforced
- Form highlights problematic fields

**Post-conditions:** Data validation properly enforced during updates
