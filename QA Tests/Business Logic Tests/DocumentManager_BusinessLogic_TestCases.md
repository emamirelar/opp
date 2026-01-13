# DocumentManager - Business Logic Test Cases

## Manager Overview
**Manager**: `DocumentManager` / `UNOPSDocumentManager`  
**Location**: `UNOPS.PAO.Business/Managers/DocumentManager.cs`, `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSDocumentManager.cs`  
**Purpose**: Manages documents including upload, storage (Google Cloud Storage), sharing, and association with entities.

## Key Business Rules (From PRD)

1. **Storage Integration**: Documents stored in Google Cloud Storage
2. **Entity Association**: Documents can be linked to Partners, Contacts, Interactions
3. **Document Types**: Documents have types for classification
4. **Signed URLs**: Temporary access URLs generated for document download
5. **File Validation**: File type and size limits enforced
6. **Soft Delete**: Documents are soft-deleted, preserving history
7. **Metadata**: Name, description, type, file details tracked

---

## P0 - Critical Business Logic Tests

### TC-DM-BL-P0-001: Upload Document - Valid File
**Priority**: P0 - Critical  
**Description**: Verify document upload to storage  
**Business Rule**: Valid files uploaded to Google Cloud Storage  
**Preconditions**: Storage configured, valid file

**Test Steps**:
1. Prepare valid PDF file
2. Call upload method with file and metadata
3. Verify file stored
4. Verify document record created with Link

**Expected Result**: Document uploaded and tracked  
**Business Impact**: Core document management

---

### TC-DM-BL-P0-002: Upload Document - Invalid File Type
**Priority**: P0 - Critical  
**Description**: Verify rejected file types  
**Business Rule**: Only allowed file types accepted  
**Preconditions**: None

**Test Steps**:
1. Attempt upload of .exe file
2. Verify rejected with clear error
3. Attempt upload of .pdf - should succeed

**Expected Result**: Invalid types rejected  
**Business Impact**: Security

---

### TC-DM-BL-P0-003: Upload Document - Size Limit
**Priority**: P0 - Critical  
**Description**: Verify file size limit enforcement  
**Business Rule**: Files above limit rejected  
**Preconditions**: Size limit configured

**Test Steps**:
1. Attempt upload of oversized file
2. Verify rejected with error
3. Upload file within limit - should succeed

**Expected Result**: Size limits enforced  
**Business Impact**: Storage management

---

### TC-DM-BL-P0-004: Generate Signed URL - Valid Document
**Priority**: P0 - Critical  
**Description**: Verify signed URL generation  
**Business Rule**: Temporary access URLs generated for downloads  
**Preconditions**: Document exists in storage

**Test Steps**:
1. Call GetSignedUrl for document
2. Verify URL returned
3. Verify URL is time-limited
4. Verify URL provides access

**Expected Result**: Signed URL generated  
**Business Impact**: Secure document access

---

### TC-DM-BL-P0-005: Associate Document with Partner
**Priority**: P0 - Critical  
**Description**: Verify document-partner association  
**Business Rule**: Documents can be linked to partners  
**Preconditions**: Partner and document exist

**Test Steps**:
1. Associate document with partner
2. Verify relationship created
3. Query partner documents
4. Verify document in list

**Expected Result**: Partner association works  
**Business Impact**: Organization documentation

---

### TC-DM-BL-P0-006: Associate Document with Contact
**Priority**: P0 - Critical  
**Description**: Verify document-contact association  
**Business Rule**: Documents can be linked to contacts  
**Preconditions**: Contact and document exist

**Test Steps**:
1. Associate document with contact
2. Verify relationship created
3. Query contact documents
4. Verify document in list

**Expected Result**: Contact association works  
**Business Impact**: Contact documentation

---

### TC-DM-BL-P0-007: Delete Document - Soft Delete
**Priority**: P0 - Critical  
**Description**: Verify document soft deletion  
**Business Rule**: Documents soft-deleted, not removed  
**Preconditions**: Document exists

**Test Steps**:
1. Delete document
2. Verify IsDeleted = true
3. Verify not in active lists
4. Verify file NOT removed from storage

**Expected Result**: Soft delete applied  
**Business Impact**: Data recovery capability

---

### TC-DM-BL-P0-008: Get Document - With Link
**Priority**: P0 - Critical  
**Description**: Verify document retrieval includes link  
**Business Rule**: Link property required for access  
**Preconditions**: Document with storage link

**Test Steps**:
1. Get document by ID
2. Verify Link property populated
3. Verify link is valid storage path

**Expected Result**: Document with link returned  
**Business Impact**: Document accessibility

---

## P1 - High Priority Business Logic Tests

### TC-DM-BL-P1-001: Document Type Assignment
**Priority**: P1 - High  
**Description**: Verify document type classification  
**Business Rule**: Documents have types for organization  
**Preconditions**: Document types defined

**Test Steps**:
1. Create document with Type = "Contract"
2. Verify type stored
3. Filter documents by type
4. Verify filtering works

**Expected Result**: Type classification works  
**Business Impact**: Document organization

---

### TC-DM-BL-P1-002: Document List by Entity
**Priority**: P1 - High  
**Description**: Verify listing documents for entity  
**Business Rule**: Can retrieve all documents for partner/contact  
**Preconditions**: Entity with 10 documents

**Test Steps**:
1. Query documents for entity
2. Verify all 10 returned
3. Verify proper ordering

**Expected Result**: Entity documents listed  
**Business Impact**: Complete documentation view

---

### TC-DM-BL-P1-003: Document Update - Metadata Only
**Priority**: P1 - High  
**Description**: Verify updating document metadata  
**Business Rule**: Name, description updatable without re-upload  
**Preconditions**: Document exists

**Test Steps**:
1. Update document name and description
2. Verify changes saved
3. Verify Link unchanged

**Expected Result**: Metadata updateable  
**Business Impact**: Information correction

---

### TC-DM-BL-P1-004: Document Replace - New Version
**Priority**: P1 - High  
**Description**: Verify document replacement  
**Business Rule**: Can replace file while keeping metadata  
**Preconditions**: Document exists

**Test Steps**:
1. Upload replacement file
2. Verify new Link generated
3. Verify metadata preserved

**Expected Result**: Document replaced  
**Business Impact**: Version management

---

### TC-DM-BL-P1-005: Document Pagination
**Priority**: P1 - High  
**Description**: Verify document list pagination  
**Business Rule**: Large lists paginated  
**Preconditions**: 100 documents

**Test Steps**:
1. Query with pagination
2. Verify correct page size
3. Verify total count correct

**Expected Result**: Pagination works  
**Business Impact**: Performance

---

### TC-DM-BL-P1-006: Multiple Entity Association
**Priority**: P1 - High  
**Description**: Verify document linked to multiple entities  
**Business Rule**: Same document can be linked to partner and contact  
**Preconditions**: Document, partner, contact exist

**Test Steps**:
1. Link document to partner
2. Link same document to contact
3. Verify both relationships exist

**Expected Result**: Multiple associations allowed  
**Business Impact**: Flexible organization

---

### TC-DM-BL-P1-007: Document Search by Name
**Priority**: P1 - High  
**Description**: Verify searching documents by name  
**Business Rule**: Documents searchable by name  
**Preconditions**: Documents with various names

**Test Steps**:
1. Search for "Contract"
2. Verify matching documents returned
3. Verify case handling

**Expected Result**: Name search works  
**Business Impact**: Document discovery

---

### TC-DM-BL-P1-008: Document Audit Fields
**Priority**: P1 - High  
**Description**: Verify document audit trail  
**Business Rule**: Created/Modified tracking  
**Preconditions**: Document exists

**Test Steps**:
1. Verify CreatedBy/CreatedDate set
2. Update document
3. Verify LastModifiedBy/Date updated

**Expected Result**: Audit trail maintained  
**Business Impact**: Accountability

---

## P2 - Medium Priority Business Logic Tests

### TC-DM-BL-P2-001: Large File Upload
**Priority**: P2 - Medium  
**Description**: Verify large file handling (within limits)  
**Test Steps**:
1. Upload 50MB file
2. Verify successful
3. Verify signed URL works

**Expected Result**: Large files handled

---

### TC-DM-BL-P2-002: Document Description - Long Text
**Priority**: P2 - Medium  
**Description**: Verify long descriptions  
**Test Steps**:
1. Create with 2000 character description
2. Verify stored completely

**Expected Result**: Long descriptions supported

---

### TC-DM-BL-P2-003: Signed URL Expiration
**Priority**: P2 - Medium  
**Description**: Verify URL time limits  
**Test Steps**:
1. Generate signed URL
2. Verify expiration time set
3. Wait for expiration
4. Verify URL no longer works

**Expected Result**: Time-limited access enforced

---

### TC-DM-BL-P2-004: Special Characters in Name
**Priority**: P2 - Medium  
**Description**: Verify filename handling  
**Test Steps**:
1. Upload file with special characters in name
2. Verify handled appropriately (sanitized or preserved)

**Expected Result**: Special characters handled

---

## P3 - Low Priority Edge Cases

### TC-DM-BL-P3-001: Empty File
**Priority**: P3 - Low  
**Description**: Verify empty file handling  
**Test Steps**:
1. Attempt upload of 0-byte file
2. Verify rejection or handling

**Expected Result**: Empty files handled

---

### TC-DM-BL-P3-002: Orphaned Document
**Priority**: P3 - Low  
**Description**: Verify document without entity associations  
**Test Steps**:
1. Create document without associations
2. Verify accessible
3. Verify can be associated later

**Expected Result**: Orphaned documents allowed

---

### TC-DM-BL-P3-003: Concurrent Uploads
**Priority**: P3 - Low  
**Description**: Verify concurrent upload handling  
**Test Steps**:
1. Upload multiple files simultaneously
2. Verify all succeed
3. Verify no conflicts

**Expected Result**: Concurrent uploads work

---

## Integration with Unit Tests

Implement in: `tests/UNOPS.PAO.Business.Tests/Managers/DocumentManagerBusinessLogicTests.cs`

---

## Related Documentation

- [Document Entity](../../UNOPS.PAO.Domain/Entities/Document.cs)
- [Google Cloud Storage Integration](../../UNOPS.PAO.GoogleServices/)
- [Existing Test Cases](../Business/DocumentManager/DocumentManager_TestCases.md)

