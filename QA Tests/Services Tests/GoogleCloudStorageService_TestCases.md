# GoogleCloudStorageService Test Cases

**Service**: `UNOPS.PAO.UNOPSBusiness/Managers/GoogleCloudStorageService.cs`  
**Priority**: P0 - Critical  
**Total Test Cases**: 35  

---

## Overview

The GoogleCloudStorageService handles all file storage operations with Google Cloud Storage, including:
- File uploads and downloads
- Signed URL generation
- Bucket management
- File metadata operations
- Access control

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| File Upload | 8 | P0 |
| File Download | 6 | P0 |
| Signed URL Generation | 7 | P0 |
| File Deletion | 4 | P1 |
| Metadata Operations | 5 | P1 |
| Error Handling | 5 | P0 |

---

## P0 - Critical Tests

### TC-GCS-001: Upload file with valid content
**Description**: Upload a valid file to GCS bucket  
**Preconditions**: 
- Valid GCS credentials configured
- Target bucket exists
**Test Steps**:
1. Create a test file with sample content
2. Call `UploadFileAsync(fileName, content, contentType)`
3. Verify file exists in bucket
**Expected Result**: File uploaded successfully, returns file URL  
**Test Data**: 1MB test file, content-type: application/pdf

### TC-GCS-002: Upload file with empty content fails
**Description**: Attempting to upload empty content should fail gracefully  
**Preconditions**: Valid GCS credentials  
**Test Steps**:
1. Call `UploadFileAsync(fileName, emptyContent, contentType)`
2. Verify appropriate error is thrown
**Expected Result**: ArgumentException with message about empty content

### TC-GCS-003: Upload large file (10MB)
**Description**: Verify large file upload works correctly  
**Preconditions**: Valid GCS credentials, sufficient quota  
**Test Steps**:
1. Create a 10MB test file
2. Call `UploadFileAsync(fileName, content, contentType)`
3. Verify file uploaded completely
**Expected Result**: File uploaded within performance threshold (< 30 seconds)  
**Performance Criteria**: Upload completes in < 30s

### TC-GCS-004: Upload file with special characters in name
**Description**: File names with special characters should be handled  
**Preconditions**: Valid GCS credentials  
**Test Steps**:
1. Create file with name "test file (1) & special.pdf"
2. Upload file
3. Verify file accessible
**Expected Result**: File uploaded with properly encoded name

### TC-GCS-005: Upload file to nested folder path
**Description**: Upload file to a specific folder structure  
**Preconditions**: Valid GCS credentials  
**Test Steps**:
1. Specify path "partners/123/documents/file.pdf"
2. Upload file
3. Verify file at correct path
**Expected Result**: File uploaded to correct folder path

### TC-GCS-006: Download existing file
**Description**: Download a file that exists in GCS  
**Preconditions**: File exists in bucket  
**Test Steps**:
1. Upload a test file first
2. Call `DownloadFileAsync(filePath)`
3. Verify content matches original
**Expected Result**: File content matches original upload

### TC-GCS-007: Download non-existent file fails
**Description**: Attempting to download non-existent file should fail  
**Preconditions**: File does not exist  
**Test Steps**:
1. Call `DownloadFileAsync("nonexistent.pdf")`
2. Verify appropriate error
**Expected Result**: FileNotFoundException or appropriate error

### TC-GCS-008: Generate signed URL for download
**Description**: Generate a time-limited signed URL for file access  
**Preconditions**: File exists in bucket  
**Test Steps**:
1. Upload a test file
2. Call `GenerateSignedUrlAsync(filePath, TimeSpan.FromMinutes(15))`
3. Verify URL is valid and accessible
**Expected Result**: Signed URL returned, accessible within time limit

### TC-GCS-009: Signed URL expires correctly
**Description**: Signed URL should expire after specified duration  
**Preconditions**: File exists, short expiration configured  
**Test Steps**:
1. Generate signed URL with 1-second expiration
2. Wait 2 seconds
3. Attempt to access URL
**Expected Result**: URL access fails after expiration

### TC-GCS-010: Generate signed URL for upload
**Description**: Generate a signed URL for file upload  
**Preconditions**: Valid bucket access  
**Test Steps**:
1. Call `GenerateSignedUploadUrlAsync(filePath, TimeSpan.FromMinutes(15))`
2. Use URL to upload file
3. Verify file exists in bucket
**Expected Result**: Signed upload URL works correctly

### TC-GCS-011: Handle network timeout gracefully
**Description**: Service should handle network timeouts  
**Preconditions**: Simulated network delay  
**Test Steps**:
1. Configure mock to simulate timeout
2. Call upload operation
3. Verify timeout exception handled
**Expected Result**: TimeoutException thrown with appropriate message

### TC-GCS-012: Handle authentication failure
**Description**: Service should handle invalid credentials  
**Preconditions**: Invalid or expired credentials  
**Test Steps**:
1. Configure invalid credentials
2. Attempt upload
3. Verify authentication error
**Expected Result**: AuthenticationException with clear message

### TC-GCS-013: Handle bucket not found
**Description**: Service should handle missing bucket  
**Preconditions**: Non-existent bucket name  
**Test Steps**:
1. Configure non-existent bucket
2. Attempt upload
3. Verify appropriate error
**Expected Result**: BucketNotFoundException or similar

### TC-GCS-014: Handle quota exceeded
**Description**: Service should handle quota limits  
**Preconditions**: Quota limit simulated  
**Test Steps**:
1. Mock quota exceeded response
2. Attempt upload
3. Verify quota error handled
**Expected Result**: QuotaExceededException with retry guidance

### TC-GCS-015: Upload with content type detection
**Description**: Content type should be auto-detected if not provided  
**Preconditions**: Valid GCS credentials  
**Test Steps**:
1. Upload file without specifying content type
2. Verify content type auto-detected
**Expected Result**: Correct content type set based on file extension

---

## P1 - High Priority Tests

### TC-GCS-016: Delete existing file
**Description**: Delete a file from GCS bucket  
**Preconditions**: File exists  
**Test Steps**:
1. Upload test file
2. Call `DeleteFileAsync(filePath)`
3. Verify file no longer exists
**Expected Result**: File deleted successfully

### TC-GCS-017: Delete non-existent file (idempotent)
**Description**: Deleting non-existent file should not fail  
**Preconditions**: File does not exist  
**Test Steps**:
1. Call `DeleteFileAsync("nonexistent.pdf")`
2. Verify no exception
**Expected Result**: Operation completes without error

### TC-GCS-018: Get file metadata
**Description**: Retrieve metadata for existing file  
**Preconditions**: File exists  
**Test Steps**:
1. Upload file with metadata
2. Call `GetFileMetadataAsync(filePath)`
3. Verify metadata returned
**Expected Result**: Metadata includes size, content-type, created date

### TC-GCS-019: Update file metadata
**Description**: Update metadata on existing file  
**Preconditions**: File exists  
**Test Steps**:
1. Upload file
2. Call `UpdateFileMetadataAsync(filePath, metadata)`
3. Verify metadata updated
**Expected Result**: Metadata updated successfully

### TC-GCS-020: List files in folder
**Description**: List all files in a folder path  
**Preconditions**: Multiple files exist in folder  
**Test Steps**:
1. Upload multiple files to same folder
2. Call `ListFilesAsync(folderPath)`
3. Verify all files returned
**Expected Result**: List contains all uploaded files

### TC-GCS-021: List files with pagination
**Description**: List files with pagination support  
**Preconditions**: Many files exist (> 100)  
**Test Steps**:
1. Create 150 test files
2. Call `ListFilesAsync(folderPath, pageSize: 50)`
3. Verify pagination works
**Expected Result**: Returns 50 files with continuation token

### TC-GCS-022: Copy file to new location
**Description**: Copy file within bucket  
**Preconditions**: Source file exists  
**Test Steps**:
1. Upload source file
2. Call `CopyFileAsync(source, destination)`
3. Verify both files exist
**Expected Result**: File copied, original remains

### TC-GCS-023: Move file to new location
**Description**: Move file within bucket  
**Preconditions**: Source file exists  
**Test Steps**:
1. Upload source file
2. Call `MoveFileAsync(source, destination)`
3. Verify file at new location, not at old
**Expected Result**: File moved successfully

### TC-GCS-024: Check file exists
**Description**: Check if file exists in bucket  
**Preconditions**: Mix of existing and non-existing files  
**Test Steps**:
1. Upload test file
2. Call `FileExistsAsync(existingPath)` - should return true
3. Call `FileExistsAsync(nonExistingPath)` - should return false
**Expected Result**: Correct boolean returned

### TC-GCS-025: Get file size
**Description**: Get size of file in bucket  
**Preconditions**: File exists  
**Test Steps**:
1. Upload file with known size
2. Call `GetFileSizeAsync(filePath)`
3. Compare with original size
**Expected Result**: Size matches original file

---

## P2 - Medium Priority Tests

### TC-GCS-026: Upload with custom metadata
**Description**: Upload file with custom metadata  
**Test Steps**:
1. Create custom metadata dictionary
2. Upload file with metadata
3. Verify metadata stored
**Expected Result**: Custom metadata retrievable

### TC-GCS-027: Concurrent uploads
**Description**: Handle multiple simultaneous uploads  
**Test Steps**:
1. Initiate 10 concurrent uploads
2. Verify all complete successfully
**Expected Result**: All files uploaded, no conflicts  
**Performance Criteria**: Complete within 60 seconds

### TC-GCS-028: Concurrent downloads
**Description**: Handle multiple simultaneous downloads  
**Test Steps**:
1. Upload 10 test files
2. Initiate 10 concurrent downloads
3. Verify all complete
**Expected Result**: All files downloaded correctly

### TC-GCS-029: Upload resume after failure
**Description**: Resume interrupted upload  
**Test Steps**:
1. Start large file upload
2. Simulate interruption
3. Resume upload
**Expected Result**: Upload completes from interruption point

### TC-GCS-030: Signed URL with custom headers
**Description**: Generate signed URL requiring specific headers  
**Test Steps**:
1. Generate URL with required content-type header
2. Access with correct header - succeeds
3. Access without header - fails
**Expected Result**: Header requirement enforced

---

## Performance Tests

### TC-GCS-P001: Upload 100MB file performance
**Description**: Large file upload within acceptable time  
**Performance Criteria**: Complete in < 120 seconds  
**Test Data**: 100MB test file

### TC-GCS-P002: Generate 100 signed URLs performance
**Description**: Bulk signed URL generation  
**Performance Criteria**: Complete in < 5 seconds

### TC-GCS-P003: List 1000 files performance
**Description**: Large directory listing performance  
**Performance Criteria**: Complete in < 10 seconds

### TC-GCS-P004: Download 50MB file performance
**Description**: Large file download within acceptable time  
**Performance Criteria**: Complete in < 60 seconds

### TC-GCS-P005: Concurrent 50 upload operations
**Description**: High concurrency upload handling  
**Performance Criteria**: Complete in < 120 seconds, no failures

---

## Error Handling Tests

| Test ID | Scenario | Expected Behavior |
|---------|----------|-------------------|
| TC-GCS-E001 | Invalid bucket name | Clear error message |
| TC-GCS-E002 | Permission denied | AuthorizationException |
| TC-GCS-E003 | Invalid file path | ArgumentException |
| TC-GCS-E004 | Network disconnection | Retry with backoff |
| TC-GCS-E005 | Corrupted upload | Checksum validation fails |

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/GoogleCloudStorageServiceTests.cs`

