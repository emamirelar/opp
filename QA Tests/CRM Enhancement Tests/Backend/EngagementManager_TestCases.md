# EngagementManager Test Cases

**Manager**: `UNOPS.PAO.UNOPSBusiness/Managers/EngagementManager.cs`  
**Entity**: `UNOPS.PAO.Domain/Entities/Engagement.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 50  

---

## Overview

The EngagementManager handles partnership engagement operations:
- Engagement CRUD lifecycle
- Multi-partner associations
- Workflow management
- Document attachments
- Timeline tracking

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 12 | P0 |
| Workflow | 10 | P0 |
| Associations | 10 | P1 |
| Documents | 6 | P1 |
| Search & Filter | 8 | P1 |
| Validation | 4 | P0 |

---

## P0 - Critical Tests

### TC-EM-001: Create engagement
**Description**: Create new engagement  
**Test Steps**:
1. Call `CreateAsync(engagementData)`
2. Verify engagement created
**Expected Result**: Engagement with generated ID

### TC-EM-002: Get engagement by ID
**Description**: Retrieve engagement  
**Test Steps**:
1. Create engagement
2. Call `GetByIdAsync(id)`
**Expected Result**: Correct engagement returned

### TC-EM-003: Update engagement
**Description**: Update engagement details  
**Test Steps**:
1. Create engagement
2. Call `UpdateAsync(id, updatedData)`
**Expected Result**: Engagement updated

### TC-EM-004: Delete engagement
**Description**: Soft delete engagement  
**Test Steps**:
1. Create engagement
2. Call `DeleteAsync(id)`
**Expected Result**: Engagement marked as deleted

### TC-EM-005: Get engagements by partner
**Description**: Filter by associated partner  
**Test Steps**:
1. Create engagements with partner associations
2. Call `GetByPartnerIdAsync(partnerId)`
**Expected Result**: Partner's engagements returned

### TC-EM-006: Create with required fields
**Description**: Title, type required  
**Test Steps**:
1. Create without title
**Expected Result**: Validation error

### TC-EM-007: Duplicate prevention
**Description**: Same title/partner combination  
**Test Steps**:
1. Create engagement
2. Create duplicate
**Expected Result**: Conflict or unique created

### TC-EM-008: Initial workflow status
**Description**: Default to Draft status  
**Test Steps**:
1. Create engagement
2. Check status
**Expected Result**: Status = "Draft"

### TC-EM-009: Transition to Submitted
**Description**: Move from Draft to Submitted  
**Test Steps**:
1. Create in Draft
2. Call `SubmitAsync(id)`
**Expected Result**: Status = "Submitted"

### TC-EM-010: Transition to Approved
**Description**: Approve submitted engagement  
**Test Steps**:
1. Submit engagement
2. Call `ApproveAsync(id)`
**Expected Result**: Status = "Approved"

### TC-EM-011: Transition to Rejected
**Description**: Reject submitted engagement  
**Test Steps**:
1. Submit engagement
2. Call `RejectAsync(id, reason)`
**Expected Result**: Status = "Rejected"

### TC-EM-012: Invalid transition blocked
**Description**: Can't skip workflow states  
**Test Steps**:
1. Try approve from Draft
**Expected Result**: Validation error

---

## P1 - High Priority Tests

### TC-EM-013: Associate partner
**Description**: Link partner to engagement  
**Test Steps**:
1. Create engagement
2. Call `AddPartnerAsync(engagementId, partnerId)`
**Expected Result**: Partner associated

### TC-EM-014: Remove partner association
**Description**: Unlink partner  
**Test Steps**:
1. Associate partner
2. Call `RemovePartnerAsync(engagementId, partnerId)`
**Expected Result**: Association removed

### TC-EM-015: Associate multiple partners
**Description**: Multi-partner engagement  
**Test Steps**:
1. Associate 3 partners
2. Get engagement with includes
**Expected Result**: All partners linked

### TC-EM-016: Associate contact
**Description**: Link contact to engagement  
**Test Steps**:
1. Create engagement
2. Call `AddContactAsync(engagementId, contactId)`
**Expected Result**: Contact associated

### TC-EM-017: Get engagement contacts
**Description**: List associated contacts  
**Test Steps**:
1. Associate contacts
2. Call `GetContactsAsync(engagementId)`
**Expected Result**: Contact list

### TC-EM-018: Attach document
**Description**: Add document to engagement  
**Test Steps**:
1. Create engagement
2. Call `AttachDocumentAsync(id, documentData)`
**Expected Result**: Document attached

### TC-EM-019: Get engagement documents
**Description**: List attached documents  
**Test Steps**:
1. Attach documents
2. Call `GetDocumentsAsync(id)`
**Expected Result**: Document list

### TC-EM-020: Remove document
**Description**: Detach document  
**Test Steps**:
1. Attach document
2. Call `RemoveDocumentAsync(id, documentId)`
**Expected Result**: Document removed

### TC-EM-021: Search by title
**Description**: Text search  
**Test Steps**:
1. Create engagements
2. Search by partial title
**Expected Result**: Matching engagements

### TC-EM-022: Filter by status
**Description**: Status filter  
**Test Steps**:
1. Create with different statuses
2. Filter by "Active"
**Expected Result**: Only active

### TC-EM-023: Filter by date range
**Description**: Date range filter  
**Test Steps**:
1. Create with different dates
2. Filter by range
**Expected Result**: Within range

### TC-EM-024: Filter by engagement type
**Description**: Type filter  
**Test Steps**:
1. Create with different types
2. Filter by type
**Expected Result**: Matching type

### TC-EM-025: Pagination support
**Description**: Paginated results  
**Test Steps**:
1. Create 50 engagements
2. Get page 2, size 10
**Expected Result**: Correct page

### TC-EM-026: Sort by date
**Description**: Date ordering  
**Test Steps**:
1. Get with sortBy=createdDate
**Expected Result**: Ordered list

### TC-EM-027: Sort by title
**Description**: Alphabetical ordering  
**Test Steps**:
1. Get with sortBy=title
**Expected Result**: A-Z order

### TC-EM-028: Get workflow history
**Description**: Transition history  
**Test Steps**:
1. Make transitions
2. Call `GetHistoryAsync(id)`
**Expected Result**: History list

---

## Validation Tests

### TC-EM-V001: Title max length
**Description**: 255 char limit  
**Test Steps**:
1. Create with 300 char title
**Expected Result**: Validation error

### TC-EM-V002: Description max length
**Description**: 5000 char limit  
**Test Steps**:
1. Create with long description
**Expected Result**: Truncated or error

### TC-EM-V003: Budget positive number
**Description**: Budget must be >= 0  
**Test Steps**:
1. Create with negative budget
**Expected Result**: Validation error

### TC-EM-V004: End date after start date
**Description**: Date validation  
**Test Steps**:
1. Create with endDate < startDate
**Expected Result**: Validation error

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Managers/EngagementManagerTests.cs`

