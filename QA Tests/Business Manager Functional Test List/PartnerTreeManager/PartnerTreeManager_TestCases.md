# PartnerTreeManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `PartnerTreeManager`  
**Location**: `UNOPS.PAO.Business/Managers/PartnerTreeManager.cs`  
**Purpose**: Manages partner tree hierarchy with categories, groups, and parent-child relationships.

---

## Functional Test Cases (20+ Cases)

### TC-PTM-F001-020: Core CRUD Operations
- F001: Create partner tree - valid model
- F002: Create partner tree - maps entity correctly
- F003: Get partner trees - returns hierarchical structure
- F004: Get partner trees - sorted by name ascending
- F005: Get partner trees - sorted by name descending
- F006: Get partner trees - builds hierarchy from parent codes
- F007: Get partner trees - handles null/empty parent as root
- F008: Get partner trees - prevents circular reference
- F009: Get partner tree by ID - existing tree
- F010: Get partner tree by ID - non-existent returns null
- F011: Update partner tree - valid update
- F012: Update partner tree - non-existent returns null
- F013: Delete partner tree - existing tree
- F014: Delete partner tree - non-existent (no error)
- F015: Get posted partner trees - external format
- F016: Get posted partner tree by ID - includes eligible entities
- F017: Get category and group structure - returns formatted structure
- F018: Get category and group structure - recursive group collection
- F019: Map entity to model - correct wrapper structure
- F020: Hierarchy building - visited codes tracking prevents duplicates

---

## Performance Test Cases (10 Cases)

### TC-PTM-P001: Create Partner Tree - Response Time
**Performance Criteria**: < 200ms per creation

### TC-PTM-P002: Get Partner Trees - Response Time
**Performance Criteria**: < 500ms for full hierarchy

### TC-PTM-P003: Get Partner Trees - Large Dataset
**Performance Criteria**: < 1000ms for 500 trees

### TC-PTM-P004: Get Partner Tree by ID - Response Time
**Performance Criteria**: < 100ms per lookup

### TC-PTM-P005: Update Partner Tree - Response Time
**Performance Criteria**: < 200ms per update

### TC-PTM-P006: Delete Partner Tree - Response Time
**Performance Criteria**: < 100ms per delete

### TC-PTM-P007: Hierarchy Building Performance
**Performance Criteria**: < 300ms for tree construction

### TC-PTM-P008: Get Category/Group Structure - Response Time
**Performance Criteria**: < 400ms for formatted structure

### TC-PTM-P009: Concurrent Tree Access
**Performance Criteria**: 20 concurrent reads < 600ms each

### TC-PTM-P010: AutoMapper Performance
**Performance Criteria**: < 30ms for entity mapping

---

## Concurrency Test Cases (10 Cases)

### TC-PTM-C001: Concurrent Partner Tree Creation
**Scenario**: 10 threads creating trees simultaneously

### TC-PTM-C002: Concurrent Get Partner Trees
**Scenario**: 20 threads requesting full hierarchy

### TC-PTM-C003: Concurrent Updates - Same Tree
**Scenario**: 5 threads updating same partner tree

### TC-PTM-C004: Concurrent Updates - Different Trees
**Scenario**: 15 threads updating different trees

### TC-PTM-C005: Concurrent Deletion
**Scenario**: Multiple threads deleting trees

### TC-PTM-C006: Create During Hierarchy Query
**Scenario**: Creating tree while hierarchy being built

### TC-PTM-C007: Update Parent Code During Query
**Scenario**: Changing parent while hierarchy queried

### TC-PTM-C008: Concurrent Category/Group Structure Access
**Scenario**: 10 threads requesting formatted structure

### TC-PTM-C009: Repository Thread Safety
**Scenario**: Verify DataRepository concurrent access

### TC-PTM-C010: High Load CRUD Operations
**Scenario**: 50 concurrent CRUD operations

---

## Edge Cases (5 Cases)

### TC-PTM-E001: Partner Tree Code with Special Characters
### TC-PTM-E002: Very Deep Hierarchy (10+ levels)
### TC-PTM-E003: Parent Code References Non-Existent Tree
### TC-PTM-E004: Circular Parent-Child Reference
### TC-PTM-E005: Partner Tree Name Exceeds Maximum Length

