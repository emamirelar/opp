# GoogleDriveDocumentManager - Unit Test Cases

**Manager**: `GoogleDriveDocumentManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/GoogleDriveDocumentManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `GoogleDriveDocumentManager` with focus on:
- Google Drive upload/download
- File management
- Permission management
- Error handling (PNO-680 related)
- Configuration validation

**Total Test Cases**: 20+

---

## 1. Upload Tests

### TC-GD-001: Upload File to Drive
**Test**: `UploadFile_Should_CreateFile_When_ValidFileProvided`

### TC-GD-002: Upload with Metadata
**Test**: `UploadFile_Should_IncludeMetadata_When_MetadataProvided`

### TC-GD-003: Upload Large File
**Test**: `UploadFile_Should_UseResumableUpload_When_FileSizeExceedsLimit`

### TC-GD-004: Upload to Folder
**Test**: `UploadFile_Should_PlaceInFolder_When_FolderIdProvided`

### TC-GD-005: Upload - Authentication Failure
**Test**: `UploadFile_Should_ThrowException_When_AuthenticationFails`

---

## 2. Download Tests

### TC-GD-006: Download File from Drive
**Test**: `DownloadFile_Should_ReturnContent_When_FileExists`

### TC-GD-007: Download Non-Existent File
**Test**: `DownloadFile_Should_ThrowException_When_FileNotFound`

### TC-GD-008: Download with Permissions Check
**Test**: `DownloadFile_Should_VerifyPermissions_When_DownloadRequested`

---

## 3. File Management Tests

### TC-GD-009: List Files in Folder
**Test**: `ListFiles_Should_ReturnFiles_When_FolderExists`

### TC-GD-010: Create Folder
**Test**: `CreateFolder_Should_CreateFolder_When_ValidNameProvided`

### TC-GD-011: Delete File
**Test**: `DeleteFile_Should_RemoveFile_When_FileExists`

### TC-GD-012: Move File
**Test**: `MoveFile_Should_ChangeLocation_When_ValidTargetProvided`

### TC-GD-013: Rename File
**Test**: `RenameFile_Should_UpdateName_When_NewNameProvided`

---

## 4. Permission Management Tests

### TC-GD-014: Share File
**Test**: `ShareFile_Should_AddPermission_When_ValidUserProvided`

### TC-GD-015: Revoke Access
**Test**: `RevokeAccess_Should_RemovePermission_When_PermissionExists`

### TC-GD-016: Get File Permissions
**Test**: `GetPermissions_Should_ReturnList_When_FileHasPermissions`

---

## 5. Configuration & Error Handling Tests (PNO-680)

### TC-GD-017: Validate Configuration on Startup
**Test**: `ValidateConfig_Should_ThrowException_When_CredentialsMissing`

### TC-GD-018: Handle Service Unavailable
**Test**: `UploadFile_Should_ThrowException_When_DriveServiceUnavailable`

### TC-GD-019: Handle Quota Exceeded
**Test**: `UploadFile_Should_ThrowException_When_QuotaExceeded`

### TC-GD-020: Retry on Transient Error
**Test**: `UploadFile_Should_Retry_When_TransientErrorOccurs`

---

## Coverage Goals
**Overall**: 20+ tests, 75%+ coverage, PNO-680 prevention

