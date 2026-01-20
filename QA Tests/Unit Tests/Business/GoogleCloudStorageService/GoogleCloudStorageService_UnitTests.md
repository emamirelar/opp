# GoogleCloudStorageService - Unit Test Cases

**Manager**: `GoogleCloudStorageService`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/GoogleCloudStorageService.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: Cloud storage operations, File upload/download, Bucket management, Access control

**Total Test Cases**: 15+

---

## Test Categories

### 1. Upload Operations (4 tests)
- TC-GCS-001: Upload file
- TC-GCS-002: Upload large file
- TC-GCS-003: Upload with metadata
- TC-GCS-004: Resumable upload

### 2. Download Operations (3 tests)
- TC-GCS-005: Download file
- TC-GCS-006: Download file range
- TC-GCS-007: Generate signed URL

### 3. File Management (4 tests)
- TC-GCS-008: List files in bucket
- TC-GCS-009: Delete file
- TC-GCS-010: Move file
- TC-GCS-011: Copy file

### 4. Error Handling (4 tests)
- TC-GCS-012: Handle authentication error
- TC-GCS-013: Handle quota exceeded
- TC-GCS-014: Handle network timeout
- TC-GCS-015: Retry transient errors

**Coverage**: 70%+

