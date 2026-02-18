# OrganizationHierarchyManager - Unit Test Cases

**Manager**: `OrganizationHierarchyManager`  
**File**: `UNOPS.PAO.Business/Managers/OrganizationHierarchyManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: Org unit hierarchy, Parent-child relationships, Tree operations, Access control

**Total Test Cases**: 15+

---

## Test Categories

### 1. Hierarchy Management (5 tests)
- TC-OH-001: Build org hierarchy
- TC-OH-002: Get org unit children
- TC-OH-003: Get org unit ancestors
- TC-OH-004: Move org unit
- TC-OH-005: Prevent circular references

### 2. Access Control (4 tests)
- TC-OH-006: Get user accessible units
- TC-OH-007: Check unit access
- TC-OH-008: Inherit permissions
- TC-OH-009: Filter by access rights

### 3. Tree Operations (3 tests)
- TC-OH-010: Flatten hierarchy
- TC-OH-011: Get tree depth
- TC-OH-012: Find unit by path

### 4. Unit Metadata (3 tests)
- TC-OH-013: Get unit details
- TC-OH-014: Update unit metadata
- TC-OH-015: Unit user count

**Coverage**: 70%+

