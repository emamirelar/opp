# PartnerManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `PartnerManager`  
**Location**: `UNOPS.PAO.Business/Managers/PartnerManager.cs`  
**Purpose**: Manages partner (organization) operations including CRUD, organization hierarchy relationships, permissions, and partner tree management.

**Key Responsibilities**:
- Partner lifecycle management (create, read, update, delete)
- Organization unit relationship management
- Partner group and category filtering
- Logo upload and management
- Permission-based access control
- Partner tree hierarchy navigation

---

## Functional Test Cases (20+ Cases)

### TC-PM-F001: Create Partner - Valid Data
**Description**: Create a new partner with all required and optional fields  
**Preconditions**: User has partner creation permissions  
**Test Steps**:
1. Prepare PartnerRequest with valid data (name, type, status, etc.)
2. Call `CreatePartnerAsync(model)`
3. Verify partner is created with generated ID
4. Verify all fields are correctly saved

**Expected Result**: Partner created successfully, returns PartnerModel with ID  
**Test Data**: Valid partner with name "Test Organization", status "Active"

---

### TC-PM-F002: Create Partner - With Organization Unit Relationships
**Description**: Create partner and associate with multiple organization units  
**Preconditions**: Organization units exist in database  
**Test Steps**:
1. Create PartnerRequest with OrganizationHierarchyIds containing 3 valid org unit IDs
2. Call `CreatePartnerAsync(model)`
3. Verify partner created
4. Query OrganizationUnitRelationships table
5. Verify 3 relationships created with correct EntityType and EntityId

**Expected Result**: Partner created with 3 organization unit relationships  
**Test Data**: Org unit IDs [101, 102, 103]

---

### TC-PM-F003: Create Partner - Invalid Organization Unit ID
**Description**: Attempt to create partner with non-existent org unit ID  
**Preconditions**: Org unit ID 99999 does not exist  
**Test Steps**:
1. Create PartnerRequest with OrganizationHierarchyIds containing [99999]
2. Call `CreatePartnerAsync(model)`
3. Verify BusinessException is thrown

**Expected Result**: BusinessException with message about invalid org unit ID  
**Test Data**: Invalid org unit ID 99999

---

### TC-PM-F004: Create Partner - Org Unit Wrong Type
**Description**: Attempt to create partner with org unit that's not of type OrgUnit  
**Preconditions**: Org hierarchy ID 500 exists but has Type = Country  
**Test Steps**:
1. Create PartnerRequest with OrganizationHierarchyIds containing [500]
2. Call `CreatePartnerAsync(model)`
3. Verify BusinessException is thrown with type validation message

**Expected Result**: BusinessException about org unit type requirement  
**Test Data**: Org hierarchy ID with Type != OrgUnit

---

### TC-PM-F005: Get Partners - Paginated List
**Description**: Retrieve partners with pagination  
**Preconditions**: Database contains 50 partners  
**Test Steps**:
1. Create PaginationRequest with PageIndex=1, PageSize=10
2. Call `GetPartners(userId, request)`
3. Verify response contains 10 partners
4. Verify TotalCount = 50
5. Verify partners are not deleted (IsDeleted = false)

**Expected Result**: Returns 10 partners, TotalCount = 50  
**Test Data**: 50 partners in database

---

### TC-PM-F006: Get Partners - Filtered By OrgUnit Type
**Description**: Verify partners filtered to only those with OrgUnit type relationships  
**Preconditions**: 
- 30 partners with OrgUnit relationships
- 20 partners with Country relationships
**Test Steps**:
1. Call `GetPartners(userId, paginationRequest)`
2. Verify only partners with OrgUnit type relationships are returned
3. Verify no partners with only Country relationships are in results

**Expected Result**: Returns only 30 partners with valid OrgUnit relationships  
**Test Data**: Mixed organization hierarchy types

---

### TC-PM-F007: Get Partners - With Sorting
**Description**: Retrieve partners sorted by name ascending  
**Preconditions**: Database has partners with names A-Z  
**Test Steps**:
1. Create PaginationRequest with OrderBy="Name", Ascending=true
2. Call `GetPartners(userId, request)`
3. Verify partners returned in alphabetical order

**Expected Result**: Partners sorted alphabetically by name  
**Test Data**: Partners with various names

---

### TC-PM-F008: Get Partner By ID - Exists
**Description**: Retrieve single partner by valid ID  
**Preconditions**: Partner with ID 123 exists  
**Test Steps**:
1. Call `GetPartner(userId, 123)`
2. Verify partner details returned
3. Verify organization unit relationships loaded

**Expected Result**: Returns PartnerModel with complete data  
**Test Data**: Partner ID 123

---

### TC-PM-F009: Get Partner By ID - Not Found
**Description**: Attempt to retrieve non-existent partner  
**Preconditions**: Partner ID 99999 does not exist  
**Test Steps**:
1. Call `GetPartner(userId, 99999)`
2. Verify returns null/default value

**Expected Result**: Returns null  
**Test Data**: Invalid partner ID 99999

---

### TC-PM-F010: Get Partner By ID - Deleted Partner
**Description**: Attempt to retrieve soft-deleted partner  
**Preconditions**: Partner ID 456 exists with IsDeleted=true  
**Test Steps**:
1. Call `GetPartner(userId, 456)`
2. Verify either returns null or includes deletion information

**Expected Result**: Handles deleted partner appropriately  
**Test Data**: Soft-deleted partner

---

### TC-PM-F011: Update Partner - Valid Changes
**Description**: Update partner with valid data  
**Preconditions**: Partner ID 123 exists  
**Test Steps**:
1. Prepare UpdatePartnerRequest with modified name, description
2. Call `UpdatePartnerAsync(userId, model)`
3. Verify partner updated successfully
4. Retrieve partner and verify changes persisted

**Expected Result**: Partner updated, changes persisted  
**Test Data**: Partner 123 with new name "Updated Organization"

---

### TC-PM-F012: Update Partner - Organization Unit Relationships (Add New)
**Description**: Add new organization unit relationships to existing partner  
**Preconditions**: 
- Partner 123 has org units [101, 102]
- Org units 103, 104 exist
**Test Steps**:
1. Prepare UpdatePartnerRequest with OrganizationHierarchyIds=[101, 102, 103, 104]
2. Call `UpdatePartnerAsync(userId, model)`
3. Verify 2 new relationships added
4. Verify existing relationships preserved
5. Verify differential update used (no delete/recreate of existing)

**Expected Result**: 4 total relationships, only 2 new ones added  
**Test Data**: Existing [101,102], adding [103,104]

---

### TC-PM-F013: Update Partner - Organization Unit Relationships (Remove Existing)
**Description**: Remove organization unit relationships from partner  
**Preconditions**: Partner 123 has org units [101, 102, 103]  
**Test Steps**:
1. Prepare UpdatePartnerRequest with OrganizationHierarchyIds=[101, 103]
2. Call `UpdatePartnerAsync(userId, model)`
3. Verify relationship to org unit 102 deleted
4. Verify relationships to 101 and 103 preserved

**Expected Result**: Relationship 102 removed, others remain  
**Test Data**: Remove org unit 102

---

### TC-PM-F014: Update Partner - Organization Unit Relationships (Replace All)
**Description**: Completely replace organization unit relationships  
**Preconditions**: Partner 123 has org units [101, 102]  
**Test Steps**:
1. Prepare UpdatePartnerRequest with OrganizationHierarchyIds=[105, 106, 107]
2. Call `UpdatePartnerAsync(userId, model)`
3. Verify relationships to 101, 102 deleted
4. Verify new relationships to 105, 106, 107 created

**Expected Result**: Old relationships deleted, new ones created  
**Test Data**: Replace [101,102] with [105,106,107]

---

### TC-PM-F015: Delete Partner - Soft Delete
**Description**: Soft delete an existing partner  
**Preconditions**: Partner ID 123 exists with IsDeleted=false  
**Test Steps**:
1. Call `DeletePartnerAsync(userId, 123)`
2. Query database for partner 123
3. Verify IsDeleted=true
4. Verify partner not returned in GetPartners() call

**Expected Result**: Partner soft-deleted, not in active lists  
**Test Data**: Partner 123

---

### TC-PM-F016: Delete Partner - Non-Existent
**Description**: Attempt to delete non-existent partner  
**Preconditions**: Partner ID 99999 does not exist  
**Test Steps**:
1. Call `DeletePartnerAsync(userId, 99999)`
2. Verify no exception thrown (graceful handling)

**Expected Result**: Operation completes without error  
**Test Data**: Invalid partner ID 99999

---

### TC-PM-F017: Get Partners By Partner Group
**Description**: Retrieve all partners belonging to a specific partner group  
**Preconditions**: 
- Partner group 5 exists
- 15 partners belong to group 5
**Test Steps**:
1. Create PaginationRequest
2. Call `GetPartnersByPartnerGroup(userId, 5, request)`
3. Verify only partners with PartnerGroupId=5 returned
4. Verify TotalCount = 15

**Expected Result**: Returns 15 partners from group 5  
**Test Data**: Partner group ID 5

---

### TC-PM-F018: Get Partners By Category Code
**Description**: Retrieve partners by partner category  
**Preconditions**: 
- Category "NGO" exists
- 25 partners have category "NGO"
**Test Steps**:
1. Call `GetPartnersByPartnerCategory(userId, "NGO", request)`
2. Verify all returned partners have category "NGO"
3. Verify TotalCount matches expected

**Expected Result**: Returns partners with NGO category  
**Test Data**: Category code "NGO"

---

### TC-PM-F019: Update Partner Logo - Valid File
**Description**: Upload partner logo image  
**Preconditions**: Partner 123 exists, valid image file prepared  
**Test Steps**:
1. Prepare IFormFile with valid JPG image
2. Call `UpdatePartnerLogoAsync(123, file)`
3. Verify file saved to correct location
4. Verify partner.LogoUrl updated with correct path
5. Verify returned URL is valid

**Expected Result**: Logo uploaded, URL returned  
**Test Data**: Valid JPG file, partner 123

---

### TC-PM-F020: Update Partner Logo - Create Directory
**Description**: Upload logo when directory doesn't exist  
**Preconditions**: 
- Partner 123 exists
- Upload directory doesn't exist
**Test Steps**:
1. Ensure directory "wwwroot/uploads/partners" doesn't exist
2. Upload partner logo
3. Verify directory automatically created
4. Verify file saved successfully

**Expected Result**: Directory created, file uploaded  
**Test Data**: Partner 123, valid image

---

### TC-PM-F021: Get Partners With Specification - Simple Filter
**Description**: Use specification pattern to filter partners  
**Preconditions**: Multiple partners with different attributes  
**Test Steps**:
1. Create specification for partners with Status=Active
2. Call `GetPartnersWithSpecification(userId, specification, pagination)`
3. Verify only active partners returned
4. Verify specification criteria applied correctly

**Expected Result**: Returns only active partners  
**Test Data**: Specification filtering by Status

---

### TC-PM-F022: Get Partners With Specification - OrgUnit Filter
**Description**: Use specification with org unit filtering  
**Preconditions**: Partners associated with different org units  
**Test Steps**:
1. Create specification that supports org unit filtering
2. Call `GetPartnersWithSpecification(userId, specification, pagination)`
3. Verify ApplyOrgUnitFilter method invoked
4. Verify filtered results match org unit criteria

**Expected Result**: Returns partners filtered by org unit  
**Test Data**: Specification with org unit filter

---

### TC-PM-F023: Get Partner With Contacts And Interactions
**Description**: Retrieve partner with all related contacts and their interactions  
**Preconditions**: 
- Partner 123 exists with 5 contacts
- Each contact has 3 interactions
**Test Steps**:
1. Call `GetPartnerWithContactsAndInteractionsAsync(123)`
2. Verify partner returned with Contacts collection
3. Verify each contact has Interactions collection loaded
4. Verify interaction count matches expected

**Expected Result**: Partner with full contact and interaction data  
**Test Data**: Partner 123 with nested relationships

---

### TC-PM-F024: Get Child Partner Trees Recursively - Single Level
**Description**: Get immediate child partner trees  
**Preconditions**: Parent codes ["NGO", "INGO"] have direct children  
**Test Steps**:
1. Call `GetChildPartnerTreesRecursively(["NGO", "INGO"])`
2. Verify immediate children returned
3. Verify correct parent-child relationships

**Expected Result**: Returns direct children of NGO and INGO  
**Test Data**: Parent codes with children

---

### TC-PM-F025: Get Child Partner Trees Recursively - Multiple Levels
**Description**: Get all descendant partner trees recursively  
**Preconditions**: Partner tree has 4 levels of hierarchy  
**Test Steps**:
1. Call `GetChildPartnerTreesRecursively(["ROOT"])`
2. Verify all descendants at all levels returned
3. Verify recursive traversal complete

**Expected Result**: Returns all descendants across 4 levels  
**Test Data**: Multi-level partner tree

---

### TC-PM-F026: Get All Descendant Partner Trees
**Description**: Get original trees plus all descendants  
**Preconditions**: Partner categories have hierarchical structure  
**Test Steps**:
1. Call `GetAllDescendantPartnerTrees(["NGO", "INGO"])`
2. Verify original trees (NGO, INGO) included in result
3. Verify all descendants included
4. Verify no duplicates

**Expected Result**: Original + descendants returned  
**Test Data**: Category codes with children

---

### TC-PM-F027: Has Permission - Creator Access
**Description**: Verify creator has full access to their partner  
**Preconditions**: User 100 created partner 123  
**Test Steps**:
1. Call `HasPermissionAsync(100, 123, "Update")`
2. Verify returns true
3. Call `HasPermissionAsync(100, 123, "Delete")`
4. Verify returns true

**Expected Result**: Creator has full permissions  
**Test Data**: Creator user accessing own partner

---

### TC-PM-F028: Has Permission - Read Access for Non-Creator
**Description**: Verify non-creator can read partner  
**Preconditions**: User 200 did not create partner 123  
**Test Steps**:
1. Call `HasPermissionAsync(200, 123, "Read")`
2. Verify returns true (read allowed for all)

**Expected Result**: Read permission granted to all users  
**Test Data**: Different user reading partner

---

### TC-PM-F029: Has Permission - Write Denied for Non-Creator
**Description**: Verify non-creator cannot update partner  
**Preconditions**: User 200 did not create partner 123  
**Test Steps**:
1. Call `HasPermissionAsync(200, 123, "Update")`
2. Verify returns false

**Expected Result**: Update permission denied  
**Test Data**: Non-creator attempting update

---

### TC-PM-F030: Empty Collection Handling
**Description**: Test behavior with empty organization hierarchy IDs  
**Preconditions**: None  
**Test Steps**:
1. Create PartnerRequest with OrganizationHierarchyIds = []
2. Call `CreatePartnerAsync(model)`
3. Verify partner created without relationships
4. Verify no errors thrown

**Expected Result**: Partner created, no relationships added  
**Test Data**: Empty array for org hierarchy IDs

---

## Performance Test Cases

### TC-PM-P001: Create Partner - Response Time
**Description**: Measure partner creation performance  
**Performance Criteria**: < 500ms for single partner creation  
**Test Steps**:
1. Create 100 partners sequentially
2. Measure time for each creation
3. Calculate average, min, max response times

**Expected Result**: Average < 500ms, Max < 1000ms  
**Load**: 100 sequential operations

---

### TC-PM-P002: Get Partners - Large Dataset Performance
**Description**: Pagination performance with large dataset  
**Performance Criteria**: < 1000ms for paginated query  
**Preconditions**: Database contains 10,000 partners  
**Test Steps**:
1. Query page 1 (records 1-50)
2. Query page 100 (records 5000-5050)
3. Query last page
4. Measure response time for each query

**Expected Result**: All queries < 1000ms  
**Load**: 10,000 partners in database

---

### TC-PM-P003: Update Partner - Organization Unit Differential Update
**Description**: Test efficiency of differential org unit update  
**Performance Criteria**: Differential update faster than delete/recreate  
**Preconditions**: Partner has 20 org unit relationships  
**Test Steps**:
1. Measure time to update partner adding 5 new org units (differential)
2. Measure memory usage
3. Verify only 5 INSERT operations executed (not 25 DELETE + 25 INSERT)

**Expected Result**: Differential update more efficient  
**Load**: 20 existing + 5 new org units

---

### TC-PM-P004: Get Partners With Specification - Complex Query
**Description**: Performance of specification pattern with complex criteria  
**Performance Criteria**: < 2000ms for complex filtered query  
**Preconditions**: 10,000 partners with varied attributes  
**Test Steps**:
1. Create specification with multiple filters (status, category, name search, org unit)
2. Execute query with specification
3. Measure execution time

**Expected Result**: Complex query < 2000ms  
**Load**: 10,000 partners with multi-criteria filter

---

### TC-PM-P005: Bulk Partner Creation
**Description**: Create large batch of partners  
**Performance Criteria**: Throughput > 50 partners/second  
**Test Steps**:
1. Prepare 1000 partner creation requests
2. Execute creations as fast as possible
3. Measure total time and calculate throughput

**Expected Result**: Throughput > 50 partners/second  
**Load**: 1000 partners

---

### TC-PM-P006: Get Partner With Relationships - Load Time
**Description**: Loading partner with many related entities  
**Performance Criteria**: < 1500ms for partner with many relationships  
**Preconditions**: 
- Partner has 50 contacts
- Each contact has 20 interactions
**Test Steps**:
1. Call `GetPartnerWithContactsAndInteractionsAsync`
2. Measure load time
3. Verify eager loading efficient

**Expected Result**: Load complete < 1500ms  
**Load**: 1 partner, 50 contacts, 1000 interactions

---

### TC-PM-P007: Recursive Partner Tree Traversal - Deep Hierarchy
**Description**: Performance of recursive tree traversal  
**Performance Criteria**: < 3000ms for 10-level deep tree  
**Preconditions**: Partner tree with 10 levels, 500 total nodes  
**Test Steps**:
1. Call `GetChildPartnerTreesRecursively` from root
2. Measure traversal time
3. Verify all nodes retrieved

**Expected Result**: Complete traversal < 3000ms  
**Load**: 10 levels, 500 nodes

---

### TC-PM-P008: Logo Upload - Large File
**Description**: Upload performance for large image file  
**Performance Criteria**: < 3000ms for 5MB image  
**Test Steps**:
1. Prepare 5MB JPG image
2. Call `UpdatePartnerLogoAsync`
3. Measure upload and save time

**Expected Result**: Upload complete < 3000ms  
**Load**: 5MB image file

---

### TC-PM-P009: Pagination - Memory Efficiency
**Description**: Verify pagination doesn't load entire dataset  
**Performance Criteria**: Memory usage < 100MB for 10K partner query  
**Preconditions**: 10,000 partners in database  
**Test Steps**:
1. Monitor memory before query
2. Query page 1 (50 records)
3. Monitor memory after query
4. Verify only 50 partners loaded into memory, not all 10,000

**Expected Result**: Memory increase < 10MB  
**Load**: 10,000 partners, query 50

---

### TC-PM-P010: Delete Partner - Cascade Performance
**Description**: Soft delete performance with many relationships  
**Performance Criteria**: < 2000ms for partner with many relationships  
**Preconditions**: Partner has 100 org unit relationships  
**Test Steps**:
1. Call `DeletePartnerAsync` for partner with many relationships
2. Measure deletion time
3. Verify relationships handled efficiently

**Expected Result**: Delete operation < 2000ms  
**Load**: 1 partner, 100 relationships

---

## Concurrency Test Cases

### TC-PM-C001: Concurrent Partner Creation - Same Name
**Description**: Test race condition when creating partners with same name simultaneously  
**Concurrency Scenario**: 10 threads create partner with name "Test Org" simultaneously  
**Test Steps**:
1. Spawn 10 threads
2. Each thread calls `CreatePartnerAsync` with same partner name
3. Verify all 10 partners created successfully
4. Verify each has unique ID
5. Check for any constraint violations or deadlocks

**Expected Result**: All 10 partners created with unique IDs  
**Load**: 10 concurrent creation requests

---

### TC-PM-C002: Concurrent Updates - Same Partner
**Description**: Multiple users updating same partner simultaneously  
**Concurrency Scenario**: 5 users update partner 123 at same time  
**Test Steps**:
1. Spawn 5 threads with different update data
2. Each updates different fields of partner 123
3. Verify no data loss
4. Verify last update wins or optimistic concurrency control applied

**Expected Result**: All updates processed, no data corruption  
**Load**: 5 concurrent updates to same entity

---

### TC-PM-C003: Concurrent Organization Unit Relationship Updates
**Description**: Race condition when updating org unit relationships  
**Concurrency Scenario**: 3 users modify org units for partner 123 simultaneously  
**Test Steps**:
1. Initial state: Partner has org units [101, 102]
2. Thread 1: Update to [101, 102, 103]
3. Thread 2: Update to [101, 104]
4. Thread 3: Update to [105, 106]
5. Execute simultaneously
6. Verify final state is consistent (one update succeeds completely)
7. Check for orphaned relationships

**Expected Result**: One update succeeds, data consistent  
**Load**: 3 concurrent relationship updates

---

### TC-PM-C004: Concurrent Read During Update
**Description**: Reading partner while it's being updated  
**Concurrency Scenario**: 
- Thread 1: Updating partner
- Thread 2: Reading same partner
**Test Steps**:
1. Thread 1 starts update (long-running operation)
2. Thread 2 reads partner during update
3. Verify Thread 2 gets consistent data (either old or new, not partial)
4. Verify no read locks blocking update

**Expected Result**: Consistent read, no deadlocks  
**Load**: 1 update + 1 concurrent read

---

### TC-PM-C005: Concurrent Delete and Read
**Description**: Deleting partner while another thread reads it  
**Concurrency Scenario**: 
- Thread 1: Deletes partner 123
- Thread 2: Reads partner 123
**Test Steps**:
1. Thread 1 calls `DeletePartnerAsync(123)`
2. Thread 2 calls `GetPartner(123)` during delete
3. Verify Thread 2 either gets partner (before delete) or null (after delete)
4. Verify no exception thrown

**Expected Result**: Consistent state, no exceptions  
**Load**: 1 delete + 1 concurrent read

---

### TC-PM-C006: Concurrent Specification Queries
**Description**: Multiple users running complex specification queries simultaneously  
**Concurrency Scenario**: 20 users running different filtered queries  
**Test Steps**:
1. Spawn 20 threads with different specifications
2. Execute all queries simultaneously
3. Verify all queries return correct results
4. Monitor database connection pool
5. Check for connection exhaustion

**Expected Result**: All queries succeed, no connection issues  
**Load**: 20 concurrent specification queries

---

### TC-PM-C007: Concurrent Logo Upload - Same Partner
**Description**: Race condition when multiple users upload logo for same partner  
**Concurrency Scenario**: 3 users upload different logos for partner 123  
**Test Steps**:
1. Spawn 3 threads with different image files
2. Each calls `UpdatePartnerLogoAsync(123, file)`
3. Verify only one logo URL stored (last write wins)
4. Verify no file corruption
5. Check filesystem for orphaned files

**Expected Result**: One logo URL saved, files managed correctly  
**Load**: 3 concurrent logo uploads

---

### TC-PM-C008: Concurrent Create and Query
**Description**: Creating partners while querying list  
**Concurrency Scenario**: 
- Thread 1: Creating 100 partners
- Thread 2: Querying partner list every 100ms
**Test Steps**:
1. Thread 1 starts creating partners
2. Thread 2 repeatedly queries GetPartners
3. Verify queries return consistent counts
4. Verify no partial/inconsistent data in query results

**Expected Result**: Queries always return consistent data  
**Load**: 100 creates + continuous queries

---

### TC-PM-C009: Concurrent Permission Checks
**Description**: Multiple threads checking permissions for same partner  
**Concurrency Scenario**: 50 threads check permissions simultaneously  
**Test Steps**:
1. Spawn 50 threads
2. Each calls `HasPermissionAsync(userId, 123, "Read")`
3. Verify all return correct permission result
4. Verify no database locking issues

**Expected Result**: All permission checks complete successfully  
**Load**: 50 concurrent permission checks

---

### TC-PM-C010: Concurrent Recursive Tree Traversal
**Description**: Multiple users traversing partner tree hierarchy  
**Concurrency Scenario**: 10 users traverse same tree structure  
**Test Steps**:
1. Spawn 10 threads
2. Each calls `GetChildPartnerTreesRecursively` with same parent
3. Verify all return same results
4. Monitor for N+1 query issues
5. Check database query count

**Expected Result**: All traversals return correct results efficiently  
**Load**: 10 concurrent tree traversals

---

### TC-PM-C011: Concurrent Bulk Operations
**Description**: Multiple users performing bulk operations  
**Concurrency Scenario**: 
- Thread 1: Creating 500 partners
- Thread 2: Updating 300 partners
- Thread 3: Querying partners
**Test Steps**:
1. Execute all bulk operations simultaneously
2. Monitor database performance
3. Verify data consistency
4. Check for transaction conflicts

**Expected Result**: All operations complete successfully  
**Load**: 500 creates + 300 updates + queries

---

### TC-PM-C012: Optimistic Concurrency - Update Conflict
**Description**: Test optimistic concurrency control for updates  
**Concurrency Scenario**: 2 users update same partner with stale data  
**Test Steps**:
1. User 1 reads partner 123 (version 1)
2. User 2 reads partner 123 (version 1)
3. User 1 updates partner (version 2)
4. User 2 attempts update with stale version 1
5. Verify concurrency exception thrown or handled

**Expected Result**: Second update fails with concurrency error  
**Load**: 2 conflicting updates

---

### TC-PM-C013: Concurrent Partner Group Queries
**Description**: Multiple users querying same partner group  
**Concurrency Scenario**: 30 users query partners by group simultaneously  
**Test Steps**:
1. Spawn 30 threads
2. Each calls `GetPartnersByPartnerGroup(groupId, pagination)`
3. Verify all return same data
4. Monitor query performance
5. Check for database contention

**Expected Result**: All queries return consistent results < 2s  
**Load**: 30 concurrent group queries

---

### TC-PM-C014: Create Partner During Organization Unit Update
**Description**: Creating partner while org units are being modified  
**Concurrency Scenario**: 
- Thread 1: Creating partner with org units [101,102]
- Thread 2: Modifying org unit 101 metadata
**Test Steps**:
1. Execute operations simultaneously
2. Verify partner creation succeeds
3. Verify relationships created correctly
4. Check for foreign key conflicts

**Expected Result**: Both operations succeed  
**Load**: 1 create + 1 org unit update

---

### TC-PM-C015: Concurrent Pagination Requests
**Description**: Multiple users requesting different pages simultaneously  
**Concurrency Scenario**: 100 users request random pages  
**Test Steps**:
1. Spawn 100 threads with random page numbers (1-100)
2. Execute pagination requests simultaneously
3. Verify correct records returned for each page
4. Check for pagination calculation errors
5. Monitor database connection pool

**Expected Result**: All pages return correct data  
**Load**: 100 concurrent pagination requests

---

## Edge Cases and Race Conditions

### TC-PM-E001: Null Organization Hierarchy IDs
**Description**: Handle null vs empty collection for org hierarchy  
**Test Steps**:
1. Create partner with OrganizationHierarchyIds = null
2. Create partner with OrganizationHierarchyIds = []
3. Verify both handle gracefully
4. Verify no null reference exceptions

**Expected Result**: Both create successfully without relationships  

---

### TC-PM-E002: Delete Non-Existent Organization Unit Relationship
**Description**: Update partner removing relationship that doesn't exist  
**Test Steps**:
1. Partner has org units [101, 102]
2. Update with OrganizationHierarchyIds = [101, 102] (removing 103 that doesn't exist)
3. Verify no error thrown
4. Verify relationships unchanged

**Expected Result**: Operation succeeds, no error  

---

### TC-PM-E003: Concurrent Create with Same Data
**Description**: Race condition creating identical partners  
**Test Steps**:
1. 5 threads create partner with identical data simultaneously
2. Verify database constraints handle appropriately
3. Check for duplicate entries

**Expected Result**: Handle based on uniqueness constraints  

---

### TC-PM-E004: Large Organization Unit Relationship Count
**Description**: Partner with 1000+ org unit relationships  
**Test Steps**:
1. Create partner with 1000 org units
2. Update to 1500 org units (add 500)
3. Verify differential update performs efficiently
4. Monitor memory and performance

**Expected Result**: Operations complete successfully  

---

### TC-PM-E005: Deeply Nested Partner Tree - Stack Overflow Protection
**Description**: Very deep partner tree hierarchy (50 levels)  
**Test Steps**:
1. Create 50-level deep partner tree
2. Call `GetChildPartnerTreesRecursively`
3. Verify no stack overflow
4. Verify recursion completes

**Expected Result**: Completes without stack overflow  

---


