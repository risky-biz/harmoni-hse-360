# Search and Filter - Manual Test Cases

## Test Case Overview
This document contains comprehensive test cases for incident search and filtering functionality in the Harmoni360 system.

---

### Test Case ID: INC-SEARCH-001
**Test Objective:** Verify basic text search functionality across incident fields
**Priority:** High
**Prerequisites:** 
- User logged in with incident viewing permissions
- Multiple incidents with varied content in database

**Test Data:**
- Search terms: "equipment", "malfunction", "cafeteria", "slip"
- Incidents containing these terms in titles and descriptions

**Test Steps:**
1. Navigate to incident list page
2. Enter "equipment" in search box
3. Click search button or press Enter
4. Verify results show incidents containing "equipment"
5. Clear search and enter "malfunction"
6. Verify results update to show relevant incidents
7. Test search with partial words (e.g., "equip")
8. Test case-insensitive search ("EQUIPMENT")

**Expected Results:**
- Search returns incidents containing search terms
- Results include matches from title, description, and location fields
- Partial word matching works correctly
- Search is case-insensitive
- Results are displayed in logical order (relevance or date)
- Search performance is acceptable (< 3 seconds)

**Post-conditions:** Search functionality working correctly for basic text queries

---

### Test Case ID: INC-SEARCH-002
**Test Objective:** Verify incident filtering by status
**Priority:** High
**Prerequisites:** 
- User with incident viewing permissions
- Incidents in various status states

**Test Data:**
- Incidents in each status: Reported, Under Investigation, Awaiting Action, Resolved, Closed

**Test Steps:**
1. Access incident list with status filter
2. Select "Reported" from status filter dropdown
3. Verify only "Reported" incidents are displayed
4. Change filter to "Under Investigation"
5. Verify results update to show only investigating incidents
6. Test "All Statuses" option
7. Test multiple status selection (if supported)

**Expected Results:**
- Status filter dropdown shows all available statuses
- Filtering works correctly for each status
- Results update immediately when filter changes
- "All Statuses" shows complete incident list
- Filter state is maintained during session
- Incident counts are accurate for each status

**Post-conditions:** Status filtering working correctly for all incident statuses

---

### Test Case ID: INC-SEARCH-003
**Test Objective:** Verify incident filtering by severity level
**Priority:** High
**Prerequisites:** 
- User with viewing permissions
- Incidents with different severity levels

**Test Data:**
- Incidents with severity: Minor, Moderate, Serious, Critical

**Test Steps:**
1. Navigate to incident list page
2. Select "Critical" from severity filter
3. Verify only critical incidents are displayed
4. Change filter to "Minor"
5. Verify results show only minor incidents
6. Test "All Severities" option
7. Combine severity filter with other filters

**Expected Results:**
- Severity filter shows all severity levels
- Filtering works accurately for each severity
- Results update correctly when filter changes
- Combined filters work together properly
- Filter maintains state during navigation

**Post-conditions:** Severity filtering functioning correctly

---

### Test Case ID: INC-SEARCH-004
**Test Objective:** Verify date range filtering for incident dates
**Priority:** Medium
**Prerequisites:** 
- User with viewing permissions
- Incidents with various incident dates

**Test Data:**
- Incidents from different date ranges (last week, last month, last year)

**Test Steps:**
1. Access incident list with date filters
2. Set "From Date" to one week ago
3. Set "To Date" to current date
4. Apply date filter
5. Verify only incidents within date range are shown
6. Test with different date ranges
7. Test with single date (from = to)
8. Test with invalid date ranges (from > to)

**Expected Results:**
- Date picker controls work correctly
- Date range filtering is accurate
- Invalid date ranges are handled gracefully
- Results update when date filters change
- Date format is consistent and user-friendly

**Post-conditions:** Date range filtering working correctly

---

### Test Case ID: INC-SEARCH-005
**Test Objective:** Verify advanced search with multiple combined filters
**Priority:** Medium
**Prerequisites:** 
- User with viewing permissions
- Diverse incident data for comprehensive testing

**Test Steps:**
1. Apply text search for "equipment"
2. Add status filter for "Under Investigation"
3. Add severity filter for "Serious"
4. Add date range filter for last month
5. Verify results match all criteria
6. Remove one filter and verify results update
7. Clear all filters and verify full list returns

**Expected Results:**
- Multiple filters work together correctly (AND logic)
- Results match all applied criteria
- Removing filters updates results appropriately
- Clear all filters functionality works
- Filter combinations are logical and useful

**Post-conditions:** Combined filtering working correctly

---

### Test Case ID: INC-SEARCH-006
**Test Objective:** Verify search result sorting and pagination
**Priority:** Medium
**Prerequisites:** 
- User with viewing permissions
- Large number of incidents for pagination testing

**Test Steps:**
1. Perform search that returns many results
2. Test sorting by incident date (newest first)
3. Test sorting by incident date (oldest first)
4. Test sorting by severity level
5. Test sorting by status
6. Navigate through pagination controls
7. Change page size and verify results

**Expected Results:**
- Sorting works correctly for each column
- Sort direction indicators are clear
- Pagination controls are functional
- Page size changes work correctly
- Sort order is maintained during pagination

**Post-conditions:** Sorting and pagination working correctly

---

### Test Case ID: INC-SEARCH-007
**Test Objective:** Verify search performance with large datasets
**Priority:** Low
**Prerequisites:** 
- Large number of incidents in database (100+)
- User with viewing permissions

**Test Steps:**
1. Perform text search on large dataset
2. Measure search response time
3. Apply multiple filters simultaneously
4. Test pagination with large result sets
5. Monitor system performance during search operations

**Expected Results:**
- Search completes within acceptable time (< 5 seconds)
- System remains responsive during search
- Large result sets are handled efficiently
- Pagination performance is acceptable
- No system timeouts or errors occur

**Post-conditions:** Search performance acceptable with large datasets

---

### Test Case ID: INC-SEARCH-008
**Test Objective:** Verify search functionality for different user roles
**Priority:** High
**Prerequisites:** 
- Users with different roles (Employee, HSE Manager, Admin)
- Incidents created by different users

**Test Steps:**
1. Login as Employee user
2. Perform search and verify results show appropriate incidents
3. Login as HSE Manager
4. Perform same search and verify expanded results
5. Login as Admin
6. Verify full search access to all incidents

**Expected Results:**
- Employees see only their own incidents in search results
- HSE Managers see all incidents in search results
- Admins have full search access
- Search respects role-based access controls
- No unauthorized incident data is exposed

**Post-conditions:** Search access control working correctly

---

### Test Case ID: INC-SEARCH-009
**Test Objective:** Verify search with special characters and edge cases
**Priority:** Medium
**Prerequisites:** 
- User with viewing permissions
- Incidents with special characters in content

**Test Steps:**
1. Search for incidents with special characters (@, #, &, etc.)
2. Test search with very long search terms
3. Test search with empty/whitespace-only terms
4. Test search with SQL injection attempts
5. Test search with HTML/script tags

**Expected Results:**
- Special characters in search work correctly
- Long search terms are handled gracefully
- Empty searches return appropriate results or errors
- SQL injection attempts are blocked
- HTML/script content is properly escaped

**Post-conditions:** Search security and edge case handling working correctly

---

### Test Case ID: INC-SEARCH-010
**Test Objective:** Verify saved search and filter preferences
**Priority:** Low
**Prerequisites:** 
- User with viewing permissions
- Search preference functionality (if implemented)

**Test Steps:**
1. Apply specific search criteria and filters
2. Save search preferences (if feature exists)
3. Navigate away from incident list
4. Return to incident list
5. Verify saved preferences are applied
6. Test updating saved preferences

**Expected Results:**
- Search preferences can be saved (if implemented)
- Saved preferences persist across sessions
- Preferences can be updated and deleted
- Default preferences work correctly

**Post-conditions:** Search preferences working correctly (if implemented)

---

### Test Case ID: INC-SEARCH-011
**Test Objective:** Verify export functionality for search results
**Priority:** Medium
**Prerequisites:** 
- User with export permissions
- Search results to export

**Test Steps:**
1. Perform search with specific criteria
2. Select incidents from search results
3. Click export button (if available)
4. Choose export format (CSV, PDF, Excel)
5. Verify exported file contains correct data
6. Test export with large result sets

**Expected Results:**
- Export functionality works for search results
- Exported data matches search results
- Multiple export formats are supported
- Large exports complete successfully
- Export files are properly formatted

**Post-conditions:** Export functionality working correctly for search results

---

### Test Case ID: INC-SEARCH-012
**Test Objective:** Verify mobile responsiveness of search and filter interface
**Priority:** Medium
**Prerequisites:** 
- Mobile device or browser developer tools
- User with viewing permissions

**Test Steps:**
1. Access incident list on mobile device
2. Test search box functionality on mobile
3. Test filter dropdowns on mobile interface
4. Verify search results display correctly
5. Test pagination controls on mobile
6. Test sorting functionality on mobile

**Expected Results:**
- Search interface is mobile-friendly
- All search controls are accessible on mobile
- Results display correctly on small screens
- Touch interactions work smoothly
- Mobile performance is acceptable

**Post-conditions:** Search functionality fully functional on mobile devices
