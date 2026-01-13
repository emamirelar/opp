# PartnerManager - Business Logic Test Cases

## Manager Overview
**Manager**: `PartnerManager` / `UNOPSPartnerManager`  
**Location**: `UNOPS.PAO.Business/Managers/PartnerManager.cs`, `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSPartnerManager.cs`  
**Purpose**: Manages partner organizations including CRUD, approval workflows, organization unit relationships, and ERP integration.

## Key Business Rules (From PRD)

1. **Partner Approval Workflow**: Partners must go through approval process before they can be used in opportunities
2. **ERP Dim Value Assignment**: Approved partners receive unique ERP dimension values (1-7999 or 8000-9999 reserved range)
3. **Organization Unit Relationships**: Partners can be associated with multiple organization units (OrgUnits only)
4. **Partner Status Lifecycle**: Draft → Active → Closed → Archived
5. **Partner Categories**: Partners belong to partner groups within categories via PartnerTree
6. **Smart Search**: Search across partner and all related entities (contacts, interactions)

---

## P0 - Critical Business Logic Tests

### TC-PM-BL-P0-001: Partner Approval - Valid Workflow
**Priority**: P0 - Critical  
**Description**: Verify partner approval process assigns ERP Dim Value correctly  
**Business Rule**: Approved partners must receive a unique ERP Dim Value  
**Preconditions**: 
- Partner exists with status Draft/Active
- Partner has PartnerGroupId and LiaisonOfficeId set
- User has approval permissions

**Test Steps**:
1. Create partner with required fields (Name, PartnerGroupId, LiaisonOfficeId)
2. Call `ApprovePartnerAsync(user, partnerId, request)`
3. Verify PartnerApprovalStatus changed to "Approved"
4. Verify ErpDimValue assigned (unique, in valid range)
5. Verify PartnerApprovalDate set to current date

**Expected Result**: Partner approved with unique ERP Dim Value  
**Business Impact**: Without ERP Dim Value, partners cannot be used in financial transactions

---

### TC-PM-BL-P0-002: Partner Approval - ERP Dim Value Uniqueness
**Priority**: P0 - Critical  
**Description**: Verify ERP Dim Values are never duplicated  
**Business Rule**: Each approved partner must have a unique ErpDimValue  
**Preconditions**: 5 partners exist with ErpDimValues 1001-1005

**Test Steps**:
1. Approve new partner
2. Verify assigned ErpDimValue is NOT 1001-1005
3. Approve another partner
4. Verify new ErpDimValue is different from all existing

**Expected Result**: All ERP Dim Values are unique  
**Business Impact**: Duplicate values would cause financial data corruption

---

### TC-PM-BL-P0-003: Partner Approval - Reserved ERP Range
**Priority**: P0 - Critical  
**Description**: Verify reserved ERP range (8000-9999) handling  
**Business Rule**: ERP values 8000-9999 are reserved for special partners  
**Preconditions**: Partner in reserved range exists (ErpDimValue = 8500)

**Test Steps**:
1. Approve new regular partner
2. Verify assigned ErpDimValue is NOT in 8000-9999 range
3. Verify reserved partners retain their special values

**Expected Result**: Regular partners get values outside reserved range  
**Business Impact**: Reserved range conflicts could affect special partner accounting

---

### TC-PM-BL-P0-004: Partner Unapproval - Remove ERP Dim Value
**Priority**: P0 - Critical  
**Description**: Verify unapproval removes ERP integration  
**Business Rule**: Unapproved partners should not have active ERP integration  
**Preconditions**: Approved partner with ErpDimValue exists

**Test Steps**:
1. Call `UnapprovePartnerAsync(user, partnerId, request)`
2. Verify PartnerApprovalStatus changed to "NotApproved"
3. Verify ErpDimValue handling (may retain value but disable)
4. Verify CanCreateNewOpportunities set to false

**Expected Result**: Partner unapproved and cannot create opportunities  
**Business Impact**: Prevents financial transactions with unapproved partners

---

### TC-PM-BL-P0-005: Partner Status - Draft to Active Transition
**Priority**: P0 - Critical  
**Description**: Verify status transition from Draft to Active  
**Business Rule**: Partners can only be activated with required fields complete  
**Preconditions**: Partner in Draft status with all required fields

**Test Steps**:
1. Call `ActivatePartnerAsync(user, partnerId, request)`
2. Verify Status changed from Draft to Active
3. Verify all required fields validated
4. Verify partner appears in active partner lists

**Expected Result**: Partner successfully activated  
**Business Impact**: Draft partners should not be visible to end users

---

### TC-PM-BL-P0-006: Partner Status - Active to Closed Transition
**Priority**: P0 - Critical  
**Description**: Verify closing an active partner  
**Business Rule**: Closed partners cannot be used for new opportunities  
**Preconditions**: Active partner with existing relationships

**Test Steps**:
1. Call `ClosePartnerAsync(user, partnerId, request)`
2. Verify Status changed to Closed
3. Verify existing relationships preserved
4. Verify partner no longer appears in selection lists

**Expected Result**: Partner closed but data preserved  
**Business Impact**: Historical data integrity must be maintained

---

### TC-PM-BL-P0-007: Organization Unit Relationship - Create with OrgUnit
**Priority**: P0 - Critical  
**Description**: Verify partners can only be associated with OrgUnit type  
**Business Rule**: OrganizationUnitRelationships must be OrgUnit type, not Hub or Region  
**Preconditions**: 
- OrgUnit hierarchy exists (Region → Hub → OrgUnit)
- Partner exists

**Test Steps**:
1. Create partner with OrganizationHierarchyIds containing OrgUnit IDs
2. Verify relationships created successfully
3. Attempt to create with Hub ID - should fail/be rejected
4. Attempt to create with Region ID - should fail/be rejected

**Expected Result**: Only OrgUnit relationships allowed  
**Business Impact**: Ensures proper organizational alignment

---

### TC-PM-BL-P0-008: Organization Unit Relationship - Differential Update
**Priority**: P0 - Critical  
**Description**: Verify org unit updates only add/remove changed relationships  
**Business Rule**: Efficient update - don't recreate unchanged relationships  
**Preconditions**: Partner with OrgUnit IDs [1, 2, 3]

**Test Steps**:
1. Update partner with OrgUnit IDs [2, 3, 4]
2. Verify OrgUnit 1 relationship removed
3. Verify OrgUnit 2, 3 relationships unchanged
4. Verify OrgUnit 4 relationship added

**Expected Result**: Only differential changes applied  
**Business Impact**: Performance and audit trail integrity

---

### TC-PM-BL-P0-009: Partner Delete - Soft Delete Verification
**Priority**: P0 - Critical  
**Description**: Verify partners are soft-deleted, not hard-deleted  
**Business Rule**: Partners must retain historical data via soft delete  
**Preconditions**: Partner with contacts and interactions

**Test Steps**:
1. Call `DeletePartnerAsync(userId, partnerId)`
2. Verify IsDeleted = true in database
3. Verify partner not in active partner lists
4. Verify contacts still accessible (orphaned or also soft-deleted)

**Expected Result**: Partner soft-deleted, data preserved  
**Business Impact**: Regulatory compliance and data recovery

---

### TC-PM-BL-P0-010: Permission Check - Read vs Update Access
**Priority**: P0 - Critical  
**Description**: Verify permission-based access control  
**Business Rule**: Different operations require different permissions  
**Preconditions**: Partner exists, users with different roles

**Test Steps**:
1. User with Read permission - verify can call GetPartnerAsync
2. User with Read permission - verify cannot call UpdatePartnerAsync
3. Admin user - verify can perform all operations
4. Creator user - verify can modify own partner

**Expected Result**: Permissions correctly enforced  
**Business Impact**: Data security and access control

---

### TC-PM-BL-P0-011: Partner Group Assignment - Category Validation
**Priority**: P0 - Critical  
**Description**: Verify partner group must have valid category  
**Business Rule**: PartnerGroupId must reference valid PartnerTree with category  
**Preconditions**: PartnerTree categories exist

**Test Steps**:
1. Create partner with valid PartnerGroupId
2. Verify partner created with correct category association
3. Attempt with invalid PartnerGroupId - should fail

**Expected Result**: Valid category assignment required  
**Business Impact**: Proper partner classification for reporting

---

### TC-PM-BL-P0-012: Partner Approval - Missing Required Fields
**Priority**: P0 - Critical  
**Description**: Verify approval fails without required fields  
**Business Rule**: Cannot approve partner without PartnerGroupId and LiaisonOfficeId  
**Preconditions**: Partner without PartnerGroupId

**Test Steps**:
1. Attempt to approve partner without PartnerGroupId
2. Verify BusinessException thrown
3. Attempt to approve without LiaisonOfficeId
4. Verify BusinessException thrown

**Expected Result**: Approval blocked with clear error message  
**Business Impact**: Ensures data completeness before ERP integration

---

### TC-PM-BL-P0-013: Partner With Contacts - Cascade Behavior
**Priority**: P0 - Critical  
**Description**: Verify contact handling when partner status changes  
**Business Rule**: Contact access should follow partner visibility  
**Preconditions**: Partner with 10 contacts

**Test Steps**:
1. Close partner
2. Verify contacts still accessible for historical queries
3. Verify contacts not shown in active contact searches
4. Archive partner
5. Verify contact archive behavior

**Expected Result**: Proper cascade of status changes  
**Business Impact**: Data consistency across related entities

---

### TC-PM-BL-P0-014: Partner Search - OrgUnit Filtering
**Priority**: P0 - Critical  
**Description**: Verify partners filtered by user's organization unit  
**Business Rule**: Users should only see partners in their org unit hierarchy  
**Preconditions**: Partners in different org units, user in specific org unit

**Test Steps**:
1. Query partners as user in OrgUnit A
2. Verify only partners with OrgUnit A relationships returned
3. Verify partners in OrgUnit B not visible
4. Admin user should see all partners

**Expected Result**: Proper org unit filtering  
**Business Impact**: Multi-tenant security and data isolation

---

### TC-PM-BL-P0-015: Partner Logo Upload - File Validation
**Priority**: P0 - Critical  
**Description**: Verify logo upload validates file type and size  
**Business Rule**: Only valid image files under size limit accepted  
**Preconditions**: Partner exists

**Test Steps**:
1. Upload valid JPG logo - should succeed
2. Upload valid PNG logo - should succeed
3. Upload invalid file type (PDF) - should fail
4. Upload oversized file - should fail

**Expected Result**: File validation enforced  
**Business Impact**: Security and storage management

---

## P1 - High Priority Business Logic Tests

### TC-PM-BL-P1-001: Partner Query - Include Contacts and Interactions
**Priority**: P1 - High  
**Description**: Verify eager loading of related entities  
**Business Rule**: GetPartnerWithContactsAndInteractionsAsync includes all related data  
**Preconditions**: Partner with 5 contacts, each with 3 interactions

**Test Steps**:
1. Call `GetPartnerWithContactsAndInteractionsAsync(partnerId)`
2. Verify 5 contacts loaded
3. Verify each contact has 3 interactions
4. Verify no N+1 query problem

**Expected Result**: All related data loaded efficiently  
**Business Impact**: Performance and user experience

---

### TC-PM-BL-P1-002: Partner by Partner Group - Filtering
**Priority**: P1 - High  
**Description**: Verify filtering partners by partner group  
**Business Rule**: GetPartnersByPartnerGroup returns only matching partners  
**Preconditions**: 3 partner groups with 10 partners each

**Test Steps**:
1. Call `GetPartnersByPartnerGroup(userId, partnerGroupId, request)`
2. Verify only partners in that group returned
3. Verify pagination works correctly
4. Verify count reflects filtered results

**Expected Result**: Correct filtering by partner group  
**Business Impact**: Accurate reporting and filtering

---

### TC-PM-BL-P1-003: Partner by Category - Filtering
**Priority**: P1 - High  
**Description**: Verify filtering partners by category code  
**Business Rule**: GetPartnersByPartnerCategory includes partners in all groups of category  
**Preconditions**: Category with 3 partner groups, each with 5 partners

**Test Steps**:
1. Call `GetPartnersByPartnerCategory(userId, categoryCode, request)`
2. Verify all 15 partners returned (3 groups × 5 partners)
3. Verify partners from other categories not included

**Expected Result**: All partners in category returned  
**Business Impact**: Category-level reporting

---

### TC-PM-BL-P1-004: Partner Tree Recursion - Child Nodes
**Priority**: P1 - High  
**Description**: Verify recursive child partner tree retrieval  
**Business Rule**: GetChildPartnerTreesRecursively returns all descendants  
**Preconditions**: Partner tree with 3-level hierarchy

**Test Steps**:
1. Call `GetChildPartnerTreesRecursively(["ROOT_CODE"])`
2. Verify all child and grandchild nodes returned
3. Verify parent node not in result
4. Verify no duplicates in result

**Expected Result**: All descendant nodes returned  
**Business Impact**: Correct hierarchy navigation

---

### TC-PM-BL-P1-005: Partner Specification - Complex Filter
**Priority**: P1 - High  
**Description**: Verify specification pattern with multiple criteria  
**Business Rule**: Specifications compose correctly for complex queries  
**Preconditions**: Diverse partner data

**Test Steps**:
1. Create specification with: status filter, name contains, category filter
2. Call `GetPartnersWithSpecification(userId, specification, pagination)`
3. Verify all criteria applied correctly
4. Verify pagination works with filtered results

**Expected Result**: Complex filter works correctly  
**Business Impact**: Advanced search functionality

---

### TC-PM-BL-P1-006: Smart Search - Cross-Entity Search
**Priority**: P1 - High  
**Description**: Verify smart search finds partners by contact/interaction data  
**Business Rule**: Search should find partners even when search term matches contact  
**Preconditions**: Partner "ABC Corp" with contact "John Smith"

**Test Steps**:
1. Search for "John Smith"
2. Verify "ABC Corp" returned (matched via contact)
3. Search for "ABC Corp"
4. Verify partner returned (direct match)

**Expected Result**: Partners found via related entity matches  
**Business Impact**: Comprehensive search experience

---

### TC-PM-BL-P1-007: Partner Update - Concurrent Modification
**Priority**: P1 - High  
**Description**: Verify handling of concurrent updates  
**Business Rule**: Optimistic concurrency or last-write-wins behavior  
**Preconditions**: Partner exists

**Test Steps**:
1. User A reads partner
2. User B reads same partner
3. User A updates partner
4. User B attempts update with stale data
5. Verify consistent final state

**Expected Result**: Consistent data without corruption  
**Business Impact**: Data integrity in multi-user environment

---

### TC-PM-BL-P1-008: Partner Query - Sorting Options
**Priority**: P1 - High  
**Description**: Verify partners can be sorted by various fields  
**Business Rule**: OrderBy in PaginationRequest applies correctly  
**Preconditions**: 50 partners with varied data

**Test Steps**:
1. Query with OrderBy = "Name", Ascending = true
2. Verify alphabetical order A-Z
3. Query with OrderBy = "Name", Ascending = false
4. Verify reverse order Z-A
5. Query with OrderBy = "CreatedDate"

**Expected Result**: Correct sorting applied  
**Business Impact**: User experience and usability

---

### TC-PM-BL-P1-009: Partner Status - Archive Validation
**Priority**: P1 - High  
**Description**: Verify archiving requirements  
**Business Rule**: Only closed partners can be archived  
**Preconditions**: Partner in Active status

**Test Steps**:
1. Attempt to archive Active partner - should fail
2. Close the partner first
3. Archive closed partner - should succeed
4. Verify status is Archived

**Expected Result**: Archive only from Closed status  
**Business Impact**: Proper lifecycle management

---

### TC-PM-BL-P1-010: Partner Creation - Audit Fields
**Priority**: P1 - High  
**Description**: Verify audit fields populated on creation  
**Business Rule**: CreatedBy, CreatedDate set automatically  
**Preconditions**: User authenticated

**Test Steps**:
1. Create new partner
2. Verify CreatedBy = current user ID
3. Verify CreatedDate = current timestamp
4. Verify LastModifiedBy/Date set

**Expected Result**: Audit trail created  
**Business Impact**: Accountability and compliance

---

### TC-PM-BL-P1-011: Partner Update - Audit Fields
**Priority**: P1 - High  
**Description**: Verify audit fields updated on modification  
**Business Rule**: LastModifiedBy, LastModifiedDate updated on changes  
**Preconditions**: Partner exists, different user makes changes

**Test Steps**:
1. Update partner as User B
2. Verify LastModifiedBy = User B ID
3. Verify LastModifiedDate updated
4. Verify CreatedBy unchanged (User A)

**Expected Result**: Modification audit trail  
**Business Impact**: Change tracking

---

### TC-PM-BL-P1-012: Partner Gmail Integration - Related Records
**Priority**: P1 - High  
**Description**: Verify Gmail Add-on partner lookup  
**Business Rule**: Find partners related to email addresses  
**Preconditions**: Partners with contacts having specific emails

**Test Steps**:
1. Call `GetPartnersForGmailAddon(request, user)` with email addresses
2. Verify partners with matching contacts returned
3. Verify correct partner-contact associations

**Expected Result**: Partners found via email matching  
**Business Impact**: Gmail Add-on functionality

---

### TC-PM-BL-P1-013: Partner by Name - Exact Match
**Priority**: P1 - High  
**Description**: Verify partner lookup by exact name  
**Business Rule**: GetPartnerByNameAsync returns exact match  
**Preconditions**: Partner "Test Corporation" exists

**Test Steps**:
1. Call `GetPartnerByNameAsync(user, "Test Corporation")`
2. Verify exact match returned
3. Call with "Test Corp" - should not match
4. Verify case sensitivity behavior

**Expected Result**: Exact name matching  
**Business Impact**: Duplicate detection and lookup

---

### TC-PM-BL-P1-014: Partner Query - Exclude Deleted
**Priority**: P1 - High  
**Description**: Verify deleted partners excluded by default  
**Business Rule**: Standard queries exclude IsDeleted=true partners  
**Preconditions**: Mix of active and deleted partners

**Test Steps**:
1. Query partners with standard GetPartners
2. Verify deleted partners not in results
3. Verify total count excludes deleted

**Expected Result**: Deleted partners filtered out  
**Business Impact**: User experience and data cleanliness

---

### TC-PM-BL-P1-015: Partner Total Count - Debug Method
**Priority**: P1 - High  
**Description**: Verify total partner count calculation  
**Business Rule**: GetTotalPartnerCountAsync respects user permissions  
**Preconditions**: 100 partners, user with limited access

**Test Steps**:
1. Call `GetTotalPartnerCountAsync(user)`
2. Verify count reflects user's visible partners
3. Admin should see all partners

**Expected Result**: Correct filtered count  
**Business Impact**: Dashboard accuracy

---

## P2 - Medium Priority Business Logic Tests

### TC-PM-BL-P2-001: Partner Pagination - First Page
**Priority**: P2 - Medium  
**Description**: Verify first page returns correct data  
**Preconditions**: 100 partners

**Test Steps**:
1. Request PageIndex=1, PageSize=10
2. Verify 10 records returned
3. Verify TotalCount=100

**Expected Result**: First 10 partners returned

---

### TC-PM-BL-P2-002: Partner Pagination - Last Page Partial
**Priority**: P2 - Medium  
**Description**: Verify last page with partial results  
**Preconditions**: 95 partners, PageSize=10

**Test Steps**:
1. Request PageIndex=10, PageSize=10
2. Verify 5 records returned (95 total, last page partial)

**Expected Result**: Correct partial page

---

### TC-PM-BL-P2-003: Partner Creation - Special Characters in Name
**Priority**: P2 - Medium  
**Description**: Verify special characters handled in name  
**Test Steps**:
1. Create partner with name "L'Oréal S.A. & Co."
2. Verify stored correctly
3. Search by name works

**Expected Result**: Special characters preserved

---

### TC-PM-BL-P2-004: Partner Logo - Update Existing
**Priority**: P2 - Medium  
**Description**: Verify logo replacement behavior  
**Preconditions**: Partner with existing logo

**Test Steps**:
1. Upload new logo
2. Verify old logo replaced
3. Verify new URL returned

**Expected Result**: Logo replaced successfully

---

### TC-PM-BL-P2-005: Partner Short Description - Validation
**Priority**: P2 - Medium  
**Description**: Verify short description length limits  
**Test Steps**:
1. Create partner with max length description
2. Verify accepted
3. Exceed limit - verify validation error

**Expected Result**: Length validation enforced

---

## P3 - Low Priority Edge Cases

### TC-PM-BL-P3-001: Partner with No Contacts
**Priority**: P3 - Low  
**Description**: Verify partner without contacts works correctly  
**Test Steps**:
1. Create partner without contacts
2. Query with contacts include
3. Verify empty contacts list, no error

**Expected Result**: Empty contacts handled gracefully

---

### TC-PM-BL-P3-002: Partner Tree - Circular Reference Prevention
**Priority**: P3 - Low  
**Description**: Verify system handles circular references  
**Test Steps**:
1. Attempt to create circular partner tree
2. Verify prevented or handled gracefully

**Expected Result**: No infinite loops

---

### TC-PM-BL-P3-003: Partner Query - Empty Database
**Priority**: P3 - Low  
**Description**: Verify queries work on empty database  
**Test Steps**:
1. Query partners with no data
2. Verify empty result, no error
3. Verify TotalCount = 0

**Expected Result**: Empty results handled correctly

---

## Integration with Unit Tests

These business logic test cases should be implemented as actual unit tests in:
`tests/UNOPS.PAO.Business.Tests/Managers/PartnerManagerBusinessLogicTests.cs`

### Test Implementation Guidance

```csharp
// Example test structure
[Fact]
public async Task ApprovePartner_WithValidData_AssignsUniqueErpDimValue()
{
    // Arrange: Create partner with required fields
    // Act: Call ApprovePartnerAsync
    // Assert: Verify ErpDimValue assigned and unique
}
```

---

## Related Documentation

- [Partner Entity](../../UNOPS.PAO.Domain/Entities/Partner.cs)
- [Partner Approval Status Enum](../../UNOPS.PAO.Domain/Enums/PartnerApprovalStatus.cs)
- [Organization Hierarchy](../../UNOPS.PAO.Domain/Entities/OrganizationHierarchy.cs)
- [Existing Test Cases](../Business/PartnerManager/PartnerManager_TestCases.md)

