# PartnerFocalPointManager Test Cases

**Manager**: `UNOPS.PAO.UNOPSBusiness/Managers/PartnerFocalPointManager.cs`  
**Entity**: `UNOPS.PAO.Domain/Entities/PartnerFocalPoint.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 28  

---

## Overview

The PartnerFocalPointManager manages focal point assignments:
- Assign users as partner focal points
- Track primary/secondary assignments
- Manage responsibility areas
- Handle delegation and handover

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 8 | P0 |
| Assignments | 8 | P1 |
| Primary Focal Point | 6 | P0 |
| Delegation | 4 | P2 |
| Validation | 2 | P0 |

---

## P0 - Critical Tests

### TC-PFP-001: Create focal point
**Description**: Assign user as focal point  
**Test Steps**:
1. Call `CreateAsync(partnerId, userId)`
**Expected Result**: Focal point created

### TC-PFP-002: Get by partner ID
**Description**: Get focal points for partner  
**Test Steps**:
1. Create focal points
2. Call `GetByPartnerIdAsync(partnerId)`
**Expected Result**: Focal point list

### TC-PFP-003: Get by user ID
**Description**: Get partners for user  
**Test Steps**:
1. Create assignments
2. Call `GetByUserIdAsync(userId)`
**Expected Result**: Partner list

### TC-PFP-004: Delete assignment
**Description**: Remove focal point  
**Test Steps**:
1. Create assignment
2. Call `DeleteAsync(id)`
**Expected Result**: Assignment removed

### TC-PFP-005: Set primary focal point
**Description**: Mark as primary  
**Test Steps**:
1. Create assignments
2. Call `SetPrimaryAsync(partnerId, userId)`
**Expected Result**: Marked as primary

### TC-PFP-006: Get primary focal point
**Description**: Get partner's primary  
**Test Steps**:
1. Set primary
2. Call `GetPrimaryAsync(partnerId)`
**Expected Result**: Primary focal point

### TC-PFP-007: Partner required
**Description**: Partner ID validation  
**Test Steps**:
1. Create without partner
**Expected Result**: Validation error

### TC-PFP-008: User required
**Description**: User ID validation  
**Test Steps**:
1. Create without user
**Expected Result**: Validation error

---

## P1 - High Priority Tests

### TC-PFP-009: Only one primary
**Description**: Single primary per partner  
**Test Steps**:
1. Set primary A
2. Set primary B
**Expected Result**: Only B is primary

### TC-PFP-010: Update responsibility
**Description**: Update responsibility area  
**Test Steps**:
1. Create assignment
2. Update responsibility
**Expected Result**: Updated

### TC-PFP-011: Set start date
**Description**: Assignment start date  
**Test Steps**:
1. Create with startDate
**Expected Result**: Date recorded

### TC-PFP-012: Set end date
**Description**: Assignment end date  
**Test Steps**:
1. Update with endDate
**Expected Result**: Date recorded

### TC-PFP-013: Get active assignments
**Description**: Current assignments only  
**Test Steps**:
1. Create current and past
2. Get active
**Expected Result**: Only current

### TC-PFP-014: Get by org unit
**Description**: Filter by org unit  
**Test Steps**:
1. Create in different orgs
2. Filter by org
**Expected Result**: Filtered

### TC-PFP-015: Prevent duplicate
**Description**: Same partner-user pair  
**Test Steps**:
1. Create assignment
2. Create duplicate
**Expected Result**: Conflict error

### TC-PFP-016: Count assignments
**Description**: Workload tracking  
**Test Steps**:
1. Get count by user
**Expected Result**: Assignment count

---

## P2 - Medium Priority Tests

### TC-PFP-017: Delegate to user
**Description**: Temporary delegation  
**Test Steps**:
1. Call `DelegateAsync(fromUserId, toUserId, period)`
**Expected Result**: Delegation created

### TC-PFP-018: End delegation
**Description**: Cancel delegation  
**Test Steps**:
1. Create delegation
2. Call `EndDelegationAsync(id)`
**Expected Result**: Delegation ended

### TC-PFP-019: Handover to new user
**Description**: Permanent transfer  
**Test Steps**:
1. Call `HandoverAsync(fromUserId, toUserId)`
**Expected Result**: All transferred

### TC-PFP-020: Get delegation history
**Description**: Delegation audit  
**Test Steps**:
1. Make delegations
2. Get history
**Expected Result**: History list

### TC-PFP-021: Notify on assignment
**Description**: Email notification  
**Test Steps**:
1. Create assignment
**Expected Result**: Notification sent

### TC-PFP-022: Notify on delegation
**Description**: Delegation notification  
**Test Steps**:
1. Create delegation
**Expected Result**: Both notified

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Managers/PartnerFocalPointManagerTests.cs`

