# OrganizationHierarchyManager - Business Logic Test Cases

## Manager Overview
**Manager**: `OrganizationHierarchyManager`  
**Location**: `UNOPS.PAO.Business/Managers/OrganizationHierarchyManager.cs`  
**Purpose**: Manages organizational unit hierarchy including regions, hubs, and organizational units.

## Key Business Rules (From PRD)

1. **Hierarchy Types**: Region → Hub → OrgUnit (organizational unit)
2. **Parent-Child Relationships**: Units have parent references creating tree structure
3. **Entity Filtering**: Only OrgUnit type can have entity relationships (Partners, etc.)
4. **Code Uniqueness**: Each org unit has unique code
5. **User Permissions**: Users see data filtered by their org unit access
6. **Recursive Queries**: Can traverse hierarchy up (ancestors) or down (descendants)

---

## P0 - Critical Business Logic Tests

### TC-OHM-BL-P0-001: Create Organization Unit - Valid OrgUnit
**Priority**: P0 - Critical  
**Description**: Verify OrgUnit creation with parent  
**Business Rule**: OrgUnits must have parent (Hub or Region)  
**Preconditions**: Parent Hub exists

**Test Steps**:
1. Create OrganizationHierarchy with Type = OrgUnit
2. Set ParentId to existing Hub
3. Set unique Code and Name
4. Verify creation successful

**Expected Result**: OrgUnit created under Hub  
**Business Impact**: Organizational structure management

---

### TC-OHM-BL-P0-002: Create Organization Unit - Code Uniqueness
**Priority**: P0 - Critical  
**Description**: Verify code uniqueness enforced  
**Business Rule**: Code must be unique across all units  
**Preconditions**: Unit with Code "OU001" exists

**Test Steps**:
1. Attempt create with Code "OU001"
2. Verify rejected with duplicate error
3. Create with Code "OU002" - should succeed

**Expected Result**: Duplicate codes rejected  
**Business Impact**: Data integrity

---

### TC-OHM-BL-P0-003: Get Organization by Type - Filter by Type
**Priority**: P0 - Critical  
**Description**: Verify type-based filtering  
**Business Rule**: Can filter by Region, Hub, OrgUnit  
**Preconditions**: Mix of all types

**Test Steps**:
1. Query Type = Region
2. Verify only regions returned
3. Query Type = Hub
4. Verify only hubs returned
5. Query Type = OrgUnit
6. Verify only org units returned

**Expected Result**: Correct type filtering  
**Business Impact**: Hierarchy navigation

---

### TC-OHM-BL-P0-004: Get Children - Direct Children Only
**Priority**: P0 - Critical  
**Description**: Verify getting direct children  
**Business Rule**: Returns immediate children, not grandchildren  
**Preconditions**: Region with 3 Hubs, each Hub has 2 OrgUnits

**Test Steps**:
1. Get children of Region
2. Verify 3 Hubs returned
3. Verify OrgUnits NOT included

**Expected Result**: Only direct children returned  
**Business Impact**: Hierarchy traversal accuracy

---

### TC-OHM-BL-P0-005: Get Descendants - Recursive Query
**Priority**: P0 - Critical  
**Description**: Verify recursive descendant retrieval  
**Business Rule**: Returns all descendants at all levels  
**Preconditions**: Region → 3 Hubs → 6 OrgUnits

**Test Steps**:
1. Get all descendants of Region
2. Verify 3 Hubs included
3. Verify all 6 OrgUnits included
4. Verify correct hierarchy preserved

**Expected Result**: All descendants returned  
**Business Impact**: Complete hierarchy access

---

### TC-OHM-BL-P0-006: Get Ancestors - Path to Root
**Priority**: P0 - Critical  
**Description**: Verify getting path from unit to root  
**Business Rule**: Returns all parent units up to root  
**Preconditions**: OrgUnit under Hub under Region

**Test Steps**:
1. Get ancestors of OrgUnit
2. Verify includes Hub
3. Verify includes Region
4. Verify ordered from immediate parent to root

**Expected Result**: Complete ancestor path  
**Business Impact**: Breadcrumb navigation

---

### TC-OHM-BL-P0-007: Update Organization Unit - Change Parent
**Priority**: P0 - Critical  
**Description**: Verify moving unit to different parent  
**Business Rule**: Units can be reorganized  
**Preconditions**: OrgUnit under Hub A

**Test Steps**:
1. Update ParentId to Hub B
2. Verify change saved
3. Verify appears under Hub B
4. Verify removed from Hub A children

**Expected Result**: Unit moved successfully  
**Business Impact**: Organizational restructuring

---

### TC-OHM-BL-P0-008: Type Validation - OrgUnit Entity Relationships
**Priority**: P0 - Critical  
**Description**: Verify only OrgUnits can have entity relationships  
**Business Rule**: Only Type=OrgUnit can be in OrganizationUnitRelationships  
**Preconditions**: Hub and OrgUnit exist

**Test Steps**:
1. Attempt to create partner-Hub relationship - should fail
2. Create partner-OrgUnit relationship - should succeed
3. Attempt region-entity relationship - should fail

**Expected Result**: Only OrgUnit relationships allowed  
**Business Impact**: Data model integrity

---

## P1 - High Priority Business Logic Tests

### TC-OHM-BL-P1-001: Hierarchy Search - By Code
**Priority**: P1 - High  
**Description**: Verify search by code  
**Business Rule**: Units searchable by code  
**Preconditions**: Units with various codes

**Test Steps**:
1. Search for exact code
2. Verify correct unit returned
3. Search partial code
4. Verify matching results

**Expected Result**: Code search works  
**Business Impact**: Quick lookup

---

### TC-OHM-BL-P1-002: Hierarchy Search - By Name
**Priority**: P1 - High  
**Description**: Verify search by name  
**Business Rule**: Units searchable by name  
**Preconditions**: Units with various names

**Test Steps**:
1. Search for name substring
2. Verify matching units returned
3. Verify case handling

**Expected Result**: Name search works  
**Business Impact**: User convenience

---

### TC-OHM-BL-P1-003: Parent Validation - Type Hierarchy
**Priority**: P1 - High  
**Description**: Verify parent type rules  
**Business Rule**: Region→Hub→OrgUnit hierarchy must be respected  
**Preconditions**: None

**Test Steps**:
1. Create Hub with Region parent - should succeed
2. Create Hub with Hub parent - should fail
3. Create OrgUnit with Hub parent - should succeed
4. Create OrgUnit with Region parent - verify business rule

**Expected Result**: Type hierarchy enforced  
**Business Impact**: Valid structure

---

### TC-OHM-BL-P1-004: Root Units - No Parent
**Priority**: P1 - High  
**Description**: Verify root unit handling  
**Business Rule**: Regions typically have no parent  
**Preconditions**: None

**Test Steps**:
1. Create Region with ParentId = null
2. Verify created successfully
3. Query root units
4. Verify Region in list

**Expected Result**: Root units supported  
**Business Impact**: Top-level hierarchy

---

### TC-OHM-BL-P1-005: Delete Organization Unit - With Children
**Priority**: P1 - High  
**Description**: Verify deletion with child units  
**Business Rule**: Cannot delete unit with children  
**Preconditions**: Hub with OrgUnits

**Test Steps**:
1. Attempt to delete Hub with children
2. Verify rejected with error
3. Delete children first
4. Delete Hub - should succeed

**Expected Result**: Cascade protection  
**Business Impact**: Data integrity

---

### TC-OHM-BL-P1-006: List All - Flat vs Hierarchical
**Priority**: P1 - High  
**Description**: Verify list formats  
**Business Rule**: Can get flat list or hierarchical tree  
**Preconditions**: Full hierarchy exists

**Test Steps**:
1. Get flat list
2. Verify all units returned without nesting
3. Get hierarchical tree
4. Verify proper nesting structure

**Expected Result**: Both formats available  
**Business Impact**: UI flexibility

---

### TC-OHM-BL-P1-007: User Permission Filtering
**Priority**: P1 - High  
**Description**: Verify user sees their org units  
**Business Rule**: Users filtered by their org unit access  
**Preconditions**: User assigned to specific OrgUnit

**Test Steps**:
1. Query as user with OrgUnit A access
2. Verify sees OrgUnit A and ancestors
3. Verify doesn't see unrelated OrgUnits

**Expected Result**: Permission-based filtering  
**Business Impact**: Multi-tenant security

---

### TC-OHM-BL-P1-008: Audit Fields - Creation
**Priority**: P1 - High  
**Description**: Verify audit trail on creation  
**Business Rule**: CreatedBy/Date tracked  
**Preconditions**: User authenticated

**Test Steps**:
1. Create organization unit
2. Verify CreatedBy = current user
3. Verify CreatedDate set

**Expected Result**: Audit trail created  
**Business Impact**: Accountability

---

## P2 - Medium Priority Business Logic Tests

### TC-OHM-BL-P2-001: Update Name and Code
**Priority**: P2 - Medium  
**Description**: Verify name/code update  
**Test Steps**:
1. Update unit name
2. Verify change saved
3. Update code (if allowed)
4. Verify uniqueness check

**Expected Result**: Updates work correctly

---

### TC-OHM-BL-P2-002: Description Field
**Priority**: P2 - Medium  
**Description**: Verify description handling  
**Test Steps**:
1. Create with description
2. Verify stored
3. Update description
4. Verify change saved

**Expected Result**: Description managed correctly

---

### TC-OHM-BL-P2-003: Sort Order
**Priority**: P2 - Medium  
**Description**: Verify ordering of sibling units  
**Test Steps**:
1. Create units with different names
2. Query siblings
3. Verify consistent ordering

**Expected Result**: Predictable ordering

---

### TC-OHM-BL-P2-004: Deep Hierarchy Performance
**Priority**: P2 - Medium  
**Description**: Verify performance with deep nesting  
**Test Steps**:
1. Create 10-level hierarchy
2. Query descendants from root
3. Verify completes in reasonable time

**Expected Result**: Performance acceptable

---

## P3 - Low Priority Edge Cases

### TC-OHM-BL-P3-001: Self-Reference Prevention
**Priority**: P3 - Low  
**Description**: Verify cannot set self as parent  
**Test Steps**:
1. Attempt to set unit's ParentId to itself
2. Verify rejected

**Expected Result**: Self-reference prevented

---

### TC-OHM-BL-P3-002: Circular Reference Prevention
**Priority**: P3 - Low  
**Description**: Verify circular references blocked  
**Test Steps**:
1. Create A → B → C hierarchy
2. Attempt to set A's parent to C
3. Verify circular reference detected and rejected

**Expected Result**: No circular references allowed

---

### TC-OHM-BL-P3-003: Empty Hierarchy Query
**Priority**: P3 - Low  
**Description**: Verify empty database handling  
**Test Steps**:
1. Query with no units in database
2. Verify empty result, no error

**Expected Result**: Empty handled gracefully

---

### TC-OHM-BL-P3-004: Very Long Name
**Priority**: P3 - Low  
**Description**: Verify max length handling  
**Test Steps**:
1. Create with 200-character name
2. Verify accepted or truncated

**Expected Result**: Long names handled

---

## Integration with Unit Tests

Implement in: `tests/UNOPS.PAO.Business.Tests/Managers/OrganizationHierarchyManagerBusinessLogicTests.cs`

---

## Related Documentation

- [OrganizationHierarchy Entity](../../UNOPS.PAO.Domain/Entities/OrganizationHierarchy.cs)
- [OrganizationUnitType Enum](../../UNOPS.PAO.Domain/Enums/OrganizationUnitType.cs)
- [Existing Test Cases](../Business/OrganizationHierarchyManager/OrganizationHierarchyManager_TestCases.md)

