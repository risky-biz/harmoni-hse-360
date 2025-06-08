# Attachment Management - Manual Test Cases

## Test Case Overview
This document contains comprehensive test cases for file attachment functionality in the Harmoni360 incident management system.

---

### Test Case ID: INC-ATTACH-001
**Test Objective:** Verify successful upload of supported file types
**Priority:** High
**Prerequisites:** 
- User logged in with attachment upload permissions
- Existing incident available for file attachment
- Test files of various supported formats

**Test Data:**
- Image files: .jpg, .jpeg, .png, .gif (under 10MB each)
- Document files: .pdf, .doc, .docx, .txt
- Video files: .mp4, .avi, .mov (under 10MB each)

**Test Steps:**
1. Navigate to incident details page
2. Click "Add Attachments" or "Upload Files" button
3. Select a JPEG image file (< 5MB)
4. Verify file appears in upload queue
5. Click "Upload" button
6. Repeat for each supported file type
7. Verify all files upload successfully

**Expected Results:**
- All supported file types are accepted
- Upload progress indicator shows during upload
- Success message displays for each successful upload
- Files appear in attachments list with correct names
- File size and type information displayed correctly
- Download links are functional

**Post-conditions:** Multiple files of different types successfully attached to incident

---

### Test Case ID: INC-ATTACH-002
**Test Objective:** Verify file size limit enforcement (10MB maximum)
**Priority:** High
**Prerequisites:** 
- User with upload permissions
- Test files exceeding 10MB size limit

**Test Data:**
- Image file: 15MB JPEG
- Video file: 25MB MP4
- Document file: 12MB PDF

**Test Steps:**
1. Access incident attachment upload
2. Attempt to select file larger than 10MB
3. Verify system response to oversized file
4. Try uploading file exactly at 10MB limit
5. Try uploading file just under 10MB limit

**Expected Results:**
- Files over 10MB are rejected with clear error message
- Error message specifies maximum file size allowed
- Files at exactly 10MB are accepted
- Files under 10MB upload successfully
- No partial uploads occur for oversized files

**Post-conditions:** File size limits properly enforced

---

### Test Case ID: INC-ATTACH-003
**Test Objective:** Verify rejection of unsupported file types
**Priority:** High
**Prerequisites:** 
- User with upload permissions
- Test files with unsupported extensions

**Test Data:**
- Executable files: .exe, .bat, .sh
- Archive files: .zip, .rar, .7z
- Script files: .js, .php, .py
- Other formats: .psd, .ai, .dwg

**Test Steps:**
1. Navigate to file upload interface
2. Attempt to select .exe file
3. Verify system rejection
4. Try each unsupported file type
5. Verify appropriate error messages

**Expected Results:**
- All unsupported file types are rejected
- Clear error message lists allowed file types
- File selection dialog may filter to allowed types only
- No security vulnerabilities from malicious files
- Consistent error handling for all unsupported types

**Post-conditions:** Security maintained by rejecting dangerous file types

---

### Test Case ID: INC-ATTACH-004
**Test Objective:** Verify multiple file upload functionality
**Priority:** Medium
**Prerequisites:** 
- User with upload permissions
- Multiple test files of supported types

**Test Data:**
- 5 different image files (various formats)
- 3 document files
- 2 video files

**Test Steps:**
1. Access attachment upload interface
2. Select multiple files simultaneously (if supported)
3. Verify all selected files appear in upload queue
4. Upload all files at once
5. Monitor upload progress for each file
6. Verify all files complete successfully

**Expected Results:**
- Multiple file selection works correctly
- Upload queue shows all selected files
- Progress indicators work for each file
- All files upload without interference
- Success confirmation for each file
- All files appear in attachments list

**Post-conditions:** Multiple files successfully attached in single operation

---

### Test Case ID: INC-ATTACH-005
**Test Objective:** Verify file download and viewing functionality
**Priority:** High
**Prerequisites:** 
- Incident with existing file attachments
- User with appropriate viewing permissions

**Test Steps:**
1. Navigate to incident with attachments
2. Click download link for image file
3. Verify image downloads correctly
4. Click download link for PDF document
5. Verify PDF opens/downloads properly
6. Test download for each file type
7. Verify file integrity after download

**Expected Results:**
- Download links are functional for all file types
- Files download with original names
- Downloaded files are not corrupted
- Images display correctly when opened
- Documents open in appropriate applications
- File sizes match original uploads

**Post-conditions:** All attached files are accessible and downloadable

---

### Test Case ID: INC-ATTACH-006
**Test Objective:** Verify attachment deletion functionality
**Priority:** Medium
**Prerequisites:** 
- User with attachment management permissions
- Incident with existing attachments

**Test Steps:**
1. Navigate to incident attachments list
2. Locate attachment to delete
3. Click "Delete" or "Remove" button
4. Confirm deletion when prompted
5. Verify attachment is removed from list
6. Attempt to access deleted file URL directly
7. Verify file is no longer accessible

**Expected Results:**
- Delete button is available for authorized users
- Confirmation dialog appears before deletion
- Attachment is removed from incident
- File is deleted from storage system
- Deleted file URLs return appropriate error
- Audit trail records deletion activity

**Post-conditions:** Attachment successfully removed from incident and storage

---

### Test Case ID: INC-ATTACH-007
**Test Objective:** Verify file upload security validation (magic bytes check)
**Priority:** High
**Prerequisites:** 
- User with upload permissions
- Malicious test files (renamed executables, etc.)

**Test Data:**
- Executable file renamed with .jpg extension
- Script file renamed with .pdf extension
- Malformed image files

**Test Steps:**
1. Create test file: rename .exe file to .jpg
2. Attempt to upload the disguised executable
3. Verify system detects file type mismatch
4. Try uploading malformed image file
5. Test with various file signature mismatches

**Expected Results:**
- System detects file type mismatches
- Files with incorrect signatures are rejected
- Clear error messages about file validation
- No malicious files are stored in system
- Security validation cannot be bypassed

**Post-conditions:** System security maintained against malicious file uploads

---

### Test Case ID: INC-ATTACH-008
**Test Objective:** Verify attachment upload during incident creation
**Priority:** Medium
**Prerequisites:** 
- User with incident creation permissions
- Test files for attachment

**Test Steps:**
1. Navigate to incident creation form
2. Fill in required incident details
3. Add attachments before submitting incident
4. Submit incident with attachments
5. Verify incident and attachments are created together
6. Check that attachments are properly linked to new incident

**Expected Results:**
- Attachment upload works during incident creation
- Files are properly associated with new incident
- Incident creation succeeds with attachments
- All attachments are accessible after creation
- No orphaned files are created

**Post-conditions:** New incident created with properly linked attachments

---

### Test Case ID: INC-ATTACH-009
**Test Objective:** Verify attachment upload error handling and recovery
**Priority:** Medium
**Prerequisites:** 
- User with upload permissions
- Network connectivity that can be interrupted

**Test Steps:**
1. Start uploading large file (near 10MB limit)
2. Interrupt network connection during upload
3. Restore network connection
4. Verify system handles interrupted upload
5. Retry upload of same file
6. Test upload with corrupted file

**Expected Results:**
- System detects upload interruptions
- Clear error messages for failed uploads
- Retry mechanism available for failed uploads
- No partial files are stored
- Corrupted uploads are rejected
- User can recover from upload errors

**Post-conditions:** Robust error handling for upload failures

---

### Test Case ID: INC-ATTACH-010
**Test Objective:** Verify attachment access control and permissions
**Priority:** High
**Prerequisites:** 
- Users with different role levels
- Incident with attachments

**Test Steps:**
1. Login as Employee (incident reporter)
2. Verify can view own incident attachments
3. Attempt to access attachments from other incidents
4. Login as HSE Manager
5. Verify can access all incident attachments
6. Test download permissions for each role
7. Test upload permissions for each role

**Expected Results:**
- Employees can access attachments for their incidents
- HSE Managers can access all incident attachments
- Unauthorized access is blocked with appropriate errors
- Upload permissions match incident edit permissions
- Download links work only for authorized users

**Post-conditions:** Attachment access properly controlled by user permissions

---

### Test Case ID: INC-ATTACH-011
**Test Objective:** Verify attachment metadata and audit trail
**Priority:** Medium
**Prerequisites:** 
- User with upload permissions
- Incident for attachment testing

**Test Steps:**
1. Upload file attachment to incident
2. Verify attachment metadata is captured (upload date, user, file size)
3. Download the attachment
4. Delete the attachment
5. Check audit trail for all attachment activities

**Expected Results:**
- Upload timestamp and user are recorded
- File size and type information stored
- Download activities are logged (if implemented)
- Deletion activities are recorded in audit trail
- Complete attachment history is maintained

**Post-conditions:** Complete audit trail maintained for all attachment activities

---

### Test Case ID: INC-ATTACH-012
**Test Objective:** Verify attachment performance with multiple large files
**Priority:** Low
**Prerequisites:** 
- User with upload permissions
- Multiple large files (8-10MB each)

**Test Steps:**
1. Upload 5 large files simultaneously
2. Monitor upload progress and system performance
3. Verify all uploads complete successfully
4. Test downloading multiple large files
5. Monitor system response time

**Expected Results:**
- System handles multiple large uploads without degradation
- Upload progress indicators work correctly
- All files upload within reasonable time
- Download performance remains acceptable
- No system timeouts or errors occur

**Post-conditions:** System performance acceptable with large file operations
