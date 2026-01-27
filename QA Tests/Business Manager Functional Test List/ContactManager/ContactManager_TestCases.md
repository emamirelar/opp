# ContactManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `ContactManager`  
**Location**: `UNOPS.PAO.Business/Managers/ContactManager.cs`  
**Purpose**: Manages contact operations including CRUD, partner associations, document management, and Gmail integration.

**Key Responsibilities**:
- Contact lifecycle management
- Partner-contact associations
- Contact document management
- Contact interactions tracking
- Profile picture management
- Gmail Add-on integration

---

## Functional Test Cases (30+ Cases)

### TC-CM-F001: Create Contact - Valid Data
**Description**: Create new contact with all required fields  
**Preconditions**: User has contact creation permissions, partner exists  
**Test Steps**:
1. Prepare ContactRequest with first name, last name, email, partner ID
2. Call `CreateContactAsync(model)`
3. Verify contact created with generated ID
4. Verify all fields saved correctly

**Expected Result**: Contact created successfully with ID  
**Test Data**: Contact "John Doe", email "john.doe@example.com", partner ID 123

---

### TC-CM-F002: Create Contact - Without Partner
**Description**: Create contact without partner association  
**Preconditions**: User has permissions  
**Test Steps**:
1. Create ContactRequest with PartnerId = null
2. Call `CreateContactAsync(model)`
3. Verify contact created
4. Verify PartnerId is null in database

**Expected Result**: Contact created without partner association  
**Test Data**: Contact without partner

---

### TC-CM-F003: Create Contact - All Optional Fields
**Description**: Create contact with all optional fields populated  
**Preconditions**: None  
**Test Steps**:
1. Create ContactRequest with: salutation, first name, last name, email, mobile, phone, position, address, notes
2. Call `CreateContactAsync(model)`
3. Verify all fields persisted

**Expected Result**: Contact with complete data created  
**Test Data**: Fully populated contact

---

### TC-CM-F004: Create Contact - Duplicate Email Same Partner
**Description**: Attempt to create contact with duplicate email for same partner  
**Preconditions**: Contact "john@example.com" exists for partner 123  
**Test Steps**:
1. Create ContactRequest with email "john@example.com", partner 123
2. Call `CreateContactAsync(model)`
3. Verify system handles duplicate appropriately

**Expected Result**: Based on business rules - either error or allow duplicate  
**Test Data**: Duplicate email for same partner

---

### TC-CM-F005: Create Contact - Duplicate Email Different Partner
**Description**: Create contact with same email for different partner  
**Preconditions**: Contact "john@example.com" exists for partner 123  
**Test Steps**:
1. Create ContactRequest with email "john@example.com", partner 456
2. Call `CreateContactAsync(model)`
3. Verify contact created (same person can be contact for multiple partners)

**Expected Result**: Contact created for different partner  
**Test Data**: Same email, different partner

---

### TC-CM-F006: Get Contact By ID - Exists
**Description**: Retrieve single contact by valid ID  
**Preconditions**: Contact ID 789 exists  
**Test Steps**:
1. Call `GetContact(userId, 789)`
2. Verify contact details returned
3. Verify partner association included

**Expected Result**: Returns ContactModel with complete data  
**Test Data**: Contact ID 789

---

### TC-CM-F007: Get Contact By ID - Not Found
**Description**: Attempt to retrieve non-existent contact  
**Preconditions**: Contact ID 99999 does not exist  
**Test Steps**:
1. Call `GetContact(userId, 99999)`
2. Verify returns null

**Expected Result**: Returns null  
**Test Data**: Invalid contact ID 99999

---

### TC-CM-F008: Get Contact - With Documents
**Description**: Retrieve contact with associated documents  
**Preconditions**: Contact 789 has 5 documents  
**Test Steps**:
1. Call `GetContactAsync(789)`
2. Verify contact returned
3. Verify Documents collection populated with 5 items

**Expected Result**: Contact with documents returned  
**Test Data**: Contact with documents

---

### TC-CM-F009: Get Contact - With Interactions
**Description**: Retrieve contact with interaction history  
**Preconditions**: Contact 789 has 10 interactions  
**Test Steps**:
1. Call `GetContactWithInteractionsAsync(789)`
2. Verify contact returned
3. Verify Interactions collection populated with 10 items
4. Verify interactions include partner, date, type information

**Expected Result**: Contact with full interaction history  
**Test Data**: Contact with 10 interactions

---

### TC-CM-F010: Update Contact - Basic Fields
**Description**: Update contact name and email  
**Preconditions**: Contact 789 exists  
**Test Steps**:
1. Prepare UpdateContactRequest with new first name, last name, email
2. Call `UpdateContactAsync(userId, model)`
3. Verify contact updated
4. Retrieve contact and verify changes persisted

**Expected Result**: Contact updated successfully  
**Test Data**: Updated name and email

---

### TC-CM-F011: Update Contact - Change Partner Association
**Description**: Move contact to different partner  
**Preconditions**: Contact 789 associated with partner 123  
**Test Steps**:
1. Update contact to partner 456
2. Call `UpdateContactAsync(userId, model)`
3. Verify PartnerId changed to 456
4. Verify contact appears in partner 456's contact list

**Expected Result**: Partner association updated  
**Test Data**: Change partner from 123 to 456

---

### TC-CM-F012: Update Contact - Remove Partner Association
**Description**: Disassociate contact from partner  
**Preconditions**: Contact 789 associated with partner 123  
**Test Steps**:
1. Update contact with PartnerId = null
2. Call `UpdateContactAsync(userId, model)`
3. Verify contact no longer associated with partner

**Expected Result**: Partner association removed  
**Test Data**: PartnerId set to null

---

### TC-CM-F013: Update Contact - Non-Existent
**Description**: Attempt to update non-existent contact  
**Preconditions**: Contact ID 99999 does not exist  
**Test Steps**:
1. Call `UpdateContactAsync(userId, model)` with ID 99999
2. Verify returns null or appropriate response

**Expected Result**: Returns null, no error  
**Test Data**: Invalid contact ID

---

### TC-CM-F014: Delete Contact - Soft Delete
**Description**: Soft delete existing contact  
**Preconditions**: Contact 789 exists with IsDeleted=false  
**Test Steps**:
1. Call `DeleteContactAsync(userId, 789)`
2. Query database
3. Verify IsDeleted=true
4. Verify contact not in active lists

**Expected Result**: Contact soft-deleted  
**Test Data**: Contact 789

---

### TC-CM-F015: Delete Contact - Non-Existent
**Description**: Attempt to delete non-existent contact  
**Preconditions**: Contact ID 99999 does not exist  
**Test Steps**:
1. Call `DeleteContactAsync(userId, 99999)`
2. Verify no exception thrown

**Expected Result**: Operation completes gracefully  
**Test Data**: Invalid contact ID

---

### TC-CM-F016: Delete Contact - With Interactions
**Description**: Delete contact that has interactions  
**Preconditions**: Contact 789 has 10 interactions  
**Test Steps**:
1. Call `DeleteContactAsync(userId, 789)`
2. Verify contact soft-deleted
3. Verify interactions preserved (not cascade deleted)

**Expected Result**: Contact deleted, interactions preserved  
**Test Data**: Contact with interactions

---

### TC-CM-F017: Get Partner Contacts - List All
**Description**: Retrieve all contacts for specific partner  
**Preconditions**: Partner 123 has 15 contacts  
**Test Steps**:
1. Call `GetPartnerContacts(123)`
2. Verify 15 contacts returned
3. Verify all have PartnerId = 123

**Expected Result**: Returns 15 contacts for partner 123  
**Test Data**: Partner 123 with contacts

---

### TC-CM-F018: Get Partner Contacts - Empty List
**Description**: Get contacts for partner with no contacts  
**Preconditions**: Partner 999 has no contacts  
**Test Steps**:
1. Call `GetPartnerContacts(999)`
2. Verify empty collection returned

**Expected Result**: Returns empty list  
**Test Data**: Partner with no contacts

---

### TC-CM-F019: Get Posted Contacts - All Active
**Description**: Retrieve all posted (active) contacts  
**Preconditions**: 50 posted contacts exist  
**Test Steps**:
1. Call `GetPostedContacts()`
2. Verify all active contacts returned
3. Verify deleted contacts not included

**Expected Result**: Returns all active contacts  
**Test Data**: Mix of active and deleted contacts

---

### TC-CM-F020: Get Posted Contact By ID - With Eligible Entities
**Description**: Get posted contact with eligible entities included  
**Preconditions**: Contact 789 has eligible entities defined  
**Test Steps**:
1. Call `GetPostedContact(789)`
2. Verify contact returned as ExternalContactModel
3. Verify EligibleEntities collection loaded

**Expected Result**: Contact with eligible entities returned  
**Test Data**: Contact with eligible entities

---

### TC-CM-F021: Update Contact Profile Picture - Valid Image
**Description**: Upload profile picture for contact  
**Preconditions**: Contact 789 exists, valid image file  
**Test Steps**:
1. Prepare IFormFile with valid JPG image
2. Call `UpdateContactProfilePictureAsync(789, file)`
3. Verify image saved
4. Verify URL returned (currently returns null - stub implementation)

**Expected Result**: Profile picture upload handled  
**Test Data**: Valid JPG, contact 789

---

### TC-CM-F022: Get Contacts - Specification Pattern
**Description**: Query contacts using specification pattern  
**Preconditions**: Multiple contacts with varied data  
**Test Steps**:
1. Create specification for contacts by email domain
2. Call `GetContactsWithSpecificationAsync(user, specification, pagination)`
3. Verify filtered results correct

**Expected Result**: Returns contacts matching specification  
**Test Data**: Specification filtering by email domain

---

### TC-CM-F023: Get Contacts - Pagination
**Description**: Retrieve contacts with pagination  
**Preconditions**: 100 contacts exist  
**Test Steps**:
1. Create PaginationRequest with PageIndex=1, PageSize=20
2. Call `GetContacts(userId, request)`
3. Verify 20 contacts returned
4. Verify TotalCount = 100

**Expected Result**: Returns paginated result  
**Test Data**: 100 contacts, page size 20

---

### TC-CM-F024: Get Contact By Email - Exists
**Description**: Find contact by email address  
**Preconditions**: Contact with email "john@example.com" exists  
**Test Steps**:
1. Call `GetContactByEmailAsync(user, "john@example.com")`
2. Verify contact returned
3. Verify correct contact details

**Expected Result**: Returns contact with matching email  
**Test Data**: Email "john@example.com"

---

### TC-CM-F025: Get Contact By Email - Not Found
**Description**: Search for contact with non-existent email  
**Preconditions**: No contact with email "notfound@example.com"  
**Test Steps**:
1. Call `GetContactByEmailAsync(user, "notfound@example.com")`
2. Verify returns null

**Expected Result**: Returns null  
**Test Data**: Non-existent email

---

### TC-CM-F026: Get Contact By Email - Multiple Matches
**Description**: Handle scenario where multiple contacts share email  
**Preconditions**: 2 contacts with "shared@example.com" for different partners  
**Test Steps**:
1. Call `GetContactByEmailAsync(user, "shared@example.com")`
2. Verify returns one contact (first match or based on criteria)

**Expected Result**: Returns single contact  
**Test Data**: Shared email address

---

### TC-CM-F027: Get Unmatched Emails with Partner Suggestions
**Description**: Find potential partner matches for unmatched emails  
**Preconditions**: Email addresses provided that may match existing partners  
**Test Steps**:
1. Provide list of email addresses
2. Call `GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, user)`
3. Verify partner suggestions returned for each email

**Expected Result**: Returns suggested partner matches  
**Test Data**: List of email addresses

---

### TC-CM-F028: Get Contacts for Gmail Add-on - By Email
**Description**: Retrieve contacts for Gmail Add-on integration  
**Preconditions**: Gmail Add-on requests contacts by email  
**Test Steps**:
1. Prepare GmailRelatedRecordsRequest with email addresses
2. Call `GetContactsForGmailAddon(request, user)`
3. Verify matching contacts returned

**Expected Result**: Returns contacts for Gmail integration  
**Test Data**: Email addresses from Gmail

---

### TC-CM-F029: Contact Search Fields - Get Metadata
**Description**: Retrieve searchable field metadata for contacts  
**Preconditions**: None  
**Test Steps**:
1. Call `GetContactSearchFields()`
2. Verify returns list of SearchFieldInfo
3. Verify includes fields like name, email, phone, partner

**Expected Result**: Returns search field metadata  
**Test Data**: None required

---

### TC-CM-F030: Create Contact - Missing Required Fields
**Description**: Attempt to create contact without required data  
**Preconditions**: None  
**Test Steps**:
1. Create ContactRequest with missing first name or email
2. Call `CreateContactAsync(model)`
3. Verify validation error or exception

**Expected Result**: Creation fails with validation error  
**Test Data**: Incomplete contact data

---

## Performance Test Cases

### TC-CM-P001: Create Contact - Response Time
**Description**: Measure contact creation performance  
**Performance Criteria**: < 300ms per contact  
**Test Steps**:
1. Create 100 contacts sequentially
2. Measure time for each
3. Calculate average response time

**Expected Result**: Average < 300ms  
**Load**: 100 sequential operations

---

### TC-CM-P002: Get Partner Contacts - Large Contact List
**Description**: Retrieve contacts for partner with many contacts  
**Performance Criteria**: < 1000ms for 500 contacts  
**Preconditions**: Partner has 500 contacts  
**Test Steps**:
1. Call `GetPartnerContacts(partnerId)`
2. Measure query time
3. Verify all 500 loaded

**Expected Result**: Query < 1000ms  
**Load**: 500 contacts for one partner

---

### TC-CM-P003: Get Contact With Interactions - Large History
**Description**: Load contact with extensive interaction history  
**Performance Criteria**: < 1500ms for 200 interactions  
**Preconditions**: Contact has 200 interactions  
**Test Steps**:
1. Call `GetContactWithInteractionsAsync(contactId)`
2. Measure load time
3. Verify eager loading efficient

**Expected Result**: Load < 1500ms  
**Load**: 1 contact, 200 interactions

---

### TC-CM-P004: Contact Search - Large Dataset
**Description**: Search contacts in database with 50,000 contacts  
**Performance Criteria**: < 2000ms for filtered search  
**Preconditions**: 50,000 contacts in database  
**Test Steps**:
1. Search contacts by partial name match
2. Measure query execution time
3. Verify pagination used

**Expected Result**: Search < 2000ms  
**Load**: 50,000 contacts

---

### TC-CM-P005: Bulk Contact Creation
**Description**: Create large batch of contacts  
**Performance Criteria**: > 30 contacts/second throughput  
**Test Steps**:
1. Prepare 1000 contact creation requests
2. Execute as fast as possible
3. Calculate throughput

**Expected Result**: Throughput > 30/second  
**Load**: 1000 contacts

---

### TC-CM-P006: Update Contact - With Profile Picture
**Description**: Update contact including large profile picture  
**Performance Criteria**: < 2000ms for update with 2MB image  
**Test Steps**:
1. Update contact with 2MB profile picture
2. Measure total update time
3. Verify image processing efficient

**Expected Result**: Update < 2000ms  
**Load**: 2MB image upload

---

### TC-CM-P007: Get Contacts - Pagination Performance
**Description**: Test pagination efficiency across large dataset  
**Performance Criteria**: Consistent < 500ms per page  
**Preconditions**: 10,000 contacts  
**Test Steps**:
1. Query page 1, 50, 100, 200
2. Measure each query time
3. Verify performance consistent

**Expected Result**: All pages < 500ms  
**Load**: 10,000 contacts

---

### TC-CM-P008: Contact Specification Query - Complex Filter
**Description**: Complex multi-criteria contact filter  
**Performance Criteria**: < 1500ms for complex query  
**Preconditions**: 20,000 contacts  
**Test Steps**:
1. Create specification with multiple filters (name, email, partner, date range)
2. Execute query
3. Measure time

**Expected Result**: Query < 1500ms  
**Load**: 20,000 contacts, complex filter

---

### TC-CM-P009: Delete Contact - With Many Relationships
**Description**: Delete contact with many relationships  
**Performance Criteria**: < 1000ms  
**Preconditions**: Contact has 50 interactions, 20 documents  
**Test Steps**:
1. Delete contact with many relationships
2. Measure deletion time
3. Verify cascade handling efficient

**Expected Result**: Delete < 1000ms  
**Load**: 1 contact, 70 relationships

---

### TC-CM-P010: Get Unmatched Emails - Large Batch
**Description**: Process large batch of emails for matching  
**Performance Criteria**: < 5000ms for 100 emails  
**Test Steps**:
1. Provide 100 email addresses
2. Call `GetUnmatchedEmailsWithPartnerSuggestionsAsync`
3. Measure processing time

**Expected Result**: Process < 5000ms  
**Load**: 100 email addresses

---

## Concurrency Test Cases

### TC-CM-C001: Concurrent Contact Creation - Same Partner
**Description**: Multiple users creating contacts for same partner  
**Concurrency Scenario**: 10 threads create contacts for partner 123  
**Test Steps**:
1. Spawn 10 threads
2. Each creates different contact for partner 123
3. Verify all 10 created successfully
4. Check for conflicts

**Expected Result**: All 10 contacts created  
**Load**: 10 concurrent creates

---

### TC-CM-C002: Concurrent Updates - Same Contact
**Description**: Multiple users updating same contact  
**Concurrency Scenario**: 5 users update contact 789 simultaneously  
**Test Steps**:
1. Spawn 5 threads with different updates
2. Execute simultaneously
3. Verify no data loss
4. Verify optimistic concurrency or last-write-wins

**Expected Result**: Consistent final state  
**Load**: 5 concurrent updates

---

### TC-CM-C003: Concurrent Delete and Read
**Description**: Deleting contact while reading it  
**Concurrency Scenario**: Thread 1 deletes, Thread 2 reads  
**Test Steps**:
1. Thread 1 calls DeleteContactAsync(789)
2. Thread 2 calls GetContact(789) during delete
3. Verify consistent result
4. No exceptions thrown

**Expected Result**: Consistent read result  
**Load**: 1 delete + 1 concurrent read

---

### TC-CM-C004: Concurrent Email Lookups
**Description**: Multiple threads searching by email  
**Concurrency Scenario**: 20 threads search different emails  
**Test Steps**:
1. Spawn 20 threads with different email addresses
2. Each calls GetContactByEmailAsync
3. Verify all return correct results
4. Monitor database connections

**Expected Result**: All searches complete correctly  
**Load**: 20 concurrent email searches

---

### TC-CM-C005: Concurrent Partner Contact Queries
**Description**: Multiple users querying same partner's contacts  
**Concurrency Scenario**: 15 users query partner 123 contacts  
**Test Steps**:
1. Spawn 15 threads
2. Each calls GetPartnerContacts(123)
3. Verify all return same results
4. No locking issues

**Expected Result**: Consistent results for all  
**Load**: 15 concurrent partner queries

---

### TC-CM-C006: Create Contact During Partner Update
**Description**: Creating contact while partner is being updated  
**Concurrency Scenario**: Thread 1 updates partner, Thread 2 creates contact for that partner  
**Test Steps**:
1. Thread 1 updates partner 123
2. Thread 2 creates contact for partner 123 during update
3. Verify contact created successfully
4. Verify partner relationship valid

**Expected Result**: Contact created with valid partner  
**Load**: 1 partner update + 1 contact create

---

### TC-CM-C007: Concurrent Profile Picture Updates
**Description**: Multiple users uploading profile pictures  
**Concurrency Scenario**: 3 users upload picture for contact 789  
**Test Steps**:
1. Spawn 3 threads with different images
2. Each calls UpdateContactProfilePictureAsync(789, file)
3. Verify one image wins
4. Check for file corruption

**Expected Result**: One profile picture saved  
**Load**: 3 concurrent uploads

---

### TC-CM-C008: Concurrent Specification Queries
**Description**: Multiple users running different contact queries  
**Concurrency Scenario**: 25 users run different specification queries  
**Test Steps**:
1. Spawn 25 threads with different specifications
2. Execute all simultaneously
3. Verify all return correct results
4. Monitor database performance

**Expected Result**: All queries succeed  
**Load**: 25 concurrent specification queries

---

### TC-CM-C009: Bulk Create with Concurrent Reads
**Description**: Creating many contacts while others query list  
**Concurrency Scenario**: Thread 1 creates 100 contacts, Thread 2 queries every 50ms  
**Test Steps**:
1. Thread 1 starts bulk create
2. Thread 2 repeatedly queries GetPostedContacts
3. Verify queries return consistent data
4. No partial data in results

**Expected Result**: Queries always consistent  
**Load**: 100 creates + continuous queries

---

### TC-CM-C010: Concurrent Gmail Add-on Requests
**Description**: Multiple Gmail Add-on instances requesting contact data  
**Concurrency Scenario**: 10 Gmail Add-ons query contacts simultaneously  
**Test Steps**:
1. Spawn 10 threads simulating Gmail Add-on requests
2. Each calls GetContactsForGmailAddon with different emails
3. Verify all return correct matches
4. Monitor API rate limits

**Expected Result**: All requests processed correctly  
**Load**: 10 concurrent Gmail Add-on requests

---

## Edge Cases

### TC-CM-E001: Contact With Very Long Name
**Description**: Create contact with maximum length names  
**Test Steps**:
1. Create contact with 255-character first and last names
2. Verify created successfully
3. Verify display handles long names

**Expected Result**: Long names handled correctly  

---

### TC-CM-E002: Contact With Special Characters
**Description**: Contact name with unicode and special characters  
**Test Steps**:
1. Create contact with name "François O'Néill-Smythe"
2. Verify stored and retrieved correctly
3. Search by name works

**Expected Result**: Special characters preserved  

---

### TC-CM-E003: Contact With Multiple Interactions Same Time
**Description**: Contact with multiple interactions at same timestamp  
**Test Steps**:
1. Create 5 interactions for contact with identical timestamps
2. Retrieve contact with interactions
3. Verify all 5 interactions returned

**Expected Result**: All interactions retrieved  

---

### TC-CM-E004: Null Partner Reference
**Description**: Contact with null partner ID handling  
**Test Steps**:
1. Create contact with PartnerId = null
2. Query contact
3. Verify no null reference errors

**Expected Result**: Null partner handled gracefully  

---

### TC-CM-E005: Orphaned Contact - Partner Deleted
**Description**: Contact references deleted partner  
**Test Steps**:
1. Create contact for partner 123
2. Soft delete partner 123
3. Query contact
4. Verify partner relationship handled

**Expected Result**: Contact accessible, partner null or marked deleted  

---


