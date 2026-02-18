# GoogleDriveDocumentManager Test Cases

**Service**: `UNOPS.PAO.UNOPSBusiness/Managers/GoogleDriveDocumentManager.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 25  

---

## Overview

The GoogleDriveDocumentManager handles Google Drive integration:
- File operations (upload, download, delete)
- Folder management
- Permission management
- File sharing

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| File Operations | 10 | P0 |
| Folder Operations | 5 | P1 |
| Permissions | 6 | P0 |
| Error Handling | 4 | P1 |

---

## P0 - Critical Tests

### TC-GDM-001: Upload file to Drive
**Description**: Upload file to Google Drive  
**Test Steps**:
1. Call `UploadFileAsync(file, folderId)`
2. Verify file uploaded
**Expected Result**: File ID returned

### TC-GDM-002: Download file from Drive
**Description**: Download file content  
**Test Steps**:
1. Upload file
2. Call `DownloadFileAsync(fileId)`
**Expected Result**: File content returned

### TC-GDM-003: Delete file from Drive
**Description**: Delete file permanently  
**Test Steps**:
1. Upload file
2. Call `DeleteFileAsync(fileId)`
**Expected Result**: File removed

### TC-GDM-004: Get file metadata
**Description**: Get file information  
**Test Steps**:
1. Upload file
2. Call `GetFileMetadataAsync(fileId)`
**Expected Result**: Metadata returned

### TC-GDM-005: Move file to folder
**Description**: Move file between folders  
**Test Steps**:
1. Create folders and file
2. Call `MoveFileAsync(fileId, targetFolderId)`
**Expected Result**: File moved

### TC-GDM-006: Copy file
**Description**: Create file copy  
**Test Steps**:
1. Upload file
2. Call `CopyFileAsync(fileId, newName)`
**Expected Result**: New file created

### TC-GDM-007: Share file with user
**Description**: Grant user access  
**Test Steps**:
1. Upload file
2. Call `ShareWithUserAsync(fileId, email, role)`
**Expected Result**: Permission added

### TC-GDM-008: Remove file sharing
**Description**: Revoke access  
**Test Steps**:
1. Share file
2. Call `RemoveShareAsync(fileId, email)`
**Expected Result**: Permission removed

### TC-GDM-009: Get file permissions
**Description**: List file permissions  
**Test Steps**:
1. Share file with multiple users
2. Call `GetPermissionsAsync(fileId)`
**Expected Result**: All permissions listed

### TC-GDM-010: Generate shareable link
**Description**: Create public link  
**Test Steps**:
1. Upload file
2. Call `CreateShareableLinkAsync(fileId)`
**Expected Result**: Shareable URL returned

---

## P1 - High Priority Tests

### TC-GDM-011: Create folder
**Description**: Create new folder  
**Test Steps**:
1. Call `CreateFolderAsync(name, parentId)`
**Expected Result**: Folder ID returned

### TC-GDM-012: List folder contents
**Description**: Get folder files  
**Test Steps**:
1. Create folder with files
2. Call `ListFolderContentsAsync(folderId)`
**Expected Result**: File list returned

### TC-GDM-013: Delete folder
**Description**: Delete folder and contents  
**Test Steps**:
1. Create folder with files
2. Call `DeleteFolderAsync(folderId)`
**Expected Result**: Folder and contents removed

### TC-GDM-014: Search files
**Description**: Search by name/type  
**Test Steps**:
1. Upload files
2. Call `SearchFilesAsync(query)`
**Expected Result**: Matching files

### TC-GDM-015: Update file content
**Description**: Replace file content  
**Test Steps**:
1. Upload file
2. Call `UpdateFileContentAsync(fileId, newContent)`
**Expected Result**: Content updated

### TC-GDM-016: Get storage quota
**Description**: Check storage usage  
**Test Steps**:
1. Call `GetQuotaAsync()`
**Expected Result**: Usage and limit

### TC-GDM-017: Handle large file upload
**Description**: Upload file > 5MB  
**Test Steps**:
1. Create 10MB file
2. Upload with resumable
**Expected Result**: File uploaded

### TC-GDM-018: Handle quota exceeded
**Description**: Quota error handled  
**Test Steps**:
1. Mock quota exceeded
2. Attempt upload
**Expected Result**: Clear error message

### TC-GDM-019: Handle auth error
**Description**: Auth failure handled  
**Test Steps**:
1. Mock auth failure
2. Attempt operation
**Expected Result**: Auth error message

### TC-GDM-020: Handle file not found
**Description**: Missing file handled  
**Test Steps**:
1. Call operation on invalid ID
**Expected Result**: Not found error

---

## Performance Tests

### TC-GDM-P001: Upload 50MB file < 60s
**Performance Criteria**: < 60 seconds

### TC-GDM-P002: List 1000 files < 5s
**Performance Criteria**: < 5 seconds

### TC-GDM-P003: Concurrent uploads
**Performance Criteria**: 10 concurrent < 120s

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/GoogleDriveDocumentManagerTests.cs`

