# OrganizationHierarchyLookupService Test Cases

**Service**: `UNOPS.PAO.Business/Services/OrganizationHierarchyLookupService.cs`  
**Priority**: P0 - Critical  
**Total Test Cases**: 20  

---

## Overview

The OrganizationHierarchyLookupService provides lookup operations for organization units:
- Org unit search and filtering
- Hierarchy traversal
- Cached lookups for performance
- Type-ahead suggestions

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Lookup Operations | 8 | P0 |
| Hierarchy Traversal | 5 | P0 |
| Caching | 4 | P1 |
| Performance | 3 | P1 |

---

## P0 - Critical Tests

### TC-OHLS-001: Get org unit by ID
**Description**: Retrieve organization unit by its ID  
**Preconditions**: Org unit exists in database  
**Test Steps**:
1. Create test org unit
2. Call `GetByIdAsync(orgUnitId)`
3. Verify correct org unit returned
**Expected Result**: Org unit with matching ID returned

### TC-OHLS-002: Get org unit by code
**Description**: Retrieve organization unit by code  
**Preconditions**: Org unit with unique code exists  
**Test Steps**:
1. Create org unit with code "HQ001"
2. Call `GetByCodeAsync("HQ001")`
3. Verify correct org unit returned
**Expected Result**: Org unit with matching code returned

### TC-OHLS-003: Search org units by name
**Description**: Search org units by partial name match  
**Preconditions**: Multiple org units exist  
**Test Steps**:
1. Create org units: "Headquarters", "Regional Office", "Field Office"
2. Call `SearchAsync("Office")`
3. Verify matching org units returned
**Expected Result**: Returns "Regional Office" and "Field Office"

### TC-OHLS-004: Search with no results
**Description**: Search returns empty when no matches  
**Preconditions**: Org units exist but none match query  
**Test Steps**:
1. Create org units
2. Call `SearchAsync("NonExistent")`
3. Verify empty result
**Expected Result**: Empty collection returned, no exception

### TC-OHLS-005: Get all children of org unit
**Description**: Get direct children of an org unit  
**Preconditions**: Parent org unit with children exists  
**Test Steps**:
1. Create parent with 3 child org units
2. Call `GetChildrenAsync(parentId)`
3. Verify all 3 children returned
**Expected Result**: All direct children returned

### TC-OHLS-006: Get all descendants of org unit
**Description**: Get all descendants recursively  
**Preconditions**: Multi-level hierarchy exists  
**Test Steps**:
1. Create 3-level hierarchy: Parent > Child > Grandchild
2. Call `GetDescendantsAsync(parentId)`
3. Verify both child and grandchild returned
**Expected Result**: All descendants at all levels returned

### TC-OHLS-007: Get ancestors of org unit
**Description**: Get parent chain up to root  
**Preconditions**: Multi-level hierarchy exists  
**Test Steps**:
1. Create 4-level hierarchy
2. Call `GetAncestorsAsync(deepestChildId)`
3. Verify all ancestors returned in order
**Expected Result**: Ancestors returned from immediate parent to root

### TC-OHLS-008: Get org units by type
**Description**: Filter org units by type  
**Preconditions**: Org units of different types exist  
**Test Steps**:
1. Create org units: type "Region", "Country", "Office"
2. Call `GetByTypeAsync("Region")`
3. Verify only region types returned
**Expected Result**: Only org units of specified type returned

### TC-OHLS-009: Get root org units
**Description**: Get all top-level org units  
**Preconditions**: Multiple root and child org units exist  
**Test Steps**:
1. Create 2 root org units, each with children
2. Call `GetRootOrgUnitsAsync()`
3. Verify only roots returned
**Expected Result**: Only org units with no parent returned

### TC-OHLS-010: Get org unit path
**Description**: Get full path from root to org unit  
**Preconditions**: Nested org unit exists  
**Test Steps**:
1. Create hierarchy: "UNOPS > Regional > Country > Office"
2. Call `GetPathAsync(officeId)`
3. Verify full path returned
**Expected Result**: Path array: ["UNOPS", "Regional", "Country", "Office"]

### TC-OHLS-011: Get org units by IDs (batch)
**Description**: Retrieve multiple org units by IDs  
**Preconditions**: Multiple org units exist  
**Test Steps**:
1. Create 5 org units
2. Call `GetByIdsAsync([id1, id2, id3])`
3. Verify 3 matching org units returned
**Expected Result**: Correct org units returned for all valid IDs

### TC-OHLS-012: Handle invalid org unit ID
**Description**: Gracefully handle non-existent ID  
**Preconditions**: ID does not exist  
**Test Steps**:
1. Call `GetByIdAsync(99999)`
2. Verify null or appropriate response
**Expected Result**: Returns null, does not throw exception

### TC-OHLS-013: Check org unit exists
**Description**: Check if org unit exists by ID  
**Preconditions**: Mix of existing and non-existing IDs  
**Test Steps**:
1. Create org unit
2. Call `ExistsAsync(existingId)` - should return true
3. Call `ExistsAsync(nonExistingId)` - should return false
**Expected Result**: Correct boolean returned

---

## P1 - High Priority Tests

### TC-OHLS-014: Cached lookup returns same result
**Description**: Verify caching works correctly  
**Preconditions**: Caching enabled  
**Test Steps**:
1. Call `GetByIdAsync(orgUnitId)` - first call
2. Call `GetByIdAsync(orgUnitId)` - second call
3. Verify second call uses cache (faster, same result)
**Expected Result**: Second call significantly faster, same result

### TC-OHLS-015: Cache invalidation on update
**Description**: Cache invalidated when org unit updated  
**Preconditions**: Org unit cached  
**Test Steps**:
1. Call `GetByIdAsync(orgUnitId)` to cache
2. Update org unit name
3. Call `GetByIdAsync(orgUnitId)` again
4. Verify new name returned
**Expected Result**: Updated data returned after modification

### TC-OHLS-016: Typeahead search
**Description**: Typeahead suggestions as user types  
**Preconditions**: Multiple org units exist  
**Test Steps**:
1. Create org units: "Regional Office", "Regional Hub", "Remote Site"
2. Call `TypeaheadAsync("Reg", limit: 5)`
3. Verify matching suggestions
**Expected Result**: Returns "Regional Office" and "Regional Hub"

### TC-OHLS-017: Typeahead with minimum characters
**Description**: Typeahead requires minimum input  
**Preconditions**: Typeahead minimum set to 2  
**Test Steps**:
1. Call `TypeaheadAsync("R", limit: 5)`
2. Verify no results (too short)
**Expected Result**: Empty result or appropriate error

---

## Performance Tests

### TC-OHLS-P001: Search 10,000 org units
**Description**: Search performance with large dataset  
**Preconditions**: 10,000 org units seeded  
**Performance Criteria**: Search completes in < 500ms

### TC-OHLS-P002: Get descendants of deep hierarchy
**Description**: Traverse 10-level deep hierarchy  
**Preconditions**: 10-level hierarchy exists  
**Performance Criteria**: Complete in < 200ms

### TC-OHLS-P003: Batch lookup 100 org units
**Description**: Retrieve 100 org units by ID  
**Preconditions**: 100 org unit IDs  
**Performance Criteria**: Complete in < 300ms

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/OrganizationHierarchyLookupServiceTests.cs`

