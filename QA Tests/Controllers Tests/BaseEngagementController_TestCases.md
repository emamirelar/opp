# BaseEngagementController Test Cases

**Controller**: `UNOPS.PAO.UNOPSPresentation/Controllers/BaseEngagementController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 35  

---

## Overview

The BaseEngagementController manages engagement entities:
- Engagement CRUD operations
- Workflow transitions
- Partner associations
- Document attachments

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 10 | P0 |
| Workflow | 8 | P0 |
| Associations | 7 | P1 |
| Documents | 5 | P1 |
| Authorization | 5 | P0 |

---

## P0 - Critical Tests

### TC-BE-001: Get all engagements
**Description**: List all engagements  
**Test Steps**:
1. Call `GET /api/engagements`
**Expected Result**: Paginated engagement list

### TC-BE-002: Get engagement by ID
**Description**: Get specific engagement  
**Test Steps**:
1. Call `GET /api/engagements/{id}`
**Expected Result**: Engagement details

### TC-BE-003: Create engagement
**Description**: Create new engagement  
**Test Steps**:
1. Call `POST /api/engagements` with data
**Expected Result**: Engagement created

### TC-BE-004: Update engagement
**Description**: Update engagement  
**Test Steps**:
1. Call `PUT /api/engagements/{id}`
**Expected Result**: Engagement updated

### TC-BE-005: Delete engagement
**Description**: Soft delete engagement  
**Test Steps**:
1. Call `DELETE /api/engagements/{id}`
**Expected Result**: Engagement marked deleted

### TC-BE-006: Get engagements by partner
**Description**: Filter by partner  
**Test Steps**:
1. Call `GET /api/partners/{partnerId}/engagements`
**Expected Result**: Partner's engagements

### TC-BE-007: Get engagements by status
**Description**: Filter by status  
**Test Steps**:
1. Call `GET /api/engagements?status=Active`
**Expected Result**: Active engagements

### TC-BE-008: Transition workflow status
**Description**: Move to next stage  
**Test Steps**:
1. Call `POST /api/engagements/{id}/transition`
**Expected Result**: Status changed

### TC-BE-009: Transition with validation
**Description**: Validate before transition  
**Test Steps**:
1. Transition with missing required fields
**Expected Result**: Validation error

### TC-BE-010: Get workflow history
**Description**: View status changes  
**Test Steps**:
1. Call `GET /api/engagements/{id}/history`
**Expected Result**: Transition history

---

## P1 - High Priority Tests

### TC-BE-011: Get available transitions
**Description**: Get valid next states  
**Test Steps**:
1. Call `GET /api/engagements/{id}/transitions`
**Expected Result**: Available transitions

### TC-BE-012: Approve engagement
**Description**: Approval workflow  
**Test Steps**:
1. Call `POST /api/engagements/{id}/approve`
**Expected Result**: Engagement approved

### TC-BE-013: Reject engagement
**Description**: Rejection workflow  
**Test Steps**:
1. Call `POST /api/engagements/{id}/reject`
**Expected Result**: Engagement rejected

### TC-BE-014: Request changes
**Description**: Send back for revision  
**Test Steps**:
1. Call `POST /api/engagements/{id}/request-changes`
**Expected Result**: Status changed

### TC-BE-015: Submit for approval
**Description**: Submit draft  
**Test Steps**:
1. Call `POST /api/engagements/{id}/submit`
**Expected Result**: Pending approval

### TC-BE-016: Associate partner
**Description**: Link partner to engagement  
**Test Steps**:
1. Call `POST /api/engagements/{id}/partners/{partnerId}`
**Expected Result**: Partner linked

### TC-BE-017: Remove partner association
**Description**: Unlink partner  
**Test Steps**:
1. Call `DELETE /api/engagements/{id}/partners/{partnerId}`
**Expected Result**: Association removed

### TC-BE-018: Get engagement partners
**Description**: List associated partners  
**Test Steps**:
1. Call `GET /api/engagements/{id}/partners`
**Expected Result**: Partner list

### TC-BE-019: Associate contact
**Description**: Link contact to engagement  
**Test Steps**:
1. Call `POST /api/engagements/{id}/contacts/{contactId}`
**Expected Result**: Contact linked

### TC-BE-020: Get engagement contacts
**Description**: List associated contacts  
**Test Steps**:
1. Call `GET /api/engagements/{id}/contacts`
**Expected Result**: Contact list

### TC-BE-021: Attach document
**Description**: Upload document  
**Test Steps**:
1. Call `POST /api/engagements/{id}/documents`
**Expected Result**: Document attached

### TC-BE-022: Get engagement documents
**Description**: List documents  
**Test Steps**:
1. Call `GET /api/engagements/{id}/documents`
**Expected Result**: Document list

### TC-BE-023: Remove document
**Description**: Detach document  
**Test Steps**:
1. Call `DELETE /api/engagements/{id}/documents/{docId}`
**Expected Result**: Document removed

### TC-BE-024: Clone engagement
**Description**: Duplicate engagement  
**Test Steps**:
1. Call `POST /api/engagements/{id}/clone`
**Expected Result**: New engagement created

### TC-BE-025: Export engagement
**Description**: Export to PDF/Excel  
**Test Steps**:
1. Call `GET /api/engagements/{id}/export`
**Expected Result**: Export file

---

## Authorization Tests

### TC-BE-A001: Create requires permission
**Expected Result**: 403 without permission

### TC-BE-A002: Org unit filter applied
**Expected Result**: Only permitted engagements

### TC-BE-A003: Approval requires approver role
**Expected Result**: 403 for non-approvers

### TC-BE-A004: Delete requires owner or admin
**Expected Result**: 403 for others

### TC-BE-A005: View respects permissions
**Expected Result**: Field-level permissions

---

## Search & Filter Tests

### TC-BE-S001: Search by title
**Description**: Text search

### TC-BE-S002: Filter by date range
**Description**: Date filtering

### TC-BE-S003: Filter by amount range
**Description**: Budget filtering

### TC-BE-S004: Combined filters
**Description**: Multiple criteria

### TC-BE-S005: Sort by date/amount
**Description**: Sorting options

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/BaseEngagementControllerTests.cs`

