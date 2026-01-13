# PartnerLiaisonOfficeManager Test Cases

**Manager**: `UNOPS.PAO.UNOPSBusiness/Managers/PartnerLiaisonOfficeManager.cs`  
**Entity**: `UNOPS.PAO.Domain/Entities/PartnerLiaisonOffice.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 30  

---

## Overview

The PartnerLiaisonOfficeManager manages partner-liaison office associations:
- Link partners to liaison offices
- Track primary/secondary associations
- Manage assignment history
- Support geographic filtering

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 10 | P0 |
| Associations | 8 | P1 |
| Primary Office | 6 | P0 |
| History | 4 | P2 |
| Validation | 2 | P0 |

---

## P0 - Critical Tests

### TC-PLO-001: Create partner-office link
**Description**: Associate partner with liaison office  
**Test Steps**:
1. Call `CreateAsync(partnerId, liaisonOfficeId)`
**Expected Result**: Association created

### TC-PLO-002: Get by partner ID
**Description**: Get offices for partner  
**Test Steps**:
1. Create associations
2. Call `GetByPartnerIdAsync(partnerId)`
**Expected Result**: Office list

### TC-PLO-003: Get by liaison office ID
**Description**: Get partners for office  
**Test Steps**:
1. Create associations
2. Call `GetByLiaisonOfficeIdAsync(officeId)`
**Expected Result**: Partner list

### TC-PLO-004: Delete association
**Description**: Remove link  
**Test Steps**:
1. Create association
2. Call `DeleteAsync(id)`
**Expected Result**: Association removed

### TC-PLO-005: Set primary office
**Description**: Mark as primary  
**Test Steps**:
1. Create associations
2. Call `SetPrimaryAsync(partnerId, officeId)`
**Expected Result**: Marked as primary

### TC-PLO-006: Get primary office
**Description**: Get partner's primary  
**Test Steps**:
1. Set primary
2. Call `GetPrimaryAsync(partnerId)`
**Expected Result**: Primary office

### TC-PLO-007: Only one primary
**Description**: Setting new primary clears old  
**Test Steps**:
1. Set primary A
2. Set primary B
**Expected Result**: Only B is primary

### TC-PLO-008: Partner required
**Description**: Partner ID validation  
**Test Steps**:
1. Create without partner
**Expected Result**: Validation error

### TC-PLO-009: Office required
**Description**: Office ID validation  
**Test Steps**:
1. Create without office
**Expected Result**: Validation error

### TC-PLO-010: Prevent duplicate
**Description**: Same partner-office pair  
**Test Steps**:
1. Create association
2. Create same again
**Expected Result**: Conflict error

---

## P1 - High Priority Tests

### TC-PLO-011: Update association
**Description**: Update assignment details  
**Test Steps**:
1. Create association
2. Call `UpdateAsync(id, data)`
**Expected Result**: Association updated

### TC-PLO-012: Add assignment note
**Description**: Add notes to assignment  
**Test Steps**:
1. Update with notes
**Expected Result**: Notes saved

### TC-PLO-013: Set assignment date
**Description**: Track when assigned  
**Test Steps**:
1. Create with date
**Expected Result**: Date recorded

### TC-PLO-014: Get all for org unit
**Description**: Filter by org unit  
**Test Steps**:
1. Get with orgUnitId filter
**Expected Result**: Filtered results

### TC-PLO-015: Get active associations
**Description**: Exclude inactive  
**Test Steps**:
1. Create active and inactive
2. Get active only
**Expected Result**: Only active

### TC-PLO-016: Count by office
**Description**: Partner count per office  
**Test Steps**:
1. Create multiple
2. Get count
**Expected Result**: Correct counts

### TC-PLO-017: Count by partner
**Description**: Office count per partner  
**Test Steps**:
1. Create multiple
2. Get count
**Expected Result**: Correct counts

### TC-PLO-018: Bulk associate
**Description**: Add multiple at once  
**Test Steps**:
1. Call bulk create
**Expected Result**: All created

---

## P2 - Medium Priority Tests

### TC-PLO-019: Get assignment history
**Description**: Historical assignments  
**Test Steps**:
1. Create, update, delete
2. Get history
**Expected Result**: History list

### TC-PLO-020: Track who assigned
**Description**: Audit trail  
**Test Steps**:
1. Create association
2. Check createdBy
**Expected Result**: User ID recorded

### TC-PLO-021: Track when assigned
**Description**: Timestamp audit  
**Test Steps**:
1. Create association
2. Check createdDate
**Expected Result**: Timestamp recorded

### TC-PLO-022: Export associations
**Description**: Export to CSV  
**Test Steps**:
1. Call export
**Expected Result**: CSV data

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Managers/PartnerLiaisonOfficeManagerTests.cs`

