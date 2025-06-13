# Incident Workflow - Manual Test Cases

## Test Case Overview
This document contains comprehensive test cases for incident status workflow and lifecycle management in the Harmoni360 system.

---

### Test Case ID: INC-WORKFLOW-001
**Test Objective:** Verify complete incident lifecycle from creation to closure
**Priority:** High
**Prerequisites:** 
- Users with different roles (Employee, HSE Manager, Admin)
- Clean test environment

**Test Data:**
- New incident requiring full workflow processing
- Test users for each workflow stage

**Test Steps:**
1. **Creation Stage (Employee)**
   - Login as Employee
   - Create new incident with "Serious" severity
   - Verify initial status is "Reported"
   - Verify incident appears in pending queue

2. **Investigation Assignment (HSE Manager)**
   - Login as HSE Manager
   - Assign investigator to incident
   - Verify status changes to "Under Investigation"
   - Verify investigator receives notification

3. **Investigation Process (Investigator)**
   - Login as assigned investigator
   - Add investigation notes
   - Upload investigation evidence
   - Change status to "Awaiting Action"

4. **Corrective Actions (HSE Manager)**
   - Add corrective actions to incident
   - Assign action owners
   - Set due dates for actions

5. **Action Completion (Action Owners)**
   - Mark corrective actions as completed
   - Add completion evidence

6. **Resolution (HSE Manager)**
   - Verify all actions completed
   - Change status to "Resolved"
   - Add resolution summary

7. **Closure (HSE Manager/Admin)**
   - Add closure notes
   - Change status to "Closed"
   - Verify incident workflow complete

**Expected Results:**
- Each workflow stage processes correctly
- Status transitions follow defined rules
- Appropriate notifications sent at each stage
- All required information captured
- Audit trail maintains complete history

**Post-conditions:** Incident successfully processed through complete lifecycle

---

### Test Case ID: INC-WORKFLOW-002
**Test Objective:** Verify status transition validation and restrictions
**Priority:** High
**Prerequisites:** 
- User with workflow management permissions
- Incident in "Reported" status

**Test Steps:**
1. Attempt to change status directly from "Reported" to "Resolved" (skipping investigation)
2. Verify system prevents invalid status jump
3. Attempt to change status from "Under Investigation" back to "Reported"
4. Test each valid status transition
5. Test each invalid status transition

**Expected Results:**
- Invalid status transitions are blocked
- Clear error messages explain workflow requirements
- Valid transitions are allowed
- Status can only move forward in defined sequence
- Business rules are enforced consistently

**Post-conditions:** Workflow integrity maintained through validation

---

### Test Case ID: INC-WORKFLOW-003
**Test Objective:** Verify investigator assignment and notification process
**Priority:** High
**Prerequisites:** 
- HSE Manager user account
- Available investigators in system
- Incident in "Reported" status

**Test Data:**
- Incident requiring investigation
- Multiple available investigators

**Test Steps:**
1. Login as HSE Manager
2. Open incident in "Reported" status
3. Click "Assign Investigator" button
4. Select investigator from dropdown
5. Add assignment notes
6. Save assignment
7. Verify investigator notification sent
8. Login as assigned investigator
9. Verify incident appears in assigned queue

**Expected Results:**
- Investigator dropdown shows only qualified users
- Assignment saves successfully
- Status automatically changes to "Under Investigation"
- Email notification sent to investigator
- Incident appears in investigator's dashboard
- Assignment timestamp recorded

**Post-conditions:** Investigator successfully assigned with proper notifications

---

### Test Case ID: INC-WORKFLOW-004
**Test Objective:** Verify corrective action management within workflow
**Priority:** High
**Prerequisites:** 
- Incident in "Awaiting Action" status
- Users available for action assignment

**Test Steps:**
1. Open incident in "Awaiting Action" status
2. Add new corrective action
3. Assign action to specific user
4. Set due date for action
5. Save corrective action
6. Login as assigned user
7. Mark action as "In Progress"
8. Add progress notes
9. Mark action as "Completed"
10. Add completion evidence

**Expected Results:**
- Corrective actions can be added and assigned
- Action status tracking works correctly
- Assigned users receive notifications
- Progress updates are captured
- Completion requires evidence/notes
- Action history is maintained

**Post-conditions:** Corrective actions properly managed within incident workflow

---

### Test Case ID: INC-WORKFLOW-005
**Test Objective:** Verify incident closure validation with pending actions
**Priority:** High
**Prerequisites:** 
- Incident with pending corrective actions
- User with closure permissions

**Test Steps:**
1. Open incident with pending corrective actions
2. Attempt to change status to "Closed"
3. Verify system prevents closure
4. Complete all pending actions
5. Attempt closure again
6. Add closure notes
7. Confirm incident closure

**Expected Results:**
- System prevents closure with pending actions
- Clear error message explains closure requirements
- Closure allowed only after all actions completed
- Closure notes are required
- Final status change to "Closed" succeeds

**Post-conditions:** Incident closure properly validated and completed

---

### Test Case ID: INC-WORKFLOW-006
**Test Objective:** Verify workflow notifications and escalations
**Priority:** Medium
**Prerequisites:** 
- Email notification system configured
- Users with valid email addresses
- Incidents of different severity levels

**Test Steps:**
1. Create "Critical" severity incident
2. Verify immediate escalation notifications
3. Assign investigator to incident
4. Verify assignment notifications
5. Let investigation exceed time limits (if configured)
6. Verify escalation notifications for delays
7. Complete workflow stages
8. Verify completion notifications

**Expected Results:**
- Critical incidents trigger immediate notifications
- Assignment notifications sent to investigators
- Escalation notifications sent for delays
- Completion notifications sent to stakeholders
- Email content is appropriate and informative

**Post-conditions:** Notification system working correctly for all workflow events

---

### Test Case ID: INC-WORKFLOW-007
**Test Objective:** Verify workflow permissions and role-based access
**Priority:** High
**Prerequisites:** 
- Users with different roles (Employee, HSE Manager, Admin)
- Incidents in various workflow stages

**Test Steps:**
1. **Employee Role Testing:**
   - Verify can create incidents
   - Verify cannot assign investigators
   - Verify cannot change status beyond "Reported"

2. **HSE Manager Role Testing:**
   - Verify can assign investigators
   - Verify can manage corrective actions
   - Verify can change status through workflow

3. **Admin Role Testing:**
   - Verify full workflow access
   - Verify can override workflow restrictions (if applicable)

**Expected Results:**
- Each role has appropriate workflow permissions
- Unauthorized actions are blocked
- Clear error messages for permission denials
- Workflow buttons/options hidden for unauthorized users

**Post-conditions:** Role-based access control properly enforced

---

### Test Case ID: INC-WORKFLOW-008
**Test Objective:** Verify workflow audit trail and history tracking
**Priority:** Medium
**Prerequisites:** 
- Incident to process through workflow
- User accounts for workflow testing

**Test Steps:**
1. Process incident through complete workflow
2. Make status changes at each stage
3. Add notes and comments at each step
4. Assign and reassign investigators
5. Add and complete corrective actions
6. Review complete audit trail

**Expected Results:**
- All workflow changes are logged
- Timestamps and user information captured
- Status change history is complete
- Assignment changes are tracked
- Action history is maintained
- Audit trail is chronological and detailed

**Post-conditions:** Complete audit trail maintained for all workflow activities

---

### Test Case ID: INC-WORKFLOW-009
**Test Objective:** Verify workflow performance with high volume
**Priority:** Low
**Prerequisites:** 
- Multiple test incidents
- Multiple user accounts

**Test Steps:**
1. Create 20+ incidents simultaneously
2. Process multiple incidents through workflow concurrently
3. Assign investigators to multiple incidents
4. Monitor system performance during high activity
5. Verify all workflow operations complete successfully

**Expected Results:**
- System handles multiple concurrent workflow operations
- No performance degradation with high volume
- All workflow operations complete successfully
- Database integrity maintained
- No workflow conflicts or errors

**Post-conditions:** System performance acceptable under high workflow volume

---

### Test Case ID: INC-WORKFLOW-010
**Test Objective:** Verify workflow integration with external systems
**Priority:** Medium
**Prerequisites:** 
- External system integrations configured (if applicable)
- Test incidents for integration testing

**Test Steps:**
1. Create incident that should trigger external notifications
2. Verify external system receives incident data
3. Update incident status
4. Verify external system receives status updates
5. Complete incident workflow
6. Verify external system receives completion notification

**Expected Results:**
- External systems receive incident notifications
- Data synchronization works correctly
- Status updates are propagated
- Integration errors are handled gracefully
- No data loss during integration

**Post-conditions:** External system integration working correctly with workflow

---

### Test Case ID: INC-WORKFLOW-011
**Test Objective:** Verify workflow reopening and status reversal scenarios
**Priority:** Medium
**Prerequisites:** 
- Closed incident available for testing
- User with appropriate permissions

**Test Steps:**
1. Open incident in "Closed" status
2. Attempt to reopen incident (if feature exists)
3. Verify reopening process and requirements
4. Test status reversal from "Resolved" to "Awaiting Action"
5. Verify audit trail captures reopening activities

**Expected Results:**
- Reopening process works correctly (if implemented)
- Appropriate justification required for reopening
- Status reversals are properly controlled
- Audit trail captures all reopening activities
- Workflow integrity maintained during reversals

**Post-conditions:** Incident reopening handled correctly with proper controls

---

### Test Case ID: INC-WORKFLOW-012
**Test Objective:** Verify workflow dashboard and reporting integration
**Priority:** Medium
**Prerequisites:** 
- Dashboard/reporting system available
- Incidents in various workflow stages

**Test Steps:**
1. Access workflow dashboard
2. Verify incident counts by status
3. Check overdue investigation reports
4. Verify pending action reports
5. Test workflow performance metrics
6. Verify real-time updates in dashboard

**Expected Results:**
- Dashboard shows accurate workflow statistics
- Reports reflect current incident statuses
- Overdue items are properly identified
- Performance metrics are accurate
- Real-time updates work correctly

**Post-conditions:** Dashboard and reporting accurately reflect workflow status

---

### Test Case ID: INC-WORKFLOW-013
**Test Objective:** Verify workflow time tracking and SLA management
**Priority:** Medium
**Prerequisites:**
- SLA configuration for incident processing
- Incidents with different severity levels

**Test Steps:**
1. Create "Critical" incident and verify SLA timer starts
2. Monitor investigation time limits
3. Test escalation when SLA thresholds exceeded
4. Verify time tracking for each workflow stage
5. Complete incident within SLA requirements
6. Review time tracking reports

**Expected Results:**
- SLA timers start automatically upon incident creation
- Time tracking works for each workflow stage
- Escalations trigger when SLAs are exceeded
- Time reports show accurate processing times
- SLA compliance is properly measured

**Post-conditions:** SLA management and time tracking working correctly
