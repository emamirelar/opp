# LinkManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `LinkManager`  
**Location**: `UNOPS.PAO.Business/Managers/LinkManager.cs`  
**Purpose**: Manages external links associated with entities (Partners, Contacts, PartnerTrees).

---

## Functional Test Cases (20+ Cases)

### TC-LM-F001-020: Core CRUD Operations
- F001: Create link - valid partner entity
- F002: Create link - valid contact entity
- F003: Create link - valid partner tree entity
- F004: Create link - name defaults to URL when null
- F005: Create link - with explicit name
- F006: Create link - entity does not exist throws exception
- F007: Get link by ID - existing link
- F008: Get link by ID - non-existent link returns null
- F009: Get link by ID - deleted link returns null
- F010: Get link by ID - orphaned link (entity deleted) returns null and deletes link
- F011: Update link - valid update
- F012: Update link - non-existent link returns null
- F013: Update link - name defaults to URL when null
- F014: Update link - entity does not exist deletes link and throws
- F015: Delete link - existing link
- F016: Delete link - non-existent link (no error)
- F017: Get all links - returns all non-deleted
- F018: Get entity links - filter by Partner type
- F019: Get entity links - filter by Contact type
- F020: Get entity links - filter by PartnerTree type

---

## Performance Test Cases (10 Cases)

### TC-LM-P001: Create Link - Response Time
**Performance Criteria**: < 200ms including validation

### TC-LM-P002: Get Link by ID - Response Time
**Performance Criteria**: < 100ms per lookup

### TC-LM-P003: Get Entity Links - Pagination Performance
**Performance Criteria**: < 300ms for 100 links

### TC-LM-P004: Update Link - Response Time
**Performance Criteria**: < 200ms including validation

### TC-LM-P005: Delete Link - Response Time
**Performance Criteria**: < 100ms per delete

### TC-LM-P006: Entity Validation - Response Time
**Performance Criteria**: < 50ms per entity check

### TC-LM-P007: Get All Links - Large Dataset
**Performance Criteria**: < 1000ms for 1000 links

### TC-LM-P008: Concurrent Link Operations
**Performance Criteria**: 20 concurrent creates < 300ms each

### TC-LM-P009: AutoMapper Performance
**Performance Criteria**: < 10ms per mapping

### TC-LM-P010: Pagination Query Performance
**Performance Criteria**: < 200ms with filtering

---

## Concurrency Test Cases (10 Cases)

### TC-LM-C001: Concurrent Link Creation - Same Entity
**Scenario**: 10 threads creating links for same partner

### TC-LM-C002: Concurrent Link Creation - Different Entities
**Scenario**: 20 threads creating links for different entities

### TC-LM-C003: Concurrent Link Updates - Same Link
**Scenario**: 5 threads updating same link

### TC-LM-C004: Concurrent Link Deletion
**Scenario**: Multiple threads deleting links simultaneously

### TC-LM-C005: Concurrent Get Entity Links
**Scenario**: 15 threads querying same entity links

### TC-LM-C006: Create During Entity Deletion
**Scenario**: Creating link while parent entity being deleted

### TC-LM-C007: Update During Read
**Scenario**: Updating link while being read

### TC-LM-C008: Concurrent Entity Validation
**Scenario**: Multiple threads validating different entities

### TC-LM-C009: High Load Link Operations
**Scenario**: 50 concurrent CRUD operations

### TC-LM-C010: Repository Concurrent Access
**Scenario**: Verify DataRepository thread safety

---

## Edge Cases (5 Cases)

### TC-LM-E001: URL with Maximum Length
### TC-LM-E002: URL with Special Characters and Encoding
### TC-LM-E003: Unsupported Entity Type Validation
### TC-LM-E004: Link Name with Unicode Characters
### TC-LM-E005: Empty Entity Links Query Returns Empty Response

