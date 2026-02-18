# OrganizationHierarchyLookupController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/OrganizationUnits/OrganizationHierarchyLookupController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 20  

---

## Overview

The OrganizationHierarchyLookupController provides org unit lookup operations:
- Hierarchy tree retrieval
- Quick lookup for dropdowns
- Parent/child navigation
- User's accessible org units

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Hierarchy Retrieval | 8 | P0 |
| Navigation | 6 | P1 |
| User Access | 4 | P0 |
| Authorization | 2 | P0 |

---

## P0 - Critical Tests

### TC-OHLC-001: Get org unit by ID
**Description**: Retrieve org unit by ID  
**Test Steps**:
1. Call `GET /api/org-units/lookup/{id}`
**Expected Result**: Org unit details

### TC-OHLC-002: Get all for dropdown
**Description**: Simplified list for UI  
**Test Steps**:
1. Call `GET /api/org-units/lookup/dropdown`
**Expected Result**: ID/name pairs

### TC-OHLC-003: Get hierarchy tree
**Description**: Full tree structure  
**Test Steps**:
1. Call `GET /api/org-units/lookup/tree`
**Expected Result**: Nested tree

### TC-OHLC-004: Get user's accessible units
**Description**: User's permitted org units  
**Test Steps**:
1. Call `GET /api/org-units/lookup/my-units`
**Expected Result**: User's org units

### TC-OHLC-005: Get by ID - not found
**Description**: Non-existing ID  
**Test Steps**:
1. Call with invalid ID
**Expected Result**: 404 Not Found

### TC-OHLC-006: Get root org units
**Description**: Top-level units only  
**Test Steps**:
1. Call `GET /api/org-units/lookup/roots`
**Expected Result**: Root units

### TC-OHLC-007: Get children of unit
**Description**: Direct children  
**Test Steps**:
1. Call `GET /api/org-units/lookup/{id}/children`
**Expected Result**: Child units

### TC-OHLC-008: Get ancestors of unit
**Description**: Path to root  
**Test Steps**:
1. Call `GET /api/org-units/lookup/{id}/ancestors`
**Expected Result**: Ancestor chain

---

## P1 - High Priority Tests

### TC-OHLC-009: Get descendants of unit
**Description**: All nested units  
**Test Steps**:
1. Call `GET /api/org-units/lookup/{id}/descendants`
**Expected Result**: All descendants

### TC-OHLC-010: Get siblings of unit
**Description**: Same-level units  
**Test Steps**:
1. Call `GET /api/org-units/lookup/{id}/siblings`
**Expected Result**: Sibling units

### TC-OHLC-011: Typeahead search
**Description**: Quick search  
**Test Steps**:
1. Call `GET /api/org-units/lookup/typeahead?q=HQ`
**Expected Result**: Matching units

### TC-OHLC-012: Filter by type
**Description**: Filter by unit type  
**Test Steps**:
1. Call with type=Division
**Expected Result**: Filtered units

### TC-OHLC-013: Filter by status
**Description**: Active only  
**Test Steps**:
1. Call with status=active
**Expected Result**: Active units

### TC-OHLC-014: Tree depth limit
**Description**: Limit tree depth  
**Test Steps**:
1. Call with maxDepth=2
**Expected Result**: Limited depth

### TC-OHLC-015: Get unit path
**Description**: Breadcrumb path  
**Test Steps**:
1. Call `GET /api/org-units/lookup/{id}/path`
**Expected Result**: Path string

### TC-OHLC-016: Search in subtree
**Description**: Search within branch  
**Test Steps**:
1. Call with rootId and search
**Expected Result**: Scoped results

---

## Authorization Tests

### TC-OHLC-A001: Unauthenticated denied
**Expected Result**: 401 Unauthorized

### TC-OHLC-A002: User sees only accessible units
**Expected Result**: Filtered by permission

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/OrganizationHierarchyLookupControllerTests.cs`

