# LiaisonOfficeController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/LiaisonOffices/LiaisonOfficeController.cs`  
**Priority**: P0 - Critical  
**Total Test Cases**: 30  

---

## Overview

The LiaisonOfficeController manages liaison offices for partner relationships:
- Liaison office CRUD operations
- Partner-liaison office associations
- Office search and filtering
- Geographic assignments

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 10 | P0 |
| Search & Filter | 8 | P1 |
| Partner Associations | 7 | P0 |
| Authorization | 5 | P0 |

---

## P0 - Critical Tests

### TC-LO-001: Get all liaison offices
**Description**: Retrieve list of all liaison offices  
**Preconditions**: User authenticated with read permission  
**Test Steps**:
1. Call `GET /api/liaison-offices`
2. Verify offices returned with pagination
**Expected Result**: Paginated list of liaison offices

### TC-LO-002: Get liaison office by ID
**Description**: Retrieve specific liaison office  
**Preconditions**: Office exists  
**Test Steps**:
1. Call `GET /api/liaison-offices/{id}`
2. Verify office details
**Expected Result**: Office with all properties and related data

### TC-LO-003: Create liaison office
**Description**: Create new liaison office  
**Preconditions**: User has create permission  
**Test Steps**:
1. Call `POST /api/liaison-offices` with office data
2. Verify office created
**Expected Result**: Office created with generated ID

### TC-LO-004: Create office - duplicate code fails
**Description**: Cannot create office with existing code  
**Preconditions**: Office with code exists  
**Test Steps**:
1. Attempt create with duplicate code
2. Verify error
**Expected Result**: 409 Conflict - duplicate code

### TC-LO-005: Update liaison office
**Description**: Update existing liaison office  
**Preconditions**: Office exists, user has update permission  
**Test Steps**:
1. Call `PUT /api/liaison-offices/{id}` with updated data
2. Verify update
**Expected Result**: Office updated successfully

### TC-LO-006: Delete liaison office
**Description**: Delete (soft) liaison office  
**Preconditions**: Office exists, not linked to partners  
**Test Steps**:
1. Call `DELETE /api/liaison-offices/{id}`
2. Verify soft delete
**Expected Result**: Office marked as deleted

### TC-LO-007: Delete office with partners fails
**Description**: Cannot delete office linked to active partners  
**Preconditions**: Office has partner associations  
**Test Steps**:
1. Attempt delete
2. Verify prevented
**Expected Result**: 400 Bad Request - office in use

### TC-LO-008: Get office by code
**Description**: Lookup office by unique code  
**Preconditions**: Office exists  
**Test Steps**:
1. Call `GET /api/liaison-offices/code/{code}`
2. Verify correct office
**Expected Result**: Office matching code

### TC-LO-009: Associate partner with office
**Description**: Link partner to liaison office  
**Preconditions**: Partner and office exist  
**Test Steps**:
1. Call `POST /api/liaison-offices/{officeId}/partners/{partnerId}`
2. Verify association
**Expected Result**: Partner linked to office

### TC-LO-010: Remove partner from office
**Description**: Unlink partner from liaison office  
**Preconditions**: Association exists  
**Test Steps**:
1. Call `DELETE /api/liaison-offices/{officeId}/partners/{partnerId}`
2. Verify removal
**Expected Result**: Association removed

### TC-LO-011: Get partners by office
**Description**: Get all partners for a liaison office  
**Preconditions**: Office has partners  
**Test Steps**:
1. Call `GET /api/liaison-offices/{officeId}/partners`
2. Verify partner list
**Expected Result**: All linked partners returned

### TC-LO-012: Get office by partner
**Description**: Get liaison office for a partner  
**Preconditions**: Partner has office assigned  
**Test Steps**:
1. Call `GET /api/partners/{partnerId}/liaison-office`
2. Verify office
**Expected Result**: Partner's liaison office returned

---

## P1 - High Priority Tests

### TC-LO-013: Search offices by name
**Description**: Search offices with name filter  
**Preconditions**: Multiple offices exist  
**Test Steps**:
1. Call `GET /api/liaison-offices?search=Regional`
2. Verify matching results
**Expected Result**: Offices matching search term

### TC-LO-014: Filter offices by country
**Description**: Filter offices by country code  
**Preconditions**: Offices in multiple countries  
**Test Steps**:
1. Call `GET /api/liaison-offices?country=KE`
2. Verify filtered results
**Expected Result**: Only Kenya offices returned

### TC-LO-015: Filter offices by region
**Description**: Filter offices by geographic region  
**Preconditions**: Offices in multiple regions  
**Test Steps**:
1. Call `GET /api/liaison-offices?region=Africa`
2. Verify filtered results
**Expected Result**: Only Africa region offices

### TC-LO-016: Paginate office results
**Description**: Pagination works correctly  
**Preconditions**: Many offices exist  
**Test Steps**:
1. Call with pagination params
2. Verify pagination metadata
**Expected Result**: Correct page returned with metadata

### TC-LO-017: Sort offices
**Description**: Sort offices by various fields  
**Preconditions**: Multiple offices  
**Test Steps**:
1. Call with sort parameters
2. Verify sort order
**Expected Result**: Offices sorted correctly

### TC-LO-018: Get offices by org unit
**Description**: Get offices in specific org unit  
**Preconditions**: Offices assigned to org units  
**Test Steps**:
1. Call `GET /api/liaison-offices?orgUnitId=123`
2. Verify filtered results
**Expected Result**: Only org unit offices returned

### TC-LO-019: Typeahead search
**Description**: Typeahead suggestions for office selection  
**Preconditions**: Offices exist  
**Test Steps**:
1. Call `GET /api/liaison-offices/typeahead?q=Nai`
2. Verify suggestions
**Expected Result**: Offices starting with "Nai"

### TC-LO-020: Export offices
**Description**: Export office list to CSV/Excel  
**Preconditions**: Export permission  
**Test Steps**:
1. Call `GET /api/liaison-offices/export`
2. Verify export file
**Expected Result**: Valid export file returned

---

## Authorization Tests

### TC-LO-A001: Unauthorized user denied
**Description**: Unauthenticated request fails  
**Test Steps**:
1. Call without auth token
**Expected Result**: 401 Unauthorized

### TC-LO-A002: User without permission denied
**Description**: User lacking office permission fails  
**Test Steps**:
1. Authenticate without office permission
2. Call office endpoints
**Expected Result**: 403 Forbidden

### TC-LO-A003: Org unit filter applied
**Description**: User sees only permitted offices  
**Preconditions**: User has org unit restrictions  
**Test Steps**:
1. Query offices
2. Verify only permitted offices returned
**Expected Result**: Results filtered by org unit

### TC-LO-A004: Read-only user cannot update
**Description**: User with read-only access cannot modify  
**Preconditions**: User has read permission only  
**Test Steps**:
1. Attempt update/delete
**Expected Result**: 403 Forbidden

### TC-LO-A005: Admin sees all offices
**Description**: Admin bypasses org unit filter  
**Preconditions**: User is admin  
**Test Steps**:
1. Query offices as admin
2. Verify all offices visible
**Expected Result**: All offices returned

---

## Validation Tests

### TC-LO-V001: Office code format validation
**Description**: Code must match required format  
**Test Steps**:
1. Create with invalid code format
**Expected Result**: 400 Bad Request

### TC-LO-V002: Required fields validation
**Description**: Required fields must be provided  
**Test Steps**:
1. Create without required fields
**Expected Result**: 400 Bad Request with field errors

### TC-LO-V003: Country code validation
**Description**: Country code must be valid ISO code  
**Test Steps**:
1. Create with invalid country
**Expected Result**: 400 Bad Request - invalid country

### TC-LO-V004: Contact email validation
**Description**: Email format validated  
**Test Steps**:
1. Create with invalid email
**Expected Result**: 400 Bad Request - invalid email

### TC-LO-V005: Phone number validation
**Description**: Phone format validated  
**Test Steps**:
1. Create with invalid phone
**Expected Result**: 400 Bad Request - invalid phone

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/LiaisonOfficeControllerTests.cs`

