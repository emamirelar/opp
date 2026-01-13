# CommonEntitiesManager - Unit Test Cases

**Manager**: `CommonEntitiesManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/CommonEntitiesManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: Shared entity operations, Common lookups, Entity metadata, Cross-entity queries

**Total Test Cases**: 15+

---

## Test Categories

### 1. Entity Lookups (5 tests)
- TC-CE-001: Get entity by ID
- TC-CE-002: Search entities
- TC-CE-003: Filter entities
- TC-CE-004: Sort entities
- TC-CE-005: Paginate entities

### 2. Metadata Operations (4 tests)
- TC-CE-006: Get entity metadata
- TC-CE-007: Update entity metadata
- TC-CE-008: Metadata validation
- TC-CE-009: Metadata inheritance

### 3. Common Queries (3 tests)
- TC-CE-010: Get active entities
- TC-CE-011: Get deleted entities
- TC-CE-012: Audit trail query

### 4. Bulk Operations (3 tests)
- TC-CE-013: Bulk update
- TC-CE-014: Bulk delete
- TC-CE-015: Bulk status change

**Coverage**: 70%+

