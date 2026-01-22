# DocumentTypeManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `DocumentTypeManager`  
**Location**: `UNOPS.PAO.Business/Managers/DocumentTypeManager.cs`  
**Purpose**: Manages document type definitions and filtering by entity type.

---

## Functional Test Cases (20+ Cases)

### TC-DTM-F001-020: Core Operations
- F001: Get document types - returns all non-deleted types
- F002: Get document types - excludes deleted types
- F003: Get document types - filter by entity type "Partner"
- F004: Get document types - filter by entity type "Contact"
- F005: Get document types - filter by entity type "Interaction"
- F006: Get document types - empty entity type returns all
- F007: Get document types - null entity type returns all
- F008: Get document types - invalid entity type returns empty
- F009: Get document types - pagination first page
- F010: Get document types - pagination second page
- F011: Get document types - pagination with page size
- F012: Get document types - empty database returns empty list
- F013: Get document types - single result
- F014: Get document types - large result set (100+ types)
- F015: Get document types - verify mapping to DocumentTypeModel
- F016: Get document types - verify Name property mapped
- F017: Get document types - verify EntityType property mapped
- F018: Get document types - verify Id property mapped
- F019: Get document types - case sensitivity of entity type filter
- F020: Get document types - whitespace in entity type filter

---

## Performance Test Cases (10 Cases)

### TC-DTM-P001: Get Document Types - Response Time
**Performance Criteria**: < 200ms for 100 document types

### TC-DTM-P002: Get Document Types - Large Dataset
**Performance Criteria**: < 500ms for 1000 document types

### TC-DTM-P003: Get Document Types - With Filter
**Performance Criteria**: < 150ms with entity type filter

### TC-DTM-P004: Get Document Types - Pagination Performance
**Performance Criteria**: < 100ms per page

### TC-DTM-P005: Concurrent Document Type Queries
**Performance Criteria**: 20 concurrent requests < 300ms each

### TC-DTM-P006: Get Document Types - Cold Start
**Performance Criteria**: < 500ms first query

### TC-DTM-P007: Get Document Types - Cached Query
**Performance Criteria**: < 50ms subsequent queries

### TC-DTM-P008: Entity Type Filter Index Usage
**Performance Criteria**: < 100ms with indexed filter

### TC-DTM-P009: Mapping Performance
**Performance Criteria**: < 10ms for 100 entity mappings

### TC-DTM-P010: Memory Usage - Large Result Set
**Performance Criteria**: < 50MB for 1000 types

---

## Concurrency Test Cases (10 Cases)

### TC-DTM-C001: Concurrent Get Document Types
**Scenario**: 20 threads requesting all document types simultaneously

### TC-DTM-C002: Concurrent Filtered Queries - Same Filter
**Scenario**: 10 threads filtering by same entity type

### TC-DTM-C003: Concurrent Filtered Queries - Different Filters
**Scenario**: 10 threads filtering by different entity types

### TC-DTM-C004: Concurrent Pagination Requests
**Scenario**: 15 threads requesting different pages

### TC-DTM-C005: Get Document Types During Data Update
**Scenario**: Reading while admin updates document types

### TC-DTM-C006: Concurrent Repository Access
**Scenario**: Multiple threads accessing DataRepository

### TC-DTM-C007: Mapper Thread Safety
**Scenario**: Concurrent AutoMapper operations

### TC-DTM-C008: Database Connection Pool
**Scenario**: 50 concurrent requests testing connection pool

### TC-DTM-C009: Concurrent Delete Flag Check
**Scenario**: Querying while documents being soft-deleted

### TC-DTM-C010: High Load Scenario
**Scenario**: 100 concurrent requests under load

---

## Edge Cases (5 Cases)

### TC-DTM-E001: Entity Type with Special Characters
### TC-DTM-E002: Very Long Entity Type Name
### TC-DTM-E003: All Document Types Deleted
### TC-DTM-E004: Pagination Beyond Available Data
### TC-DTM-E005: Null Request Parameters Object

