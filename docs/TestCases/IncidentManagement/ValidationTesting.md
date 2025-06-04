# Validation Testing - Manual Test Cases

## Test Case Overview
This document contains comprehensive test cases for data validation and error handling in the HarmoniHSE360 incident management system.

---

### Test Case ID: INC-VALID-001
**Test Objective:** Verify required field validation for incident creation
**Priority:** High
**Prerequisites:** 
- User logged in with incident creation permissions
- Access to incident creation form

**Test Steps:**
1. Navigate to incident creation page
2. Leave Title field empty and attempt to submit
3. Verify error message for required title
4. Enter title, leave Description empty, attempt submit
5. Verify error message for required description
6. Fill description, leave Severity unselected, attempt submit
7. Verify error message for required severity
8. Leave Location empty and attempt submit
9. Verify error message for required location
10. Leave Incident Date empty and attempt submit
11. Verify error message for required date

**Expected Results:**
- Form submission is prevented when required fields are empty
- Clear error messages displayed for each missing required field
- Error messages appear near the relevant form fields
- Form highlights problematic fields (red border, etc.)
- Multiple validation errors can be displayed simultaneously
- Submit button remains disabled until all required fields are filled

**Post-conditions:** Required field validation working correctly

---

### Test Case ID: INC-VALID-002
**Test Objective:** Verify field length validation for text inputs
**Priority:** High
**Prerequisites:** 
- User with incident creation/edit permissions
- Understanding of field length limits

**Test Data:**
- Title: 5-100 characters (test with 4, 5, 100, 101 characters)
- Description: 10-1000 characters (test with 9, 10, 1000, 1001 characters)
- Location: 3+ characters (test with 2, 3 characters)

**Test Steps:**
1. **Title Field Testing:**
   - Enter 4 characters in title field, attempt submit
   - Verify minimum length error message
   - Enter exactly 5 characters, verify acceptance
   - Enter exactly 100 characters, verify acceptance
   - Enter 101 characters, verify maximum length error

2. **Description Field Testing:**
   - Enter 9 characters in description, attempt submit
   - Verify minimum length error
   - Enter exactly 10 characters, verify acceptance
   - Enter exactly 1000 characters, verify acceptance
   - Enter 1001 characters, verify maximum length error

3. **Location Field Testing:**
   - Enter 2 characters in location, attempt submit
   - Verify minimum length error
   - Enter 3+ characters, verify acceptance

**Expected Results:**
- Minimum length validation prevents submission with too few characters
- Maximum length validation prevents entry of excess characters
- Character counters display current/maximum counts (if implemented)
- Error messages specify exact character requirements
- Field validation occurs in real-time during typing

**Post-conditions:** Field length validation working correctly for all text inputs

---

### Test Case ID: INC-VALID-003
**Test Objective:** Verify date and time validation for incident dates
**Priority:** High
**Prerequisites:** 
- User with incident creation permissions
- Various date/time test scenarios

**Test Steps:**
1. **Future Date Testing:**
   - Set incident date to tomorrow
   - Attempt to submit form
   - Verify future date error message

2. **Invalid Date Format Testing:**
   - Enter invalid date format (if manual entry allowed)
   - Verify format validation error

3. **Date Range Testing:**
   - Set incident date to very old date (e.g., 1900)
   - Verify reasonable date range validation

4. **Time Validation Testing:**
   - Test various time formats
   - Verify time component validation

**Expected Results:**
- Future dates are rejected with clear error message
- Invalid date formats are rejected
- Unreasonable historical dates may be flagged
- Time validation works correctly
- Date picker prevents invalid date selection

**Post-conditions:** Date and time validation working correctly

---

### Test Case ID: INC-VALID-004
**Test Objective:** Verify GPS coordinate validation
**Priority:** Medium
**Prerequisites:** 
- User with incident creation permissions
- GPS coordinate input fields

**Test Data:**
- Valid coordinates: 40.7128, -74.0060 (New York)
- Invalid latitude: 91.0 (exceeds ±90 range)
- Invalid longitude: 181.0 (exceeds ±180 range)

**Test Steps:**
1. Enter valid latitude and longitude coordinates
2. Verify coordinates are accepted
3. Enter latitude > 90 degrees
4. Verify validation error for invalid latitude
5. Enter longitude > 180 degrees
6. Verify validation error for invalid longitude
7. Enter non-numeric values in coordinate fields
8. Verify numeric validation errors

**Expected Results:**
- Valid coordinates within proper ranges are accepted
- Latitude values outside ±90 range are rejected
- Longitude values outside ±180 range are rejected
- Non-numeric coordinate values are rejected
- Clear error messages explain coordinate requirements

**Post-conditions:** GPS coordinate validation working correctly

---

### Test Case ID: INC-VALID-005
**Test Objective:** Verify severity and status enum validation
**Priority:** High
**Prerequisites:** 
- User with incident management permissions
- Access to severity and status selection fields

**Test Steps:**
1. **Severity Validation:**
   - Verify only valid severity options are available (Minor, Moderate, Serious, Critical)
   - Test form submission with each severity level
   - Attempt to submit without selecting severity

2. **Status Validation:**
   - Verify only valid status options are available
   - Test status transitions follow business rules
   - Attempt invalid status transitions

3. **API Testing (if applicable):**
   - Send API request with invalid severity value
   - Send API request with invalid status value
   - Verify API rejects invalid enum values

**Expected Results:**
- Only valid enum values are selectable in dropdowns
- Invalid enum values are rejected by backend
- Status transitions follow defined workflow rules
- API validates enum values and returns appropriate errors

**Post-conditions:** Enum validation working correctly for severity and status

---

### Test Case ID: INC-VALID-006
**Test Objective:** Verify special character and injection attack prevention
**Priority:** High
**Prerequisites:** 
- User with incident creation permissions
- Test data with special characters and potential injection attacks

**Test Data:**
- SQL injection attempts: "'; DROP TABLE incidents; --"
- XSS attempts: "<script>alert('XSS')</script>"
- HTML injection: "<h1>Test</h1><img src='x' onerror='alert(1)'>"
- Special characters: "Test @#$%^&*()_+ incident"

**Test Steps:**
1. Enter SQL injection attempt in title field
2. Submit form and verify data is properly escaped
3. Enter XSS script in description field
4. Verify script is not executed and is properly escaped
5. Enter HTML tags in various fields
6. Verify HTML is escaped and not rendered
7. Enter legitimate special characters
8. Verify special characters are preserved correctly

**Expected Results:**
- SQL injection attempts are prevented/escaped
- XSS scripts are not executed and are properly escaped
- HTML injection is prevented
- Legitimate special characters are preserved
- No script execution occurs in any context
- Data integrity is maintained

**Post-conditions:** Security validation preventing injection attacks

---

### Test Case ID: INC-VALID-007
**Test Objective:** Verify file upload validation and security
**Priority:** High
**Prerequisites:** 
- User with file upload permissions
- Various test files for validation testing

**Test Data:**
- Valid files: .jpg, .pdf, .docx (under 10MB)
- Invalid files: .exe, .bat, .zip
- Oversized files: >10MB
- Malicious files: renamed executables

**Test Steps:**
1. Upload valid file types and verify acceptance
2. Attempt to upload .exe file, verify rejection
3. Upload file exceeding 10MB limit, verify rejection
4. Upload file with correct extension but wrong content type
5. Upload file with malicious content
6. Test multiple file upload validation

**Expected Results:**
- Valid file types are accepted
- Invalid file types are rejected with clear error messages
- File size limits are enforced
- File content validation prevents malicious uploads
- Multiple file validation works correctly
- Security measures prevent dangerous file uploads

**Post-conditions:** File upload validation and security working correctly

---

### Test Case ID: INC-VALID-008
**Test Objective:** Verify form validation error message clarity and usability
**Priority:** Medium
**Prerequisites:** 
- User with form access
- Various validation scenarios

**Test Steps:**
1. Trigger multiple validation errors simultaneously
2. Verify all error messages are displayed
3. Check error message positioning and visibility
4. Test error message language and clarity
5. Verify error messages disappear when issues are resolved
6. Test validation error accessibility features

**Expected Results:**
- Multiple error messages can be displayed simultaneously
- Error messages are positioned near relevant fields
- Error messages are clear and actionable
- Error messages disappear when validation passes
- Error messages are accessible to screen readers
- Error styling is consistent and noticeable

**Post-conditions:** Validation error messages are user-friendly and accessible

---

### Test Case ID: INC-VALID-009
**Test Objective:** Verify client-side vs server-side validation consistency
**Priority:** High
**Prerequisites:** 
- User with form access
- Browser developer tools for testing

**Test Steps:**
1. Disable JavaScript in browser
2. Attempt to submit form with invalid data
3. Verify server-side validation catches errors
4. Re-enable JavaScript
5. Test same validation scenarios
6. Verify client-side and server-side validation are consistent

**Expected Results:**
- Server-side validation works without JavaScript
- Client-side validation provides immediate feedback
- Both validation layers catch the same errors
- No security vulnerabilities exist when JavaScript is disabled
- Validation rules are consistent between client and server

**Post-conditions:** Validation works correctly with and without JavaScript

---

### Test Case ID: INC-VALID-010
**Test Objective:** Verify validation performance with large data inputs
**Priority:** Low
**Prerequisites:** 
- User with form access
- Large text inputs for testing

**Test Steps:**
1. Enter maximum allowed characters in description field (1000)
2. Monitor form responsiveness during typing
3. Test validation performance with large inputs
4. Submit form with maximum data and measure response time
5. Test validation with multiple large fields

**Expected Results:**
- Form remains responsive with large text inputs
- Validation performance is acceptable
- Form submission completes within reasonable time
- No browser freezing or performance issues
- Large data validation is efficient

**Post-conditions:** Validation performance acceptable with large inputs

---

### Test Case ID: INC-VALID-011
**Test Objective:** Verify validation for optional fields and edge cases
**Priority:** Medium
**Prerequisites:** 
- User with form access
- Understanding of optional field requirements

**Test Steps:**
1. Submit form with only required fields filled
2. Verify optional fields can be left empty
3. Fill optional fields with valid data
4. Test optional field length limits
5. Test optional field format validation (if applicable)

**Expected Results:**
- Optional fields can be left empty without errors
- Optional fields accept valid data when provided
- Optional field validation works when data is entered
- Form submission succeeds with or without optional data

**Post-conditions:** Optional field validation working correctly

---

### Test Case ID: INC-VALID-012
**Test Objective:** Verify validation in different browsers and devices
**Priority:** Medium
**Prerequisites:** 
- Access to multiple browsers (Chrome, Firefox, Safari, Edge)
- Mobile devices for testing

**Test Steps:**
1. Test form validation in Chrome browser
2. Test same validation scenarios in Firefox
3. Test validation in Safari (if available)
4. Test validation in Edge browser
5. Test validation on mobile devices
6. Verify consistent validation behavior across platforms

**Expected Results:**
- Validation works consistently across all browsers
- Error messages display correctly in all browsers
- Mobile validation works properly
- No browser-specific validation issues
- Consistent user experience across platforms

**Post-conditions:** Validation working consistently across all supported platforms
