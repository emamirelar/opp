# PartnerCategoryController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/PartnerTrees/PartnerCategoryController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 22  

---

## Overview

The PartnerCategoryController manages partner categories:
- Category CRUD operations
- Category hierarchies
- Partner-category associations
- Category-based filtering

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 8 | P0 |
| Hierarchy | 6 | P1 |
| Associations | 5 | P1 |
| Authorization | 3 | P0 |

---

## P0 - Critical Tests

### TC-PCC-001: Get all categories
**Description**: List all categories  
**Test Steps**:
1. Call `GET /api/partner-categories`
**Expected Result**: Category list

### TC-PCC-002: Get category by ID
**Description**: Retrieve specific category  
**Test Steps**:
1. Call `GET /api/partner-categories/{id}`
**Expected Result**: Category details

### TC-PCC-003: Create category
**Description**: Create new category  
**Test Steps**:
1. Call `POST /api/partner-categories`
**Expected Result**: Category created

### TC-PCC-004: Update category
**Description**: Update existing category  
**Test Steps**:
1. Call `PUT /api/partner-categories/{id}`
**Expected Result**: Category updated

### TC-PCC-005: Delete category
**Description**: Remove category  
**Test Steps**:
1. Call `DELETE /api/partner-categories/{id}`
**Expected Result**: Category deleted

### TC-PCC-006: Get by ID - not found
**Description**: Non-existing ID  
**Test Steps**:
1. Call with invalid ID
**Expected Result**: 404 Not Found

### TC-PCC-007: Create - validation
**Description**: Validate required fields  
**Test Steps**:
1. Submit without name
**Expected Result**: Validation error

### TC-PCC-008: Delete - with partners
**Description**: Can't delete if partners assigned  
**Test Steps**:
1. Delete category with partners
**Expected Result**: Conflict error

---

## P1 - High Priority Tests

### TC-PCC-009: Get category tree
**Description**: Hierarchical view  
**Test Steps**:
1. Call `GET /api/partner-categories/tree`
**Expected Result**: Nested structure

### TC-PCC-010: Get root categories
**Description**: Top-level only  
**Test Steps**:
1. Call `GET /api/partner-categories/roots`
**Expected Result**: Root categories

### TC-PCC-011: Get children
**Description**: Child categories  
**Test Steps**:
1. Call `GET /api/partner-categories/{id}/children`
**Expected Result**: Child list

### TC-PCC-012: Move category
**Description**: Change parent  
**Test Steps**:
1. Call `PUT /api/partner-categories/{id}/move`
**Expected Result**: Parent changed

### TC-PCC-013: Prevent circular reference
**Description**: Can't set child as parent  
**Test Steps**:
1. Try to set descendant as parent
**Expected Result**: Validation error

### TC-PCC-014: Get category path
**Description**: Breadcrumb path  
**Test Steps**:
1. Call `GET /api/partner-categories/{id}/path`
**Expected Result**: Path string

### TC-PCC-015: Get partners in category
**Description**: List associated partners  
**Test Steps**:
1. Call `GET /api/partner-categories/{id}/partners`
**Expected Result**: Partner list

### TC-PCC-016: Add partner to category
**Description**: Associate partner  
**Test Steps**:
1. Call `POST /api/partner-categories/{id}/partners/{partnerId}`
**Expected Result**: Association created

### TC-PCC-017: Remove partner from category
**Description**: Remove association  
**Test Steps**:
1. Call `DELETE /api/partner-categories/{id}/partners/{partnerId}`
**Expected Result**: Association removed

### TC-PCC-018: Get for dropdown
**Description**: Simplified list  
**Test Steps**:
1. Call `GET /api/partner-categories/dropdown`
**Expected Result**: ID/name pairs

### TC-PCC-019: Search categories
**Description**: Search by name  
**Test Steps**:
1. Call with search parameter
**Expected Result**: Matching categories

---

## Authorization Tests

### TC-PCC-A001: Read requires auth
**Expected Result**: 401 without auth

### TC-PCC-A002: Write requires admin
**Expected Result**: 403 for non-admin

### TC-PCC-A003: Delete requires admin
**Expected Result**: 403 for non-admin

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/PartnerCategoryControllerTests.cs`

