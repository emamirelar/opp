# InteractionManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `InteractionManager`  
**Location**: `UNOPS.PAO.Business/Managers/InteractionManager.cs`  
**Purpose**: Manages interaction tracking between partners, contacts, and users including meetings, emails, calls, and visits.

**Key Responsibilities**:
- Interaction CRUD operations
- Multi-entity associations (contacts, partners, users)
- Junction table management (InteractionContacts, InteractionPartners, InteractionUsers)
- Gmail integration for email tracking
- Interaction filtering and search

---

## Functional Test Cases (30+ Cases)

### TC-IM-F001: Create Interaction - With Single Contact
**Description**: Create interaction associated with one contact  
**Test Steps**:
1. Prepare InteractionRequest with ContactIds = [101]
2. Call `CreateInteractionAsync(model)`
3. Verify interaction created
4. Verify InteractionContacts junction record created

**Expected Result**: Interaction and junction record created  

---

### TC-IM-F002: Create Interaction - With Multiple Contacts
**Description**: Create interaction with 5 contacts  
**Test Steps**:
1. Prepare InteractionRequest with ContactIds = [101, 102, 103, 104, 105]
2. Call `CreateInteractionAsync(model)`
3. Verify 5 junction records in InteractionContacts

**Expected Result**: 5 contact associations created  

---

### TC-IM-F003: Create Interaction - With Partners
**Description**: Create interaction associated with partners  
**Test Steps**:
1. Prepare InteractionRequest with PartnerIds = [201, 202]
2. Call `CreateInteractionAsync(model)`
3. Verify 2 InteractionPartners junction records created

**Expected Result**: Partner associations created  

---

### TC-IM-F004: Create Interaction - With Users
**Description**: Create interaction with internal users  
**Test Steps**:
1. Prepare InteractionRequest with UserIds = [301, 302, 303]
2. Call `CreateInteractionAsync(model)`
3. Verify 3 InteractionUsers junction records created

**Expected Result**: User associations created  

---

### TC-IM-F005: Create Interaction - All Associations
**Description**: Create interaction with contacts, partners, and users  
**Test Steps**:
1. Prepare InteractionRequest with ContactIds=[101,102], PartnerIds=[201], UserIds=[301,302]
2. Call `CreateInteractionAsync(model)`
3. Verify all junction records created (2 contacts, 1 partner, 2 users)

**Expected Result**: All associations created correctly  

---

### TC-IM-F006: Create Interaction - Invalid Contact ID
**Description**: Attempt to create interaction with non-existent contact  
**Test Steps**:
1. Prepare InteractionRequest with ContactIds = [99999]
2. Call `CreateInteractionAsync(model)`
3. Verify appropriate error handling

**Expected Result**: Error or contact skipped gracefully  

---

### TC-IM-F007: Create Interaction - Type Validation
**Description**: Create interactions of different types  
**Test Steps**:
1. Create interaction with Type = "Meeting"
2. Create interaction with Type = "Email"
3. Create interaction with Type = "Call"
4. Create interaction with Type = "Visit"
5. Verify all types saved correctly

**Expected Result**: All interaction types handled  

---

### TC-IM-F008: Get Interaction By ID
**Description**: Retrieve single interaction with associations  
**Test Steps**:
1. Call `GetInteraction(userId, interactionId)`
2. Verify interaction details returned
3. Verify associated contacts, partners, users loaded

**Expected Result**: Complete interaction data returned  

---

### TC-IM-F009: Get Interactions - Paginated List
**Description**: Retrieve interactions with pagination  
**Preconditions**: 100 interactions exist  
**Test Steps**:
1. Create PaginationRequest PageIndex=1, PageSize=20
2. Call `GetInteractions(userId, request)`
3. Verify 20 interactions returned, TotalCount=100

**Expected Result**: Paginated results returned  

---

### TC-IM-F010: Get Interactions With Specification
**Description**: Filter interactions using specification pattern  
**Test Steps**:
1. Create specification for interactions by type "Meeting"
2. Call `GetInteractionsWithSpecification(userId, specification, pagination)`
3. Verify only meetings returned

**Expected Result**: Filtered interactions returned  

---

### TC-IM-F011: Update Interaction - Basic Fields
**Description**: Update interaction details  
**Test Steps**:
1. Update interaction title, description, date
2. Call `UpdateInteractionAsync(userId, model)`
3. Verify changes persisted

**Expected Result**: Interaction updated  

---

### TC-IM-F012: Update Interaction - Add Contact
**Description**: Add contact to existing interaction  
**Preconditions**: Interaction has ContactIds=[101]  
**Test Steps**:
1. Update with ContactIds=[101, 102, 103]
2. Call `UpdateInteractionAsync(userId, model)`
3. Verify junction records: 101 preserved, 102 and 103 added

**Expected Result**: 3 total contact associations  

---

### TC-IM-F013: Update Interaction - Remove Contact
**Description**: Remove contact from interaction  
**Preconditions**: Interaction has ContactIds=[101, 102, 103]  
**Test Steps**:
1. Update with ContactIds=[101, 103]
2. Call `UpdateInteractionAsync(userId, model)`
3. Verify contact 102 junction record deleted

**Expected Result**: Contact 102 association removed  

---

### TC-IM-F014: Update Interaction - Replace All Contacts
**Description**: Completely replace contact associations  
**Preconditions**: Interaction has ContactIds=[101, 102]  
**Test Steps**:
1. Update with ContactIds=[105, 106, 107]
2. Verify old contacts removed, new ones added

**Expected Result**: Contacts replaced completely  

---

### TC-IM-F015: Update Interaction - Add Partner
**Description**: Add partner association to interaction  
**Test Steps**:
1. Update interaction adding PartnerIds=[201, 202]
2. Verify InteractionPartners junction records created

**Expected Result**: Partner associations added  

---

### TC-IM-F016: Update Interaction - Remove Partner
**Description**: Remove partner from interaction  
**Test Steps**:
1. Update removing partner from PartnerIds
2. Verify junction record deleted

**Expected Result**: Partner association removed  

---

### TC-IM-F017: Update Interaction - Add User
**Description**: Add internal user to interaction  
**Test Steps**:
1. Update adding UserIds=[301, 302]
2. Verify InteractionUsers junction records created

**Expected Result**: User associations added  

---

### TC-IM-F018: Update Interaction - Remove User
**Description**: Remove user from interaction  
**Test Steps**:
1. Update removing user from UserIds
2. Verify junction record deleted

**Expected Result**: User association removed  

---

### TC-IM-F019: Delete Interaction
**Description**: Delete interaction and handle junction records  
**Test Steps**:
1. Call `DeleteInteractionAsync(userId, interactionId)`
2. Verify interaction deleted
3. Verify junction records handled appropriately

**Expected Result**: Interaction deleted  

---

### TC-IM-F020: Get Contact Interactions
**Description**: Retrieve all interactions for specific contact  
**Preconditions**: Contact 101 has 15 interactions  
**Test Steps**:
1. Call `GetContactInteractionsAsync(101, paginationRequest)`
2. Verify 15 interactions returned
3. Verify all include contact 101

**Expected Result**: All contact interactions returned  

---

### TC-IM-F021: Find Gmail Interaction
**Description**: Find existing interaction by Gmail metadata  
**Test Steps**:
1. Prepare GmailInteractionRequest with Gmail message ID
2. Call `FindGmailInteractionAsync(model)`
3. Verify matching interaction found

**Expected Result**: Interaction found by Gmail ID  

---

### TC-IM-F022: Create Gmail Interaction
**Description**: Create interaction from Gmail email  
**Test Steps**:
1. Prepare InteractionRequest with Gmail metadata
2. Call `CreateGmailInteractionAsync(model)`
3. Verify interaction created with email type

**Expected Result**: Gmail-sourced interaction created  

---

### TC-IM-F023: Update Gmail Interaction
**Description**: Update interaction created from Gmail  
**Test Steps**:
1. Update Gmail-sourced interaction
2. Call `UpdateGmailInteractionAsync(model)`
3. Verify updates applied

**Expected Result**: Gmail interaction updated  

---

### TC-IM-F024: Transaction Rollback - Contact Creation Fails
**Description**: Verify transaction rollback on junction table error  
**Test Steps**:
1. Attempt to create interaction with invalid contact ID
2. Verify entire transaction rolled back
3. Verify no orphaned records

**Expected Result**: Clean rollback, no data corruption  

---

### TC-IM-F025: Interaction Search Fields Metadata
**Description**: Get searchable field metadata  
**Test Steps**:
1. Call `GetInteractionSearchFields()`
2. Verify returns field information for type, date, title, description

**Expected Result**: Search metadata returned  

---

### TC-IM-F026: Interaction With No Associations
**Description**: Create interaction without contacts, partners, or users  
**Test Steps**:
1. Create InteractionRequest with empty ContactIds, PartnerIds, UserIds
2. Call `CreateInteractionAsync(model)`
3. Verify interaction created without junction records

**Expected Result**: Standalone interaction created  

---

### TC-IM-F027: Get Posted Interactions
**Description**: Retrieve all posted (active) interactions  
**Test Steps**:
1. Call `GetPostedInteractions()`
2. Verify active interactions returned

**Expected Result**: Active interactions list returned  

---

### TC-IM-F028: Get Posted Interaction By ID
**Description**: Get single posted interaction  
**Test Steps**:
1. Call `GetPostedInteraction(interactionId)`
2. Verify interaction returned as ExternalInteractionModel

**Expected Result**: External model returned  

---

### TC-IM-F029: Interaction Date Range Filter
**Description**: Filter interactions by date range  
**Test Steps**:
1. Create specification for interactions between two dates
2. Verify only interactions in range returned

**Expected Result**: Date-filtered results  

---

### TC-IM-F030: Interaction With Maximum Associations
**Description**: Create interaction with many associations  
**Test Steps**:
1. Create interaction with 50 contacts, 20 partners, 10 users
2. Verify all junction records created
3. Verify performance acceptable

**Expected Result**: All associations created efficiently  

---

## Performance Test Cases

### TC-IM-P001: Create Interaction - Response Time
**Performance Criteria**: < 400ms with 5 associations  
**Test Steps**:
1. Create 100 interactions with 5 contacts each
2. Measure average creation time

**Expected Result**: Average < 400ms  

---

### TC-IM-P002: Junction Table Operations - Bulk Update
**Performance Criteria**: < 1000ms for 50 association changes  
**Test Steps**:
1. Update interaction adding 50 contacts
2. Measure update time including junction records

**Expected Result**: Update < 1000ms  

---

### TC-IM-P003: Get Interactions - Large Dataset
**Performance Criteria**: < 1000ms for paginated query on 50K interactions  
**Test Steps**:
1. Query interactions with pagination from large dataset
2. Measure query time

**Expected Result**: Query < 1000ms  

---

### TC-IM-P004: Get Contact Interactions - Heavy Load
**Performance Criteria**: < 1500ms for contact with 500 interactions  
**Test Steps**:
1. Query interactions for very active contact
2. Measure retrieval time

**Expected Result**: Query < 1500ms  

---

### TC-IM-P005: Transaction Processing Speed
**Performance Criteria**: Transaction commit < 200ms  
**Test Steps**:
1. Create interaction with transaction
2. Measure transaction commit time

**Expected Result**: Commit < 200ms  

---

### TC-IM-P006: Bulk Interaction Creation
**Performance Criteria**: > 40 interactions/second throughput  
**Test Steps**:
1. Create 1000 interactions as fast as possible
2. Calculate throughput

**Expected Result**: Throughput > 40/sec  

---

### TC-IM-P007: Complex Specification Query
**Performance Criteria**: < 2000ms for multi-filter query  
**Test Steps**:
1. Query with specification filtering by type, date range, contact, partner
2. Measure execution time

**Expected Result**: Query < 2000ms  

---

### TC-IM-P008: Junction Table Join Performance
**Performance Criteria**: < 500ms with 3 joins  
**Test Steps**:
1. Query interaction with contacts, partners, and users joined
2. Measure query time

**Expected Result**: Join query < 500ms  

---

## Concurrency Test Cases

### TC-IM-C001: Concurrent Interaction Creation
**Concurrency Scenario**: 15 threads create interactions simultaneously  
**Test Steps**:
1. Spawn 15 threads creating different interactions
2. Verify all created successfully
3. No deadlocks

**Expected Result**: All 15 created  

---

### TC-IM-C002: Concurrent Updates - Same Interaction
**Concurrency Scenario**: 3 users update same interaction  
**Test Steps**:
1. 3 threads update interaction 500 simultaneously
2. Verify consistent final state
3. Check optimistic concurrency

**Expected Result**: Consistent state, no data loss  

---

### TC-IM-C003: Concurrent Junction Table Updates
**Concurrency Scenario**: 2 threads modify contact associations  
**Test Steps**:
1. Thread 1 adds contacts [101, 102]
2. Thread 2 removes contact [103]
3. Execute simultaneously
4. Verify final state consistent

**Expected Result**: No orphaned junction records  

---

### TC-IM-C004: Concurrent Contact Association - Same Contact
**Concurrency Scenario**: 10 interactions associate same contact simultaneously  
**Test Steps**:
1. 10 threads create interactions with ContactIds=[101]
2. Verify all junction records created
3. No unique constraint violations

**Expected Result**: All 10 associations created  

---

### TC-IM-C005: Concurrent Delete and Read
**Concurrency Scenario**: Delete while reading interaction  
**Test Steps**:
1. Thread 1 deletes interaction
2. Thread 2 reads interaction during delete
3. Verify consistent result

**Expected Result**: No exceptions, consistent read  

---

### TC-IM-C006: Transaction Isolation - Concurrent Creates
**Concurrency Scenario**: 5 transactions creating interactions with same contacts  
**Test Steps**:
1. 5 threads in separate transactions
2. All create interactions with overlapping contacts
3. Verify isolation maintained

**Expected Result**: Proper transaction isolation  

---

### TC-IM-C007: Concurrent Gmail Interaction Sync
**Concurrency Scenario**: Multiple Gmail add-ons syncing interactions  
**Test Steps**:
1. 5 threads create Gmail interactions
2. Some may be duplicates based on Gmail message ID
3. Verify deduplication works

**Expected Result**: No duplicate interactions  

---

### TC-IM-C008: Bulk Create with Concurrent Queries
**Concurrency Scenario**: Creating many interactions while querying list  
**Test Steps**:
1. Thread 1 creates 200 interactions
2. Thread 2 queries interactions every 100ms
3. Verify queries return consistent data

**Expected Result**: Consistent query results  

---

### TC-IM-C009: Concurrent Partner Association Updates
**Concurrency Scenario**: 3 threads modify partner associations  
**Test Steps**:
1. 3 threads update PartnerIds for interaction 500
2. Execute simultaneously
3. Verify final state has one set of partners

**Expected Result**: Consistent partner associations  

---

### TC-IM-C010: Concurrent User Association Adds
**Concurrency Scenario**: Multiple threads adding users to interaction  
**Test Steps**:
1. 4 threads add different users to interaction 500
2. Verify all users added (union of all)
3. No duplicate junction records

**Expected Result**: All users associated once  

---

## Edge Cases

### TC-IM-E001: Empty Junction Arrays
**Description**: Create interaction with null/empty association arrays  
**Test Steps**:
1. Create with ContactIds=null, PartnerIds=[], UserIds=null
2. Verify interaction created without associations

**Expected Result**: Graceful handling of empty arrays  

---

### TC-IM-E002: Duplicate Contact IDs in Request
**Description**: ContactIds array has duplicates [101, 102, 101]  
**Test Steps**:
1. Create interaction with duplicate contact IDs
2. Verify only unique junction records created

**Expected Result**: Duplicates handled, unique records only  

---

### TC-IM-E003: Very Long Interaction Description
**Description**: Interaction with maximum length description  
**Test Steps**:
1. Create interaction with 10,000 character description
2. Verify stored and retrieved correctly

**Expected Result**: Long text handled  

---

### TC-IM-E004: Interaction at Midnight UTC
**Description**: Interaction dated exactly at midnight  
**Test Steps**:
1. Create interaction with DateTime = midnight UTC
2. Verify timezone handling correct

**Expected Result**: Timezone preserved  

---

### TC-IM-E005: Transaction Failure During Junction Update
**Description**: Simulate failure during junction table update  
**Test Steps**:
1. Force error during InteractionContacts insert
2. Verify entire transaction rolled back
3. No orphaned interaction record

**Expected Result**: Clean rollback  

---


