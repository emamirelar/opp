# BaseEngagementManager - Unit Test Cases

**Manager**: `BaseEngagementManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/BaseEngagementManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: Base engagement operations, Common engagement logic, Workflow integration, Status management

**Total Test Cases**: 20+

---

## Test Categories

### 1. Engagement CRUD (5 tests)
- TC-BE-001: Create engagement
- TC-BE-002: Get engagement
- TC-BE-003: Update engagement
- TC-BE-004: Delete engagement
- TC-BE-005: List engagements

### 2. Workflow Operations (5 tests)
- TC-BE-006: Submit engagement
- TC-BE-007: Approve engagement
- TC-BE-008: Reject engagement
- TC-BE-009: Withdraw engagement
- TC-BE-010: Workflow validation

### 3. Status Management (4 tests)
- TC-BE-011: Update status
- TC-BE-012: Get status history
- TC-BE-013: Status transitions
- TC-BE-014: Invalid status change

### 4. Permissions (3 tests)
- TC-BE-015: Check edit permission
- TC-BE-016: Check view permission
- TC-BE-017: Role-based access

### 5. Integration (3 tests)
- TC-BE-018: Link to partner
- TC-BE-019: Add participants
- TC-BE-020: Engagement notifications

**Coverage**: 70%+

