# OrganizationHierarchyManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `OrganizationHierarchyManager`  
**Location**: `UNOPS.PAO.Business/Managers/OrganizationHierarchyManager.cs`  
**Purpose**: Manages organization unit hierarchy for UNOPS organizational structure.

---

## Functional Test Cases (20+ Cases)

### TC-OHM-F001-020: Core Operations
- F001: Get organization hierarchy - returns tree structure
- F002: Get organization hierarchy - empty database returns empty
- F003: Get organization hierarchy - single root node
- F004: Get organization hierarchy - multiple root nodes
- F005: Get organization hierarchy - nested children
- F006: Get organization hierarchy prime - returns prime format
- F007: Get organization hierarchy prime - maps to PrimeModel
- F008: Get organization hierarchy by ID - existing organization
- F009: Get organization hierarchy by ID - non-existent returns null/default
- F010: Get organizations by type - OrgUnit type
- F011: Get organizations by type - Hub type
- F012: Get organizations by type - Region type
- F013: Get organizations by type - returns empty for no matches
- F014: Get all organizations - returns all organizations
- F015: Get all organizations - empty database returns empty
- F016: Get all organizations - large dataset mapping
- F017: Organization hierarchy - parent-child relationships correct
- F018: Organization hierarchy - depth levels correct
- F019: Organization hierarchy prime - sorted correctly
- F020: AutoMapper mapping - all properties mapped correctly

---

## Performance Test Cases (10 Cases)

### TC-OHM-P001: Get Organization Hierarchy - Response Time
**Performance Criteria**: < 500ms for full hierarchy

### TC-OHM-P002: Get Organization Hierarchy - Large Organization
**Performance Criteria**: < 1000ms for 500 org units

### TC-OHM-P003: Get Organization Hierarchy Prime - Response Time
**Performance Criteria**: < 400ms for prime format

### TC-OHM-P004: Get By ID - Response Time
**Performance Criteria**: < 100ms per lookup

### TC-OHM-P005: Get Organizations by Type - Response Time
**Performance Criteria**: < 200ms with type filter

### TC-OHM-P006: Get All Organizations - Response Time
**Performance Criteria**: < 500ms for all organizations

### TC-OHM-P007: Hierarchy Tree Building
**Performance Criteria**: < 300ms tree construction

### TC-OHM-P008: Concurrent Hierarchy Access
**Performance Criteria**: 20 concurrent reads < 600ms each

### TC-OHM-P009: AutoMapper Performance
**Performance Criteria**: < 50ms for 100 entity mappings

### TC-OHM-P010: Repository Query Performance
**Performance Criteria**: < 200ms for ValuesRepository calls

---

## Concurrency Test Cases (10 Cases)

### TC-OHM-C001: Concurrent Get Hierarchy
**Scenario**: 20 threads requesting full hierarchy

### TC-OHM-C002: Concurrent Get Hierarchy Prime
**Scenario**: 15 threads requesting prime format

### TC-OHM-C003: Concurrent Get By ID - Same ID
**Scenario**: 10 threads fetching same organization

### TC-OHM-C004: Concurrent Get By ID - Different IDs
**Scenario**: 20 threads fetching different organizations

### TC-OHM-C005: Concurrent Get By Type
**Scenario**: 15 threads filtering by different types

### TC-OHM-C006: Concurrent Get All Organizations
**Scenario**: 10 threads getting all organizations

### TC-OHM-C007: Hierarchy Read During Update
**Scenario**: Reading hierarchy while org structure changes

### TC-OHM-C008: ValuesRepository Thread Safety
**Scenario**: Verify repository is thread-safe

### TC-OHM-C009: Mapper Concurrent Usage
**Scenario**: Concurrent AutoMapper operations

### TC-OHM-C010: High Load Scenario
**Scenario**: 50 concurrent hierarchy operations

---

## Edge Cases (5 Cases)

### TC-OHM-E001: Organization with Self-Reference Parent
### TC-OHM-E002: Circular Hierarchy Reference
### TC-OHM-E003: Organization Type Enum Invalid Value
### TC-OHM-E004: Very Deep Hierarchy (20+ levels)
### TC-OHM-E005: Organization Name with Special Characters

