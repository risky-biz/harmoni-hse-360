# Create Incident - Manual Test Cases

## Test Case Overview
This document contains comprehensive test cases for the incident creation functionality in the Harmoni360 system.

---

### Test Case ID: INC-CREATE-001
**Test Objective:** Verify successful creation of incident with all required fields
**Priority:** High
**Prerequisites:** 
- User logged in with Employee role or higher
- Access to incident creation page (/incidents/create)

**Test Data:**
- Title: "Workplace slip and fall incident"
- Description: "Employee slipped on wet floor in cafeteria area during lunch break"
- Severity: "Moderate"
- Incident Date: Current date and time
- Location: "Main Building - Cafeteria"

**Test Steps:**
1. Navigate to incident creation page
2. Enter valid title in the Title field
3. Enter detailed description in Description field
4. Select "Moderate" from Severity dropdown
5. Set incident date to current date/time
6. Enter location information
7. Click "Submit" button

**Expected Results:**
- Form accepts all valid input without errors
- Success message displays: "Incident created successfully"
- User redirected to incident details page
- Incident appears in incident list with "Reported" status
- Auto-generated incident ID is assigned

**Post-conditions:** New incident record created in database with "Reported" status

---

### Test Case ID: INC-CREATE-002
**Test Objective:** Verify incident creation with optional GPS coordinates
**Priority:** Medium
**Prerequisites:** 
- User logged in with incident creation permissions
- Browser location services enabled

**Test Data:**
- Title: "Equipment malfunction in warehouse"
- Description: "Forklift hydraulic system failure during material handling operation"
- Severity: "Serious"
- Location: "Warehouse Section B"
- Latitude: 40.7128
- Longitude: -74.0060

**Test Steps:**
1. Navigate to incident creation page
2. Fill in all required fields with test data
3. Click "Get Current Location" button (if available)
4. Verify GPS coordinates are populated automatically OR manually enter coordinates
5. Verify location is displayed on map preview (if available)
6. Submit the incident

**Expected Results:**
- GPS coordinates are captured and stored
- Location appears correctly on any map display
- Incident is created successfully with location data
- Coordinates are visible in incident details

**Post-conditions:** Incident created with accurate GPS location data

---

### Test Case ID: INC-CREATE-003
**Test Objective:** Verify incident creation with witness information and immediate actions
**Priority:** Medium
**Prerequisites:** User logged in with incident reporting permissions

**Test Data:**
- Title: "Chemical spill in laboratory"
- Description: "Accidental spill of cleaning solution in Lab Room 205"
- Severity: "Critical"
- Location: "Research Building - Lab 205"
- Witness Names: "John Smith, Sarah Johnson"
- Immediate Actions: "Area evacuated, spill contained with absorbent materials, safety team notified"

**Test Steps:**
1. Access incident creation form
2. Enter required incident details
3. Fill in "Witness Names" field with multiple witnesses
4. Enter detailed immediate actions taken
5. Select "Critical" severity level
6. Submit the incident

**Expected Results:**
- All optional fields accept input correctly
- Critical severity triggers appropriate notifications
- Witness information is saved and displayed
- Immediate actions are recorded in incident details
- System may trigger additional workflows for critical incidents

**Post-conditions:** Critical incident created with complete witness and action information

---

### Test Case ID: INC-CREATE-004
**Test Objective:** Verify auto-save functionality during incident creation
**Priority:** Medium
**Prerequisites:** 
- User logged in
- Auto-save feature enabled

**Test Data:**
- Partial incident data for testing auto-save

**Test Steps:**
1. Navigate to incident creation page
2. Start entering incident title
3. Wait for auto-save indicator (should appear after few seconds)
4. Continue entering description
5. Observe auto-save status indicators
6. Refresh the page or navigate away and return
7. Verify previously entered data is preserved

**Expected Results:**
- Auto-save indicator shows "Saving..." then "Saved"
- Data persists after page refresh
- No data loss occurs during form completion
- Auto-save occurs at regular intervals

**Post-conditions:** Draft incident data preserved in browser storage

---

### Test Case ID: INC-CREATE-005
**Test Objective:** Verify incident creation for different severity levels
**Priority:** High
**Prerequisites:** User with incident creation permissions

**Test Data:**
- Test incidents for each severity: Minor, Moderate, Serious, Critical

**Test Steps:**
1. Create incident with "Minor" severity
   - Verify standard workflow applies
2. Create incident with "Moderate" severity
   - Verify appropriate notifications sent
3. Create incident with "Serious" severity
   - Verify escalation procedures triggered
4. Create incident with "Critical" severity
   - Verify immediate notifications and escalations

**Expected Results:**
- Each severity level processes correctly
- Appropriate notifications sent based on severity
- Serious and Critical incidents trigger special handling
- Status tracking works for all severity levels

**Post-conditions:** Incidents created with proper severity-based processing

---

### Test Case ID: INC-CREATE-006
**Test Objective:** Verify incident creation with maximum character limits
**Priority:** Medium
**Prerequisites:** User logged in with creation permissions

**Test Data:**
- Title: 100 characters (maximum allowed)
- Description: 1000 characters (maximum allowed)
- Location: 200 characters (maximum allowed)
- Witness Names: 500 characters (maximum allowed)
- Immediate Actions: 500 characters (maximum allowed)

**Test Steps:**
1. Navigate to incident creation form
2. Enter exactly 100 characters in title field
3. Enter exactly 1000 characters in description
4. Fill other fields with maximum allowed characters
5. Verify character counters (if present) show correct counts
6. Submit the incident

**Expected Results:**
- All fields accept maximum character limits
- Character counters display correctly
- No truncation occurs during save
- Incident creates successfully with full data

**Post-conditions:** Incident created with maximum allowed content

---

### Test Case ID: INC-CREATE-007
**Test Objective:** Verify incident creation from mobile device
**Priority:** Medium
**Prerequisites:** 
- Mobile device with supported browser
- User logged in

**Test Data:** Standard incident creation data

**Test Steps:**
1. Access incident creation page on mobile device
2. Verify form layout is responsive and usable
3. Test touch interactions with form fields
4. Test dropdown selections on mobile
5. Test date/time picker on mobile
6. Submit incident from mobile device

**Expected Results:**
- Form displays correctly on mobile screen
- All form elements are accessible and functional
- Touch interactions work smoothly
- Date/time pickers are mobile-friendly
- Incident submits successfully from mobile

**Post-conditions:** Incident successfully created via mobile interface

---

### Test Case ID: INC-CREATE-008
**Test Objective:** Verify incident creation with special characters and international text
**Priority:** Low
**Prerequisites:** User logged in

**Test Data:**
- Title with special characters: "Incident #123 - Equipment @Failure!"
- Description with international characters: "Incidente en área de producción - équipement défaillant"
- Location with symbols: "Building A-1 (Section β)"

**Test Steps:**
1. Enter title with special characters and numbers
2. Enter description with international characters
3. Enter location with symbols and Greek letters
4. Submit the incident
5. Verify data displays correctly in incident details

**Expected Results:**
- All special characters are accepted and stored
- International characters display correctly
- No encoding issues occur
- Data integrity maintained throughout system

**Post-conditions:** Incident created with special character data intact

---

### Test Case ID: INC-CREATE-009
**Test Objective:** Verify incident creation performance with large description
**Priority:** Low
**Prerequisites:** User logged in

**Test Data:**
- Large description (approaching 1000 character limit)
- Multiple paragraphs with detailed information

**Test Steps:**
1. Enter very detailed, long description (900+ characters)
2. Include multiple paragraphs and detailed information
3. Monitor form responsiveness during typing
4. Submit the incident
5. Measure time from submission to confirmation

**Expected Results:**
- Form remains responsive with large text input
- No performance degradation during typing
- Submission completes within reasonable time (< 5 seconds)
- Large description saves and displays correctly

**Post-conditions:** Performance acceptable with maximum content size

---

### Test Case ID: INC-CREATE-010
**Test Objective:** Verify incident creation cancellation and data cleanup
**Priority:** Medium
**Prerequisites:** User logged in

**Test Steps:**
1. Start creating an incident with partial data
2. Click "Cancel" or navigate away from page
3. Verify confirmation dialog appears (if implemented)
4. Confirm cancellation
5. Return to incident creation page
6. Verify form is cleared or shows appropriate state

**Expected Results:**
- Cancellation works without errors
- User prompted to confirm if unsaved changes exist
- Form state is properly reset after cancellation
- No partial data persists inappropriately

**Post-conditions:** Clean form state after cancellation
